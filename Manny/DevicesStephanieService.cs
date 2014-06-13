using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using System.Xml;

namespace Manny
{
    class DevicesStephanieService : IMannyService
    {
        private StephanieAvatar avatar;

        public DevicesStephanieService(StephanieAvatar _avatar)
        {
            avatar = _avatar;
        }

        public string Type
        {
            get { return "devices-stephanie"; }
        }

        public void handleCommand(string functionName, XmlNode xmlData, Action<dynamic> action)
        {
            dynamic payload = new {

            };


            

            //throw new NotImplementedException();
            if (functionName == "toggle")
            {
                Debug.WriteLine("toggle");
                int pin = int.Parse(xmlData["stephaniePin"].InnerText);
                togglePin(pin);

                Debug.WriteLine("pre action");
                action(payload);
            }
            else if (functionName == "turnOn")
            {
                int pin = int.Parse(xmlData["stephaniePin"].InnerText);
                turnOnPin(pin);

                action(payload);
            }
            else if (functionName == "turnOff")
            {
                int pin = int.Parse(xmlData["stephaniePin"].InnerText);
                turnOffPin(pin);

                action(payload);
            }
            else
            {
                payload = new
                {
                    err = "no function name " + functionName + " on service"
                };

                action(payload);
            }
        }

        private void togglePin(int pin)
        {
            Debug.WriteLine("toggling stephanie device on pin " + pin);
            avatar.ToggleDevice(pin);
        }

        private void turnOnPin(int pin)
        {
            Debug.WriteLine("turning on stephanie device on pin " + pin);
            avatar.TurnOnDevice(pin);
        }

        private void turnOffPin(int pin)
        {
            Debug.WriteLine("turning off stephanie device on pin " + pin);
            avatar.TurnOffDevice(pin);
        }
    }
}
