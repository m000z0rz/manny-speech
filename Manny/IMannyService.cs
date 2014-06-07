using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
namespace Manny
{
    interface IMannyService
    {
        string Type {get;}

        void handleCommand(string functionName, XmlNode xmlData, Action<dynamic> action);
    }
}
