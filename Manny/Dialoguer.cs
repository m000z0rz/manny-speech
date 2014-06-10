using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

using System.Diagnostics;
using System.IO;
//using System.Collections;
using System.Speech.Recognition.SrgsGrammar;

namespace Manny
{

    class Dialoguer
    {
        public class DialoguerMode
        {
            private string type;
            public string Type
            {
                get
                {
                    return type;
                }
            }

            public DialoguerMode(string _type)
            {
                type = _type;
            }
        }

        public class ModeChangedEventArgs : EventArgs
        {
            public ModeChangedEventArgs(DialoguerMode oldMode, DialoguerMode newMode)
            {
                OldMode = oldMode;
                NewMode = newMode;
            }
            public DialoguerMode OldMode { get; set; }
            public DialoguerMode NewMode { get; set; }
        }

        private DialoguerMode _mode;
        public DialoguerMode Mode {
            get {
                return _mode;
            }
            set {
                DialoguerMode oldMode = _mode;
                setGrammarStates(oldMode, value);
                _mode = value;

                ModeChangedEventArgs e = new ModeChangedEventArgs(oldMode, value);
                OnModeChanged(e);
            }
        }

        public static DialoguerMode ModeInactive = new DialoguerMode("inactive");
        public static DialoguerMode ModeActive = new DialoguerMode("active");
        public static DialoguerMode ModeStopListening = new DialoguerMode("stopListening");

        private SpeechRecognitionEngine sre;
        private SpeechSynthesizer tts;

        List<Grammar> grammars;

        Grammar toggleDevice;
        Grammar turnOnDevice;
        Grammar turnOffDevice;
        Grammar dimDevice;

        List<Device> devices = new List<Device>();

        #region events

        public event EventHandler<SpeechRecognizedEventArgs> CommandRecognized;
        public event EventHandler<VisemeReachedEventArgs> VisemeReached;
        public event EventHandler<ModeChangedEventArgs> ModeChanged;

        private void OnCommandRecognized(SpeechRecognizedEventArgs e)
        {
            if (CommandRecognized != null) CommandRecognized(this, e);
        }

        private void OnVisemeReached(VisemeReachedEventArgs e)
        {
            if (VisemeReached != null) VisemeReached(this, e);
        }

        private void OnModeChanged(ModeChangedEventArgs e)
        {
            if (ModeChanged != null) ModeChanged(this, e);
        }

        #endregion


        public Dialoguer(string startupPath) : this(startupPath, null, null, null, null) {

        }

        public Dialoguer(
            string startupPath,
            Stream recognitionAudioStream,
            SpeechAudioFormatInfo recognitionAudioFormat,
            Stream ttsAudioStream,
            SpeechAudioFormatInfo ttsAudioFormat
            )
        {
            // Build sre & tts, set inputs/outputs
            sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            tts = new SpeechSynthesizer();

            if(recognitionAudioStream == null) {
                sre.SetInputToDefaultAudioDevice();
            } else {
                sre.SetInputToAudioStream(recognitionAudioStream, recognitionAudioFormat);
            }

            if(ttsAudioStream == null) {
                tts.SetOutputToDefaultAudioDevice();
            } else {
                tts.SetOutputToAudioStream(ttsAudioStream, ttsAudioFormat);
            }

            

            // Load grammars
            grammars = new List<Grammar>();

            foreach (string grammarFile in Directory.GetFiles(Path.Combine(startupPath, "grammars"), "*.xml"))
            {
                Debug.WriteLine("grammar file " + grammarFile);
                Grammar g = new Grammar(grammarFile);
                grammars.Add(g);

                sre.LoadGrammar(g);
            }

            // dictation grammar to reduce false positives
            DictationGrammar dg = new DictationGrammar("grammar:dictation#pronunciation");
            dg.Name = "dictation";
            //grammars.Add(dg);
            sre.LoadGrammar(dg);


            // Wire events
            sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);

            tts.VisemeReached += new EventHandler<VisemeReachedEventArgs>(tts_VisemeReached);
            tts.SpeakStarted += new EventHandler<SpeakStartedEventArgs>(tts_SpeakStarted);
            tts.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(tts_SpeakCompleted);


            // Get started recognizing!
            Mode = ModeInactive;

            sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void SetDevices(List<Device> _devices)
        {
            devices = _devices;
            recompileDeviceGrammars();
        }

        void recompileDeviceGrammars()
        {           
            if (toggleDevice != null)
            {
                sre.UnloadGrammar(toggleDevice);
                grammars.Remove(toggleDevice);
            }
            if (turnOnDevice != null)
            {
                sre.UnloadGrammar(turnOnDevice);
                grammars.Remove(turnOnDevice);
            }
            if (turnOffDevice != null)
            {
                sre.UnloadGrammar(turnOffDevice);
                grammars.Remove(turnOffDevice);
            }
            if (dimDevice != null)
            {
                sre.UnloadGrammar(dimDevice);
                grammars.Remove(dimDevice);
            }

            if (devices.Count == 0) return;

            List<string> roomNames = new List<string>();
            List<string> deviceNames = new List<string>();

            foreach(Device device in devices)
            {
                if(device.Room != null && device.Room != "" && !roomNames.Contains(device.Room))
                {
                    roomNames.Add(device.Room);
                }

                if(!deviceNames.Contains(device.Name))
                {
                    deviceNames.Add(device.Name);
                }
            }



            SrgsRule rule = new SrgsRule("toggleDevice");

            SrgsOneOf srgsThe = new SrgsOneOf();
            srgsThe.Add(new SrgsItem("the"));
            srgsThe.Add(new SrgsItem());

            SrgsOneOf srgsToggle = new SrgsOneOf();
            SrgsItem item;
            //item = new SrgsItem("toggle");
            srgsToggle.Add(new SrgsItem("toggle"));
            srgsToggle.Add(new SrgsItem());

            SrgsOneOf srgsRoom = new SrgsOneOf();
            foreach(string roomName in roomNames)
            {
                item = new SrgsItem(roomName);
                item.Add(new SrgsSemanticInterpretationTag("out.room=\"" + roomName + "\";"));
                //srgsRoom.Add(new SrgsItem(roomName));
                srgsRoom.Add(item);
            }
            srgsRoom.Add(new SrgsItem());

            SrgsOneOf srgsDevice = new SrgsOneOf();
            foreach(string deviceName in deviceNames)
            {
                item = new SrgsItem(deviceName);
                item.Add(new SrgsSemanticInterpretationTag("out.deviceName=\"" + deviceName + "\";"));
                //srgsDevice.Add(new SrgsItem(deviceName));
                srgsDevice.Add(item);
            }

            rule.Add(srgsToggle);
            rule.Add(srgsThe);
            rule.Add(srgsRoom);
            rule.Add(srgsDevice);
            //Debug.WriteLine("toggle script: " + rule.Script);
            toggleDevice = new Grammar(new SrgsDocument(rule));            

            SrgsOneOf srgsTurnOn = new SrgsOneOf();
            srgsTurnOn.Add(new SrgsItem("turn on"));
            rule = new SrgsRule("turnOnDevice");
            rule.Add(srgsTurnOn);
            rule.Add(srgsThe);
            rule.Add(srgsRoom);
            rule.Add(srgsDevice);
            //Debug.WriteLine("turn on script: " + rule.Script);
            turnOnDevice = new Grammar(new SrgsDocument(rule));

            SrgsOneOf srgsTurnOff = new SrgsOneOf();
            srgsTurnOff.Add(new SrgsItem("turn off"));
            rule = new SrgsRule("turnOffDevice");
            rule.Add(srgsTurnOff);
            rule.Add(srgsThe);
            rule.Add(srgsRoom);
            rule.Add(srgsDevice);
            //Debug.WriteLine("turn off script: " + rule.Script);
            turnOffDevice = new Grammar(new SrgsDocument(rule));


            rule = new SrgsRule("dimDevice");
            rule.Add(new SrgsItem("dim"));
            rule.Add(srgsThe);
            rule.Add(srgsRoom);
            rule.Add(srgsDevice);
            dimDevice = new Grammar(new SrgsDocument(rule));


            Debug.WriteLine("built rules");
            

            sre.LoadGrammar(toggleDevice);
            sre.LoadGrammar(turnOnDevice);
            sre.LoadGrammar(turnOffDevice);
            sre.LoadGrammar(dimDevice);
            grammars.Add(toggleDevice);
            grammars.Add(turnOnDevice);
            grammars.Add(turnOffDevice);
            grammars.Add(dimDevice);
        }

        void setGrammarStates(DialoguerMode oldMode, DialoguerMode newMode)
        {
            if(newMode == ModeInactive)
            {
                foreach (Grammar g in grammars)
                {
                    if (g.RuleName == "name") g.Enabled = true;
                    else if (g.RuleName == "resumeListening") g.Enabled = true;
                    else g.Enabled = false;
                }
            }
            else if(newMode == ModeActive)
            {
                foreach (Grammar g in grammars)
                {
                    g.Enabled = true;
                }
            }
            else if (newMode == ModeStopListening)
            {
                foreach (Grammar g in grammars)
                {
                    if (g.RuleName == "resumeListening") g.Enabled = true;
                    else g.Enabled = false;
                }
            }
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {

            if (e.Result.Grammar.RuleName == "dictation")
            {
                Debug.WriteLine("ignoring dictation " + e.Result.Text);
                return;
            }

            Debug.WriteLine("Recognized " + e.Result.Grammar.RuleName + ": " + e.Result.Text);

            
            System.Xml.XmlDocument xd;
            xd = (System.Xml.XmlDocument)e.Result.ConstructSmlFromSemantics();
            Debug.WriteLine("xml semantics: {0}", xd.InnerXml);


            string ruleName = e.Result.Grammar.RuleName;
            if (ruleName == "name")
            {
                if (e.Result.Text == "Manny") return; // require 'Hey Manny'
                // good reference for ssml:http://www.cepstral.com/en/tutorials/view/ssml
                PromptBuilder pb = new PromptBuilder();
                pb.AppendSsmlMarkup("<prosody rate=\"1.2\">Yes?</prosody>");
                tts.SpeakAsync(pb);
                Mode = ModeActive;
            }
            else if (ruleName == "inactivate")
            {
                Mode = ModeInactive;
                //tts.SpeakAsync("Very good sir.");
            }
            else if (ruleName == "stopListening")
            {
                Mode = ModeStopListening;
                tts.SpeakAsync("no longer listening");
            }
            else if (ruleName == "resumeListening")
            {
                Mode = ModeActive;
                tts.SpeakAsync("resumed listening");
            }
            else if (ruleName == "getTime")
            {
                OnCommandRecognized(e);
                PromptBuilder pb = new PromptBuilder();
                string timeString = DateTime.Now.ToString("hh:mmtt");
                pb.AppendText("It is currently");
                pb.AppendTextWithHint(timeString, SayAs.Time12);
                Debug.WriteLine("recognized getTime " + timeString);

                tts.SpeakAsync(pb);
            }
            else
            {
                OnCommandRecognized(e);
            }
        }




        public Prompt SpeakAsync(string textToSpeak)
        {
            return tts.SpeakAsync(textToSpeak);
        }

        public void SpeakAsync(Prompt prompt)
        {
            tts.SpeakAsync(prompt);
        }

        public Prompt SpeakAsync(PromptBuilder promptBuilder)
        {
            return tts.SpeakAsync(promptBuilder);
        }





        void tts_SpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            sre.RecognizeAsyncCancel();
        }

        void tts_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        void tts_VisemeReached(object sender, VisemeReachedEventArgs e)
        {
            OnVisemeReached(e);
        }
    }
}
