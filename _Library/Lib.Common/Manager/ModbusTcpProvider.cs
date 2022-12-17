using Lib.Common.Components.Agreements;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using static Lib.Common.Manager.GlobalVariables;

namespace Lib.Common.Manager
{
    public class ModbusTcpProvider
    {
        public static void ReadDocument()
        {
            string LocalPath = null;

            YamlBase.Modules.Where(c => c.Launcher == nameof(Communication.ModbusTcp)).Select(c => new
            {
                c.FilePath

            }).ToList().ForEach(c =>
            {
                LocalPath = $"{AppDomain.CurrentDomain.BaseDirectory}{c.FilePath}";
            });

            IConfigurationBuilder Configbuilder = new ConfigurationBuilder().AddJsonFile(LocalPath);

            Parameter = Configbuilder.Build();
        }

        public IConfigurationRoot MachineMap
        {
            get { return Parameter; }

            private set
            {
                if (Parameter != value) MachineInfoChanging?.Invoke(this, EventArgs.Empty);

                Parameter = value;
            }
        }

        private static IConfigurationRoot Parameter;

        public event EventHandler MachineInfoChanging;
    }
}
