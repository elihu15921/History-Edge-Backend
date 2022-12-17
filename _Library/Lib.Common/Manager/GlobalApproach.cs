using Lib.Common.Components.Agreements;
using Lib.Common.Components.Func;
using Lib.Common.Components.Host;
using Lib.Common.Components.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;

namespace Lib.Common.Manager
{
    public class GlobalApproach
    {
        public static void PushDataToHost(HostChannel channel, PayloadRoot payload)
        {
            switch (payload.Version.FormatFirstCapitalized())
            {
                case nameof(HostTransaction.Eai):
                    ContactBuilder(new EaiLauncher(), payload, channel);
                    break;

                case nameof(HostTransaction.Smes):
                    ContactBuilder(new MesLauncher(), payload, channel);
                    break;

                default:
                    ContactBuilder(new MqttLauncher(), payload, channel);
                    break;
            }
        }

        public static Func<JsonWriterFactory, IPAddress, string, bool> LocalBuilder { get; set; } = WriteLocalFile;
        public static Action<bool, Communication> PipeBuilder { get; set; } = BuildPipeTitle;
        public static Action<BaseStationFactory, PayloadRoot, HostChannel> ContactBuilder { get; set; } = PushHost;
        public static Action<ProtocolFactory, IConfigurationRoot> SignalBuilder { get; set; } = PullMachine;
        public static Action<DocumentWriterFactory> DocumentWriter { get; set; } = BuildJsonDocument;
        private static void BuildJsonDocument(DocumentWriterFactory factory) => factory.Build();
        private static void PullMachine(ProtocolFactory factory, IConfigurationRoot root) => factory.SendAsync(root);
        private static void PushHost(BaseStationFactory factory, PayloadRoot root, HostChannel channel) => factory.SendAsync(root, channel);
        private static bool WriteLocalFile(JsonWriterFactory writer, IPAddress serverAddress, string request) => writer.Write(serverAddress, request);
        private static void BuildPipeTitle(bool enable, Communication protocol)
        {
            int revise = 1, deep = 0;

            string sign = "(-)", result = " is failed.", nowTime = $" [{DateTime.Now:MM/dd HH:mm ss}]   =>   ";

            if (enable)
            {
                sign = "(+)";
                result = " is success.";
                revise = 0;
            }

            Console.WriteLine(protocol switch
            {
                Communication.CsvFile => new string(' ', deep) + nowTime + nameof(Communication.CsvFile) + result + new string(' ', 8 + revise) + sign,
                Communication.ModbusTcp => new string(' ', deep) + nowTime + nameof(Communication.ModbusTcp) + result + new string(' ', 6 + revise) + sign,
                Communication.OpcUa => new string(' ', deep) + nowTime + nameof(Communication.OpcUa) + result + new string(' ', 10 + revise) + sign,
                Communication.EdgeService => new string(' ', deep) + nowTime + nameof(Communication.EdgeService) + result + new string(' ', 4 + revise) + sign,
                _ => null
            });
        }
    }
}