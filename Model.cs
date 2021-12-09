using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerHealthCheck
{
    [XmlRoot("ServerList")]
    public class ServerList
    {
        [XmlElement("Server")]
        public List<Server> Servers { get; set; }
    }

    public class Server
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("Drive")]
        public List<Drive> Drives { get; set; }
    }

    public class Drive
    {
        [XmlAttribute("Letter")]
        public string Letter { get; set; }

        [XmlAttribute("MinimumGb")]
        public int MinimumGb { get; set; }
    }
}
