using System;
using System.IO;
using static Lib.Common.Manager.GlobalVariables;

namespace Lib.Common.Manager
{
    internal class MainProvider
    {
        internal string ConfigBasic
        {
            get { return ConfigInfo; }

            private set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("ConfigInfo content cannot be empty");
                }

                if (ConfigInfo != value) EventInfoChanging?.Invoke(this, EventArgs.Empty);

                ConfigInfo = value;
            }
        }

        public MainProvider()
        {
            using StreamReader value = File.OpenText($"{AppDomain.CurrentDomain.BaseDirectory}{LocalFileName}/Main.yml");

            ConfigInfo = value.ReadToEnd();
        }

        private static string ConfigInfo { get; set; }

        public event EventHandler EventInfoChanging;
    }
}