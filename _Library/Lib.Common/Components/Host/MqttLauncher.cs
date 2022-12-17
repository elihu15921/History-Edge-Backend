using Lib.Common.Components.Agreements;
using Lib.Common.Components.History;
using Lib.Common.Components.Models;
using Lib.Common.Manager;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Serilog.Events;
using System;
using System.IO;
using System.Threading.Tasks;
using static Lib.Common.Manager.GlobalVariables;

namespace Lib.Common.Components.Host
{
    public class MqttLauncher : BaseStationFactory
    {
        public override async Task SendAsync(PayloadRoot payload, HostChannel channel)
        {
            if (channel == HostChannel.Undefined || payload.Row.Count == 0) return;

            string clientId = null, topic = null;

            switch (channel)
            {
                case HostChannel.Status:
                    clientId = payload.MachineNo + "#" + nameof(HostChannel.Status);
                    topic = $"/{nameof(HostTransaction.Smmp).ToLower()}/{payload.MachineNo}/{nameof(WorkTasks.Collection).ToLower()}/{nameof(HostChannel.Status).ToLower()}";
                    break;

                case HostChannel.Parameter:
                    clientId = payload.MachineNo + "#" + nameof(HostChannel.Parameter);
                    topic = $"/{nameof(HostTransaction.Smmp).ToLower()}/{payload.MachineNo}/{nameof(WorkTasks.Collection).ToLower()}/{nameof(HostChannel.Parameter).ToLower()}";
                    break;

                case HostChannel.Production:
                    clientId = payload.MachineNo + "#" + nameof(HostChannel.Production);
                    topic = $"/{nameof(HostTransaction.Smmp).ToLower()}/{payload.MachineNo}/{nameof(WorkTasks.Collection).ToLower()}/{nameof(HostChannel.Production).ToLower()}";
                    break;
            }

            payload.Row.ForEach(c =>
            {
                if (YamlBase.Propertie.Debug) Console.WriteLine($"[{DateTime.Now:MM/dd HH:mm ss}] ModbusTCP => NO.{payload.MachineNo} - {c.AttribNo}:{c.AttribValue}");
            });

            try
            {
                FoundationProvider box = new();

                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot config = builder.Build();

                IMqttClient client = new MqttFactory().CreateMqttClient();

                await client.ConnectAsync(new MqttClientOptionsBuilder()
                    .WithTcpServer(box.FoundationBasic.Server.Address.Split('/')[2].Split(':')[0], box.FoundationBasic.Server.MqttPort)
                    .WithCredentials(config.GetValue<string>("Server:Account"), config.GetValue<string>("Server:Password"))
                    .WithClientId(clientId)
                    .Build());

                await client.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(JsonConvert.SerializeObject(payload))
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build());

                await client.DisconnectAsync();

                client.Dispose();
            }
            catch (Exception e)
            {
                LogBuilder.WriteLog(LogEventLevel.Error, "MQTT Server => " + e.Message);
            }
        }
    }
}
