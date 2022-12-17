using Lib.Common.Components.Agreements;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Edge.ModbusTcp.Components;

namespace Edge.ModbusTcp
{
    public class WorkBuilder : IConstruction
    {
        public void Start() => new HostBuilder().ConfigureServices(svcs => svcs.AddHostedService<ModbusTcpManager>()).Build().Run();
    }
}
