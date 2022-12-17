using Edge.ModbusTcp.Pipeline;
using Lib.Common.Manager;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Edge.ModbusTcp.Components
{
    public class ModbusTcpManager : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            GlobalApproach.DocumentWriter(new ModbusTcpWriter());

            ModbusTcpProvider provider = new();

            GlobalApproach.SignalBuilder(new Standard(), provider.MachineMap);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            RebootModbusTcp = true;

            ModbusTcpManager management = new();

            management.StartAsync(new CancellationToken());

            return Task.CompletedTask;
        }

        internal static bool RebootModbusTcp { get; set; }
    }
}