using Lib.Common.Components.Agreements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Edge.Zeus.Controllers
{
    public class WebController : ISOAP
    {
        public string InvokeSrv(string in0)
        {
            return
                new XElement("response", new XElement("reqid", "3175562140567422664467"),
                new XElement("srvver", "1.0"),
                new XElement("srvcode", ""),
                new XElement("payload", new XElement("param", new XAttribute("key", "std_data"),
                new XAttribute("type", "xml"),
                new XElement("data_response", new XElement("execution", new XElement("status", new XAttribute("code", ""),
                new XAttribute("sql_cod", ""),
                new XAttribute("description", ""))))))).ToString();
        }
    }
}
