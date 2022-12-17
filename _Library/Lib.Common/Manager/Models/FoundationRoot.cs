using System.Collections.Generic;

namespace Lib.Common.Manager.Models
{
    public class FoundationRoot
    {
        public string UpdateTime { get; set; }
        public bool Disabled { get; set; }
        public ServerMap Server { get; set; } = new();
        public EdgeMap Edge { get; set; } = new();
        public EaiMap Eai { get; set; } = new();

        public List<ModbusTcpRoot> ModbusTCP = new();

        //public List<OpcUa> OpcUa = new();

        //public List<WebApi> WebApi = new();
    }

    public class ServerMap
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int MqttPort { get; set; }
    }

    public class EdgeMap
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }

    public class EaiMap
    {
        public string Version { get; set; }
        public Host Host { get; set; } = new();
        public Service Service { get; set; } = new();
    }

    public class Host
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Id { get; set; }
        public string Account { get; set; }
        public string Language { get; set; }
    }

    public class Service
    {
        public string Name { get; set; }
        public string Srvver { get; set; }
        public string Ip { get; set; }
        public string Id { get; set; }
    }
}
