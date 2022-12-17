using Lib.Common.Components.Models;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace Lib.Common.Components.Agreements
{
    public abstract class DocumentWriterFactory
    {
        public abstract void Build();
    }

    public abstract class JsonWriterFactory
    {
        public abstract bool Write(IPAddress serverAddress, string request);
    }

    public abstract class BaseStationFactory
    {
        public abstract Task SendAsync(PayloadRoot root, HostChannel channel);
    }

    public abstract class ProtocolFactory
    {
        public abstract Task SendAsync(IConfigurationRoot root);
    }
}