// http://makezine.com/projects/make-35/my-franken-keepon/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
using DailyCoding.EasyTimer;
using System.ComponentModel;



namespace Manny.Keepon
{
    class Keepon
    {
        KeeponMotorTarget motorTarget;
        IDisposable targetTimeout;
        Queue<KeeponMotorTarget> movementQueue = new Queue<KeeponMotorTarget>();
        Stream serialPortBaseStream;
        BackgroundWorker serialReader = new BackgroundWorker();

        string serialBuffer = "";

        KeeponPanEncoder? lastPanEncoder = null; // NOREACH, FORWARD, BACK, UP
        KeeponPonEncoder? lastPonEncoder = null; // HALFDOWN, UP, DOWN, HALFUP
        KeeponSideEncoder? lastSideEncoder = null; // CENTER, RIGHT, LEFT
        KeeponTiltEncoder? lastTiltEncoder = null; // BACK, RIGHT, LEFT, CENTER

        int max_pan = 30; // really 100
        int min_pan = -30; // really -100


        public bool Moving
        {
            get
            {
                return !(movementQueue.Count == 0 && (motorTarget == null || motorTarget.Complete));
            }
        }

        #region events

        public class KeeponButtonEventArgs : EventArgs
        {
            private bool _pressed = true;
            private KeeponButton _button;

            public KeeponButtonEventArgs(KeeponButton button, bool pressed)
            {
                _button = button;
                _pressed = pressed;
            }

            public KeeponButton Button { get { return _button;  } }
            public bool Pressed { get { return _pressed; } }
            public bool Released { get { return !_pressed; } }
        }

        public event EventHandler<KeeponButtonEventArgs> ButtonPressed;

        private void OnButtonPressed(KeeponButtonEventArgs e)
        {
            if (ButtonPressed != null) ButtonPressed(this, e);
        }

        #endregion events







        public void Disconnect()
        {
            serialPortBaseStream = null;
        }

        public void Connect(Stream _serialStream)
        {
            serialPortBaseStream = _serialStream;

            serialReader.DoWork += serialReader_DoWork;
            serialReader.RunWorkerAsync();
        }








        #region serial

        private void serialReader_DoWork(object sender, DoWorkEventArgs e)
        {
            int byteIn = 0;
            while ((byteIn = serialPortBaseStream.ReadByte()) != -1)
            {
                char character = (char)byteIn;
                if (character == '\n')
                {
                    parseSerialInput(serialBuffer);
                    serialBuffer = "";
                }
                else
                {
                    serialBuffer += character;
                }
            }
        }

        private void parseSerialInput(string command)
        {
            command = command.Trim();
            if (command.StartsWith("AUDIO")) return;

            string[] words = command.Split(' ');
            // BUTTON [DANCE, TOUCH] [OFF, ON]
            // BUTTON [HEAD, FRONT, BACK, RIGHT, LEFT] [OFF, ON]
            // MOTOR [PAN, TILT, SIDE, PON] FINISHED
            // MOTOR [PAN, TILT, SIDE, PON] STALLED
            // ENCODER TILT [NOREACH, FORWARD, BACK, UP]
            // ENCODER PON [HALFDOWN, UP, DOWN, HALFUP]
            // ENCODER SIDE [CENTER, RIGHT, LEFT]
            // ENCODER PAN [BACK, RIGHT, LEFT, CENTER]
            // EMF [PAN, TILT, PONSIDE] [-127...127]
            // POSITION [PAN, TILT, PONSIDE] [VAL]
            // AUDIO TEMPO [67, 80, 100, 133, 200]
            // AUDIO MEAN [0...64]
            // AUDIO RANGE [0...64]
            // AUDIO ENVELOPE [0...127]
            // AUDIO BPM [VAL]

            if (words[0] == "BUTTON")
            {
                Debug.WriteLine("<keepon> read ", command);

                KeeponButton button;

                if(Enum.TryParse<KeeponButton>(words[1], true, out button))
                {
                    bool? pressed = null;
                    if(words[2] == "ON") pressed = true;
                    else pressed = false;

                    if(pressed.HasValue)
                    {
                        OnButtonPressed(new KeeponButtonEventArgs(button, (bool) pressed));
                    }
                }
            }
            else if (words[0] == "MOTOR")
            {
                Debug.WriteLine("<keepon> read ", command);
                KeeponMovementType movementType;
                KeeponMotorState motorState;
                if (motorTarget != null
                    && Enum.TryParse<KeeponMovementType>(words[1], out movementType)
                    && Enum.TryParse<KeeponMotorState>(words[2], out motorState)
                    )
                {
                    if (movementType == KeeponMovementType.Pan) motorTarget.PanState = motorState;
                    else if (movementType == KeeponMovementType.Tilt) motorTarget.TiltState = motorState;
                    else if (movementType == KeeponMovementType.Side) motorTarget.SideState = motorState;
                    else if (movementType == KeeponMovementType.Pon) motorTarget.PonState = motorState;

                    tryAdvanceMovement();
                }
                // words[1] is PAN/ TILT / SIDE / PON
                // words[2] is FINISHED/STALLED
            }
            else if (words[0] == "ENCODER")
            {
                if (words[1] == "TILT")
                {
                    KeeponTiltEncoder encoderValue;
                    if(Enum.TryParse<KeeponTiltEncoder>(words[2], out encoderValue)) lastTiltEncoder = encoderValue;
                } 
                if (words[1] == "PON")
                {
                    KeeponPonEncoder encoderValue;
                    if (Enum.TryParse<KeeponPonEncoder>(words[2], out encoderValue)) lastPonEncoder = encoderValue;
                } 
                if (words[1] == "SIDE")
                {
                    KeeponSideEncoder encoderValue;
                    if (Enum.TryParse<KeeponSideEncoder>(words[2], out encoderValue)) lastSideEncoder = encoderValue;
                } 
                if (words[1] == "PAN")
                {
                    KeeponPanEncoder encoderValue;
                    if (Enum.TryParse<KeeponPanEncoder>(words[2], out encoderValue)) lastPanEncoder = encoderValue;
                } 
            }

        }


        private bool trySendSerial(string toSend)
        {
            bool written = false;
            if (serialPortBaseStream != null)
            {
                Debug.WriteLine("Writing " + toSend);
                //serialStream.Write(toSend);
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(toSend);
                serialPortBaseStream.Write(bytes, 0, bytes.Length);
                written = true;
            }
            return written;
        }

        #endregion serial


        /// <summary>
        /// Enqueue a position to move to
        /// </summary>
        /// <param name="motorTarget"></param>
        public void NextMove(KeeponMotorTarget motorTarget)
        {
            movementQueue.Enqueue(motorTarget);
            tryAdvanceMovement();
        }

        public void NextMove(params KeeponMotorTarget[] motorTargets)
        {
            foreach (KeeponMotorTarget motorTarget in motorTargets)
            {
                movementQueue.Enqueue(motorTarget);
            }

            tryAdvanceMovement();
        }

        /// <summary>
        /// Clear any queued moves and move directly to the supplied target
        /// </summary>
        /// <param name="motorTarget"></param>
        public void Move(KeeponMotorTarget motorTarget)
        {
            clearMovements();
            NextMove(motorTarget);
        }

        public void Move(params KeeponMotorTarget[] motorTargets)
        {
            clearMovements();
            foreach (KeeponMotorTarget motorTarget in motorTargets)
            {
                movementQueue.Enqueue(motorTarget);
            }

            tryAdvanceMovement();
        }

        public void Stop()
        {
            clearMovements();
            trySendSerial("MOVE STOP;");
        }
         

        private void clearMovements()
        {
            movementQueue.Clear();
            if (targetTimeout != null) targetTimeout.Dispose();
            motorTarget = null;
        }






        private void tryAdvanceMovement()
        {
            if (motorTarget != null && !motorTarget.Complete) return;
            if (movementQueue.Count == 0) return;

            motorTarget = movementQueue.Dequeue();

            if (motorTarget.PanTarget.HasValue)
            {
                Debug.WriteLine("maybe send pan?");
                sendMovePan((int) motorTarget.PanTarget, motorTarget.PanSpeed);
            }

            if (motorTarget.TiltTarget.HasValue)
            {
                Debug.WriteLine("maybe send tilt?");
                sendMoveTilt((int)motorTarget.TiltTarget, motorTarget.TiltSpeed);
            }

            if (motorTarget.SideTarget.HasValue)
            {
                sendMoveSide((KeeponSideTarget)motorTarget.SideTarget, motorTarget.PonSideSpeed);
            }
            else if (motorTarget.PonTarget.HasValue)
            {
                sendMovePon((KeeponPonTarget)motorTarget.PonTarget, motorTarget.PonSideSpeed);
            }

            if (targetTimeout != null) targetTimeout.Dispose();
            motorTarget.Started = DateTime.Now;
            targetTimeout = EasyTimer.SetTimeout(() =>
            {
                Debug.WriteLine("<keepon> motorTarget timeout");
                tryAdvanceMovement();
            }, motorTarget.Timeout);
            

        }

        private string speedCommand(KeeponMotorType motor, int? speed)
        {
            if (!speed.HasValue) return "";
            else
            {
                int speedInt = clamp((int) speed, 0, 255);
                string motorName = Enum.GetName(typeof(KeeponMotorType), motor).ToUpper();
                //int serialSpeed = toRange255((double)speed);
                return "SPEED " + motorName + " " + speedInt + ";";
            }
        }

        private string speedCommand(KeeponMotorType motor, double? speed)
        {
            if (!speed.HasValue) return "";
            else
            {
                int speedInt = toRange255((double)speed);
                return speedCommand(motor, speed);
            }
        }


        private void sendMovePan(int position, int? speed = null)
        {
            position = clamp(position, min_pan, max_pan);
            sendMove(KeeponMovementType.Pan, position.ToString(), speed);
        }

        private void sendMoveTilt(int position, int? speed = null)
        {
            position = clamp(position, -100, 100);
            sendMove(KeeponMovementType.Tilt, position.ToString(), speed);
        }

        private void sendMoveSide(KeeponSideTarget position, int? speed = null)
        {
            KeeponSideMovement? movement = null;
            if (position == KeeponSideTarget.Left) movement = KeeponSideMovement.Left;
            else if (position == KeeponSideTarget.Right) movement = KeeponSideMovement.Right;
            else if (position == KeeponSideTarget.Center)
            {
                if(lastSideEncoder.HasValue)
                {
                    if (lastSideEncoder == KeeponSideEncoder.Left) movement = KeeponSideMovement.CenterFromLeft;
                    else if (lastSideEncoder == KeeponSideEncoder.Right) movement = KeeponSideMovement.CenterFromRight;
                    else movement = null;
                }
                else
                {
                    movement = KeeponSideMovement.CenterFromRight;
                }
            }

            //if(lastSideEncoder


            if (!movement.HasValue) return;

            string pos = Enum.GetName(typeof(KeeponSideMovement), (KeeponSideMovement) movement).ToUpper();
            sendMove(KeeponMovementType.Side, pos, speed);
        }

        private void sendMovePon(KeeponPonTarget position, int? speed = null)
        {
            if (position == KeeponPonTarget.None) return;
            string pos = Enum.GetName(typeof(KeeponPonTarget), position).ToUpper();
            sendMove(KeeponMovementType.Pon, pos, speed);
        }

        private void sendMove(KeeponMovementType movementType, string position, int? speed = null)
        {
            KeeponMotorType motorType;
            
            switch(movementType)
            {
                case KeeponMovementType.Pan:
                    motorType = KeeponMotorType.Pan;
                    break;
                case KeeponMovementType.Tilt:
                    motorType = KeeponMotorType.Tilt;
                    break;
                default:
                    motorType = KeeponMotorType.PonSide;
                    break;
            }

            string command = speedCommand(motorType, speed);
            string movementString = Enum.GetName(typeof(KeeponMovementType), movementType).ToUpper();
            command += "MOVE " + movementString + " " + position + ";";

            trySendSerial(command);
        }



        // a couple math utility functions
        private int clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        private double limitRange(double value, double min, double max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        private double map(double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            return (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
        }

        private int toRange50(double value)
        {
            int newValue = (int)Math.Round(map(value, -1, 1, -50, 50));
            newValue = clamp(newValue, -50, 50);
            return newValue;
        }

        private int toRange100(double value)
        {
            int newValue = (int)Math.Round(map(value, -1, 1, -100, 100));
            newValue = clamp(newValue, -100, 100);
            return newValue;
        }

        private int toRange255(double value)
        {
            int newValue = (int)Math.Round(map(value, 0, 1, 0, 255));
            newValue = clamp(newValue, 0, 255);
            return newValue;
        }



        public void playSound(int soundId)
        {
            string command = "SOUND PLAY " + soundId;

            trySendSerial(command);
        }

    }
}
