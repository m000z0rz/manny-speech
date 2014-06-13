using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace Manny
{
    class Device
    {
        public int DeviceID { get; set; }
        public string Name { get; set; }
        public string Room { get; set; }
        public bool Dimmable { get; set; }

        public string ServiceType { get; set; }
        public int? StephaniePin { get; set; }
        public string InsteonAddress { get; set; }

        public Device()
        {

        }

        public Device(XmlNode fromJsonNode)
        {
            XmlNode node = fromJsonNode;

            DeviceID = int.Parse(node["deviceId"].InnerText);
            Name = node["name"].InnerText;
            Room = node["room"].InnerText;

            int dimmable = int.Parse(node["dimmable"].InnerText);
            if (dimmable == 0) Dimmable = false; else Dimmable = true;

            int pin;
            if (int.TryParse(node["stephaniePin"].InnerText, out pin)) StephaniePin = pin;
            else StephaniePin = null;

            InsteonAddress = node["insteonAddress"].InnerText;
        }
        

    }
}
