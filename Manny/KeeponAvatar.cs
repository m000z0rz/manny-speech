using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
using System.ComponentModel;


using Manny.Keepon;


namespace Manny
{
    class KeeponAvatar
    {
        Dialoguer dialoguer;

        Manny.Keepon.Keepon keepon;


        

        public void Connect(Stream _serialStream, Dialoguer _dialoguer)
        {
            dialoguer = _dialoguer;

            keepon = new Keepon.Keepon();

            keepon.Connect(_serialStream);

            keepon.ButtonPressed += keepon_ButtonPressed;

            dialoguer.VisemeReached += dialoguer_VisemeReached;
            dialoguer.ModeChanged += dialoguer_ModeChanged;
            dialoguer.CommandRecognized += dialoguer_CommandRecognized;
            dialoguer.SpeakCompleted += dialoguer_SpeakCompleted;
        }

        void dialoguer_SpeakCompleted(object sender, System.Speech.Synthesis.SpeakCompletedEventArgs e)
        {
            keepon.Stop();
        }



        void keepon_ButtonPressed(object sender, Keepon.Keepon.KeeponButtonEventArgs e)
        {
            if (e.Button == KeeponButton.Head && e.Pressed)
            {
                dialoguer.Mode = Dialoguer.ModeActive;
            }
        }




        public void Disconnect()
        {
            keepon.Disconnect();

            dialoguer.VisemeReached -= dialoguer_VisemeReached;
            dialoguer.ModeChanged -= dialoguer_ModeChanged;
            dialoguer.CommandRecognized -= dialoguer_CommandRecognized;
        }




        void dialoguer_CommandRecognized(object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            keepon.Move(
                new KeeponMotorTarget(0, null, 50, 255, 500),
                new KeeponMotorTarget(null, null, 0, 255, 500)
                );
            //moveTilt(50, 1);
            //BlinkThirdEye();
        }

        void dialoguer_ModeChanged(object sender, Dialoguer.ModeChangedEventArgs e)
        {
            if (e.NewMode == Dialoguer.ModeActive)
            {
                keepon.Move(
                    new KeeponMotorTarget(0, 200, 0, 150, KeeponSideTarget.Center, 255)
                    );
                //movePan(0, 0.8);
                //moveSide(SideMovement.CenterFromLeft, 0.5);
                //moveSide("CENTER", 0.5);
                //moveTilt(0, 0.5);
            }
            else if (e.NewMode == Dialoguer.ModeInactive)
            {
                keepon.Move(
                    new KeeponMotorTarget(25, 50, 90, 40, KeeponSideTarget.Right, 40, 3000)
                    );
                //movePan(0.5, 0.2);
                //moveSide(SideMovement.Right, 0.15);
                //moveSide("RIGHT", 0.15);
                //moveTilt(1, 0.15);
            }
            else if (e.NewMode == Dialoguer.ModeStopListening)
            {
                keepon.Move(
                    new KeeponMotorTarget(-35, 50, 90, 40, KeeponSideTarget.Left, 40, 3000)
                    );
                //movePan(-0.9, 0.2);
                //moveSide(SideMovement.Left, 0.15);
                //moveSide("LEFT", 0.15);
                //moveTilt(1, 0.15);
            }
        }

        void dialoguer_VisemeReached(object sender, System.Speech.Synthesis.VisemeReachedEventArgs e)
        {
            OnViseme(e.Viseme);
        }










        // other things you can do:
        //   sound repeat <0...63>
        // sound delay
        // sound stop
        // can technically move pan -100 to 100, not just -50 to 50
        // move stop
        // mode tempo
        // mode sleep




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
                keepon.NextMove(
                    new KeeponMotorTarget(null, null, null, null, KeeponPonTarget.HalfDown, 255, 500),
                    new KeeponMotorTarget(null, null, null, null, KeeponPonTarget.HalfUp, 255, 500)
                    );
                //moveVertical(PonMovement.HalfUp);
            }


        }
    }
}
