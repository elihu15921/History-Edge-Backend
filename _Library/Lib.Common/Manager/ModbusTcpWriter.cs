using Lib.Common.Components.Agreements;
using Lib.Common.Manager.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using static Lib.Common.Manager.GlobalVariables;

namespace Lib.Common.Manager
{
    public class ModbusTcpWriter : DocumentWriterFactory
    {
        public override void Build()
        {
            YamlBase.Modules.Where(c => c.Launcher == nameof(Communication.ModbusTcp)).Select(c => new
            {
                path = c.FilePath

            }).ToList().ForEach(c =>
            {
                FoundationProvider provider = new();

                File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{c.path}", JsonConvert.SerializeObject(new ModbusTcpTitle()
                {
                    MachineBox = provider.FoundationBasic.ModbusTCP

                }, Formatting.Indented));

                ModbusTcpProvider.ReadDocument();
            });
        }
    }
}