using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace Manny
{
    class SpeechService : IMannyService
    {
        public string Type
        {
            get { return "speech"; }
        }

        public void handleCommand(string functionName, XmlNode xmlData, Action<dynamic> action)
        {
            throw new NotImplementedException();
        }
    }
}
