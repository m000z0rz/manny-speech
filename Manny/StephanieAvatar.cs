using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace Manny
{
    class StephanieAvatar
    {
        //StreamWriter serialStream;
        Stream serialPortBaseStream;
        Dialoguer dialoguer;

        public void Connect(Stream _serialStream, Dialoguer _dialoguer)
        {
            serialPortBaseStream = _serialStream;
            dialoguer = _dialoguer;

            dialoguer.VisemeReached += dialoguer_VisemeReached;
            dialoguer.ModeChanged += dialoguer_ModeChanged;
            dialoguer.CommandRecognized += dialoguer_CommandRecognized;
        }


        public void Disconnect()
        {
            serialPortBaseStream = null;

            dialoguer.VisemeReached -= dialoguer_VisemeReached;
            dialoguer.ModeChanged -= dialoguer_ModeChanged;
            dialoguer.CommandRecognized -= dialoguer_CommandRecognized;
        }




        void dialoguer_CommandRecognized(object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            //BlinkThirdEye();
        }

        void dialoguer_ModeChanged(object sender, Dialoguer.ModeChangedEventArgs e)
        {
            if (e.NewMode == Dialoguer.ModeActive)
            {
                EyesOn();
            }
            else if (e.NewMode == Dialoguer.ModeInactive)
            {
                EyesOff();
            }
        }

        void dialoguer_VisemeReached(object sender, System.Speech.Synthesis.VisemeReachedEventArgs e)
        {
            OnViseme(e.Viseme);
        }








        private bool trySendCommand(int opCode, int param = 0)
        {
            return trySendSerial("?" + opCode.ToString("D3") + param.ToString("D3") + "a");
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


        public void OpenBrainTray()
        {
            //serialStream.Write("?028000");
            trySendCommand(28);
        }

        public void CloseBrainTray()
        {
            //serialStream.Write("?029000");
            trySendCommand(29);
        }

        public void BlinkThirdEye()
        {
            //serialStream.Write("?023000");
            trySendCommand(23);
        }

        public void EyesOn()
        {
            //serialStream.Write("?026000");
            trySendCommand(26);
        }

        public void EyesOff()
        {
            //serialStream.Write("?027000");
            trySendCommand(27);
        }

        public void MoveMouth()
        {
            //serialStream.Write("?024000");
            trySendCommand(24);
        }

        // 1 through 16?
        public void ToggleDevice(int deviceNumber)
        {
            if (deviceNumber < 0 || deviceNumber > 16) return;
            //serialStream.Write("?0" + deviceNumber.ToString("D2") + "000");
            trySendCommand(deviceNumber);
        }

        public void TurnOnDevice(int deviceNumber)
        {
            if (deviceNumber < 0 || deviceNumber > 16) return;
            trySendCommand(deviceNumber, 2);
            //?
        }

        public void TurnOffDevice(int deviceNumber)
        {
            if (deviceNumber < 0 || deviceNumber > 16) return;
            trySendCommand(deviceNumber, 1);
            //?
        }

        public void TryCheckDevice(int deviceNumber, out bool isOn)
        {
            isOn = false;
            if (deviceNumber < 0 || deviceNumber > 16) return;
            trySendCommand(19, 1);
        }


        public void OnViseme(int visemeValue)
        {
            /*
            'speech viseme types:
            'SVP_0 = 0       'silence
            'SVP_1 = 1       'ae ax ah
            'SVP_2 = 2       'aa
            'SVP_3 = 3       'ao
            'SVP_4 = 4       'ey eh uh
            'SVP_5 = 5       'er
            'SVP_6 = 6       'y iy ih ix
            'SVP_7 = 7       'w uw
            'SVP_8 = 8       'ow
            'SVP_9 = 9       'aw
            'SVP_10 = 10     'oy
            'SVP_11 = 11     'ay
            'SVP_12 = 12     'h
            'SVP_13 = 13     'r
            'SVP_14 = 14     'l
            'SVP_15 = 15     's z
            'SVP_16 = 16     'sh ch jh zh
            'SVP_17 = 17     'th dh
            'SVP_18 = 18     'f v
            'SVP_19 = 19     'd t n
            'SVP_20 = 20     'k g ng
            'SVP_21 = 21     'p b m
             */

            if (visemeValue <= 11 && visemeValue > 0)
            {
                MoveMouth();
            }


        }
    }
}
