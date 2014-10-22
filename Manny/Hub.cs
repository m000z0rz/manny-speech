using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Xml;

using Newtonsoft.Json;

namespace Manny
{
    class Hub
    {
        SocketIOClient.Client socket;




        public bool IsConnected
        {
            get
            {
                return (socket != null && socket.IsConnected);
            }
        }

        



        #region Events

        public event EventHandler<EventArgs> Connected;

        private void OnConnected()
        {
            EventArgs e = new EventArgs();
            OnConnected(e);
        }

        private void OnConnected(EventArgs e)
        {
            if (Connected != null) Connected(this, e);
        }

        #endregion



        public void Connect(string hubAddress)
        {
            Disconnect();
            socket = new SocketIOClient.Client(hubAddress);

            socket.On("connect", (Action<SocketIOClient.Messages.IMessage>)((data) =>
            {
                OnConnected(); // subscriber should do announce node here, and probably fetch fresh device list
            }));

            

            socket.Connect();
        }

        public void Disconnect()
        {
            if (socket != null && socket.IsConnected)
            {
                socket.Dispose();
                socket = null;
            }
        }


        public void AnnounceNode(XmlNode nodeContext, String[] services)
        {
            if (!IsConnected) return;

            dynamic payload;
            payload = new
            {
                nodeContext = JsonConvert.DeserializeObject(JsonConvert.SerializeXmlNode(nodeContext, Newtonsoft.Json.Formatting.None)),
                services = services
            };

            socket.Emit("announceNode", payload);


        }

        public void GetDeviceList(Action<List<Device>> callback)
        {
            if (!IsConnected) return;

            socket.Emit("getDeviceList", null, "", (Action<dynamic>)((deviceData) =>
             {
                 List<Device> devices = new List<Device>();
                 string json = JsonConvert.SerializeObject(deviceData.Args[0]);
                 System.Xml.XmlDocument xml = JsonConvert.DeserializeXmlNode(deviceData.Args[0], "root");
                 foreach (XmlNode device in xml["root"].GetElementsByTagName("devices"))
                 {
                     //Debug.WriteLine("device: " + device.InnerXml);
                     devices.Add(new Device(device));
                 }

                 callback(devices);
                 //localDialoguer.SetDevices(devices);
             }));
        }
    }
}
