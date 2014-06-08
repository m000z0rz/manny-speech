using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using System.IO;
using System.IO.Ports;


using System.Xml;

using Newtonsoft.Json;

//using WebSocket4Net;


namespace Manny
{
    public partial class frmMain : Form
    {
        XmlElement config;

        Dialoguer localDialoguer;
        

        Queue<string> speakQueue = new Queue<string>();

        SocketIOClient.Client socket;


        Dictionary<string, IMannyService> serviceMap = new Dictionary<string, IMannyService>();
        StephanieAvatar stephanie;


        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Load config file
            string configFilename = Path.Combine(Application.StartupPath, "config.json");
            if (!File.Exists(configFilename))
            {
                throw new Exception("config.json not found");
            }
            XmlDocument configDocument = JsonConvert.DeserializeXmlNode(File.ReadAllText(configFilename), "root");
            config = configDocument["root"];



            // Connect to hub
            Debug.WriteLine("Connecting to hub @ " + config["hub"]["address"].InnerText);
            string hubAddress = config["hub"]["address"].InnerText;
            socket = new SocketIOClient.Client(hubAddress);
            socket.Connect();
           
            // Setup local speech recognition/tts
            
            localDialoguer = new Dialoguer(Application.StartupPath);
            localDialoguer.CommandRecognized += localDialoguer_CommandRecognized;

            //StephanieAvatar stephanie;
            stephanie = new StephanieAvatar();

            if (config["localSpeech"] != null && config["localSpeech"]["avatar"] != null)
            {
                XmlElement avatarConfig = config["localSpeech"]["avatar"];
                string avatarType = avatarConfig["type"].InnerText;
                if (avatarType == "stephanie")
                {
                    string serialPortName = avatarConfig["serialPort"].InnerText;
                    SerialPort stephanieSerial = new SerialPort(serialPortName, 9600, Parity.None, 8, StopBits.One);
                    stephanieSerial.Encoding = System.Text.Encoding.ASCII;
                    stephanieSerial.Open();
                    stephanie.Connect(stephanieSerial.BaseStream, localDialoguer);

                }
            }


            // setup services

            Debug.WriteLine(config.InnerXml);
            XmlNodeList xmlServices = config.GetElementsByTagName("services");
            List<string> serviceTypes = new List<string>();
            foreach (XmlNode xmlService in xmlServices)
            {
                string serviceType = xmlService["type"].InnerText;
                if (serviceType == "devices-stephanie")
                {
                    serviceTypes.Add(serviceType);
                    DevicesStephanieService devicesStephanie = new DevicesStephanieService(stephanie);
                    serviceMap.Add(serviceType, devicesStephanie);

                }
            }



            
            socket.On("connect", (Action<SocketIOClient.Messages.IMessage>)((data) =>
                {
                    Debug.WriteLine("Connected to hub");
                    dynamic payload;
                    payload = new {
                        context = JsonConvert.DeserializeObject(JsonConvert.SerializeXmlNode(config["nodeContext"], Newtonsoft.Json.Formatting.None)),
                        services = serviceTypes.ToArray()
                    };

                    socket.Emit("announceNode", payload );

                    socket.Emit("getDeviceList", null, "", (Action<dynamic>)((deviceData) =>
                        {
                            List<Device> devices = new List<Device>();
                            Debug.WriteLine("getDeviceList callback?");
                            string json = JsonConvert.SerializeObject(deviceData.Args[0]);
                            System.Xml.XmlDocument xml = JsonConvert.DeserializeXmlNode(deviceData.Args[0], "root");
                            foreach (XmlNode device in xml["root"].GetElementsByTagName("devices"))
                            {
                                Debug.WriteLine("device: " + device.InnerXml);
                                devices.Add(new Device(device));
                            }

                            localDialoguer.SetDevices(devices);
                            //Debug.WriteLine(xml.ToString());
                        }));
                }));

            socket.On("handleCommand", (Action<SocketIOClient.Messages.IMessage>)((data) =>
                {
                    //todo: callback?
                    string json = JsonConvert.SerializeObject(data.Json.Args[0]);
                    XmlDocument xd = JsonConvert.DeserializeXmlNode(json, "root");
                    XmlNode xmlData = xd["root"];

                    IMannyService service = serviceMap[xmlData["type"].InnerText];
                    if (service != null)
                    {
                        service.handleCommand(xmlData["functionName"].InnerText, xmlData, (Action<dynamic>)((data2) =>
                            {
                                // todo: callback
                                /*
                                 * http://stackoverflow.com/questions/23146777/socketio4net-acknowledgement-callback
                                // Simulate a ack callback because SocketIO4Net doesn't provide one by default.
                                var msgText = JsonConvert.SerializeObject(new object[] {
                                    // object you want to send back
                                });
                                var ack = new AckMessage()
                                {
                                    AckId = msg.AckId,
                                    MessageText = msgText
                                };
                                this.socket.Send(ack);
                                 * */
                                Debug.WriteLine("pre ack");

                                string[] pieces = data.RawMessage.Split(':');
                                string[] pieces2 = pieces[1].Split('+');
                                int ackId = int.Parse(pieces2[0]);

                                string messageText;
                                if(data2 == null) {
                                    messageText = "{}";
                                } else {
                                    messageText = JsonConvert.SerializeObject(data2);   
                                }
                                messageText = ackId + "+[" + messageText + "]";
                                var ack = new SocketIOClient.Messages.AckMessage()
                                {
                                    MessageText = messageText
                                };

                                Debug.WriteLine("should send " + ack.Encoded);
                                socket.Send(ack);
                            }));
                    }
                    else
                    {
                        throw new Exception("doesn't have service and need to figure out how to send callback");
                    }
                }));
        }

        void localDialoguer_CommandRecognized(object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            System.Xml.XmlDocument xd;
            xd = (System.Xml.XmlDocument)e.Result.ConstructSmlFromSemantics();

            Debug.WriteLine(xd.InnerXml);

            string ruleName = e.Result.Grammar.RuleName;

            if (ruleName == "getTemperature")
            {
                getTemperature();
            }
            else if (ruleName == "fanOn")
            {
                fanOn();
            }
            else if (ruleName == "fanAuto")
            {
                fanAuto();
            }
            else if (ruleName == "ejectBrain")
            {
                if (stephanie != null) stephanie.OpenBrainTray();
            }
            else if (ruleName == "retractBrain")
            {
                if (stephanie != null) stephanie.CloseBrainTray();
            }
            else if (ruleName == "toggleDevice")
            {
                string room, deviceName;
                if (xd["SML"]["room"] != null) room = xd["SML"]["room"].InnerText;
                else room = config["nodeContext"]["room"].InnerText;

                deviceName = xd["SML"]["deviceName"].InnerText;

                var payload = new
                {
                    type = "device",
                    functionName = "toggle",
                    room = room,
                    deviceName = deviceName
                };

                socket.Emit("handleCommand", payload, "", (Action<dynamic>)((data) =>
                {
                    //Debug.WriteLine("set fan on callback");
                    //string json = JsonConvert.SerializeObject(data.Args[0]);
                    //Debug.WriteLine(json);
                    Debug.WriteLine("speak toggled");
                    localDialoguer.SpeakAsync(deviceName + " toggled");
                }));

            }
            else if (ruleName == "turnOnDevice")
            {
                string room, deviceName;
                if (xd["SML"]["room"] != null) room = xd["SML"]["room"].InnerText;
                else room = config["nodeContext"]["room"].InnerText;

                deviceName = xd["SML"]["deviceName"].InnerText;

                var payload = new
                {
                    type = "device",
                    functionName = "turnOn",
                    room = room,
                    deviceName = deviceName
                };

                socket.Emit("handleCommand", payload, "", (Action<dynamic>)((data) =>
                {
                    //Debug.WriteLine("set fan on callback");
                    //string json = JsonConvert.SerializeObject(data.Args[0]);
                    //Debug.WriteLine(json);

                    localDialoguer.SpeakAsync(deviceName + " on");
                }));
            }
            else if (ruleName == "turnOffDevice")
            {
                string room, deviceName;
                if (xd["SML"]["room"] != null) room = xd["SML"]["room"].InnerText;
                else room = config["nodeContext"]["room"].InnerText;

                deviceName = xd["SML"]["deviceName"].InnerText;

                var payload = new
                {
                    type = "device",
                    functionName = "turnOff",
                    room = room,
                    deviceName = deviceName
                };

                socket.Emit("handleCommand", payload, "", (Action<dynamic>)((data) =>
                {
                    //Debug.WriteLine("set fan on callback");
                    //string json = JsonConvert.SerializeObject(data.Args[0]);
                    //Debug.WriteLine(json);

                    localDialoguer.SpeakAsync(deviceName + " off");
                }));
            }
            else if (ruleName == "dimDevice")
            {
                string room, deviceName;
                if (xd["SML"]["room"] != null) room = xd["SML"]["room"].InnerText;
                else room = config["nodeContext"]["room"].InnerText;

                deviceName = xd["SML"]["deviceName"].InnerText;
                
                var payload = new
                {
                    type = "device",
                    functionName = "setDimmer",
                    room = room,
                    deviceName = deviceName,
                    lightLevel = 0.5
                };

                socket.Emit("handleCommand", payload, "", (Action<dynamic>)((data) =>
                {
                    //Debug.WriteLine("set fan on callback");
                    //string json = JsonConvert.SerializeObject(data.Args[0]);
                    //Debug.WriteLine(json);

                    localDialoguer.SpeakAsync(deviceName + " dimmed");
                }));
            }
            else if (ruleName == "startAllLinking")
            {
                var payload = new
                {
                    type = "devices-insteon",
                    functionName = "startAllLinking"
                };

                socket.Emit("handleCommand", payload, "", (Action<dynamic>)((data) =>
                {
                    localDialoguer.SpeakAsync("I am ready to link an insteon device.");
                }));
            }
            else if (ruleName == "cancelAllLinking")
            {
                var payload = new
                {
                    type = "devices-insteon",
                    functionName = "cancelAllLinking"
                };
                socket.Emit("handleCommand", payload, "", (Action<dynamic>)((data) =>
                {
                    localDialoguer.SpeakAsync("Linking process canceled.");
                }));
            }
            else if (ruleName == "pullUpWiiU")
            {
                var onkyoInput = new
                {
                    type = "onkyo",
                    functionName = "setInputSelector",
                    input = "game"
                };
                socket.Emit("handleCommand", onkyoInput);

                var onkyoVolume = new
                {
                    type = "onkyo",
                    functionName = "setVolume",
                    volume = "35"
                };
                socket.Emit("handleCommand", onkyoVolume);

                var projectorPower = new
                {
                    type = "benqprojector",
                    functionName = "setPower",
                    power = "on"
                };
                socket.Emit("handleCommand", projectorPower);

                
                var projectorSource = new
                {
                    type = "benqprojector",
                    functionName = "setSource",
                    source = "hdmi"
                };
                socket.Emit("handleCommand", projectorSource);
                
            }
            else if (ruleName == "pullUpPC")
            {
                var onkyoInput = new
                {
                    type = "onkyo",
                    functionName = "setInputSelector",
                    input = "pc"
                };
                socket.Emit("handleCommand", onkyoInput);

                var onkyoVolume = new
                {
                    type = "onkyo",
                    functionName = "setVolume",
                    volume = "45"
                };
                socket.Emit("handleCommand", onkyoVolume);


                var projectorPower = new
                {
                    type = "benqprojector",
                    functionName = "setPower",
                    power = "on"
                };
                socket.Emit("handleCommand", projectorPower);


                
                var projectorSource = new
                {
                    type = "benqprojector",
                    functionName = "setSource",
                    source = "pc"
                };
                socket.Emit("handleCommand", projectorSource);
                
            }
            else if (ruleName == "shutdownProjector")
            {
                var projectorPower = new
                {
                    type = "benqprojector",
                    functionName = "setPower",
                    power = "off"
                };
                socket.Emit("handleCommand", projectorPower);
            }
        }

        void fanOn()
        {
            var payload = new {
                type = "nest",
                functionName = "setFanOn"
            };

            socket.Emit("handleCommand", payload, "", (Action<dynamic>) ((data) =>
            {
                Debug.WriteLine("set fan on callback");
                string json = JsonConvert.SerializeObject(data.Args[0]);
                Debug.WriteLine(json);

                localDialoguer.SpeakAsync("Fan is now on.");
            }));
        }

        void fanAuto()
        {
            var payload = new {
                type = "nest",
                functionName = "setFanAuto"
            };

            socket.Emit("handleCommand", payload, "", (Action<dynamic>) ((data) =>
            {
                Debug.WriteLine("set fan auto callback");
                string json = JsonConvert.SerializeObject(data.Args[0]);
                Debug.WriteLine(json);

                localDialoguer.SpeakAsync("Fan is now set to auto.");
            }));
        }

        void getTemperature()
        {
            Debug.WriteLine("getting temperature");
            Debug.WriteLine(socket.ReadyState);

            //dynamic payload;
            //payload.type = "nest";
            //payload.functionName = "getStatus";

            var payload = new {
                type = "nest",
                functionName = "getStatus"
            };

            //socket.Emit("handleCommand", "{\"type\":\"nest\", \"functionName\":\"getStatus\"}", "", (data) =>
            socket.Emit("handleCommand", payload, "", (Action<dynamic>) ((data) =>
            {
                Debug.WriteLine("callback?");
                string json = JsonConvert.SerializeObject(data.Args[0]);
                //Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                System.Xml.XmlDocument xml = JsonConvert.DeserializeXmlNode(data.Args[0],"root");
                Debug.WriteLine(xml.ToString());
                
                //object objer = data.Json;
                //Debug.WriteLine(json);

                localDialoguer.SpeakAsync("" + xml["root"]["currentTemperature"].InnerText + " degrees.");
                //data
                //Debug.WriteLine(data);   
                //object thing;
            }));
           
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            getTemperature();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            fanOn();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            fanAuto();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            socket.Close();
        }
    }
}
