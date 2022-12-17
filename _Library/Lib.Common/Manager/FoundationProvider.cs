using Lib.Common.Components.Verification;
using Lib.Common.Manager.Models;
using System;
using System.IO;
using static Lib.Common.Manager.GlobalVariables;

namespace Lib.Common.Manager
{
    public class FoundationProvider
    {
        public static void ReadDocument()
        {
            bool pass = true;

            if (!File.Exists(FoundationDocument))
            {
                FoundationWriter writer = new();

                if (!writer.Write(new(new byte[] { 0, 0, 0, 0 }), null)) pass = false;
            }

            if (pass)
            {
                using StreamReader sr = File.OpenText(FoundationDocument);

                FoundationInfo = sr.ReadToEnd().FoundationFilter();
            }
        }

        public FoundationRoot FoundationBasic
        {
            get { return FoundationInfo; }

            private set
            {
                if (FoundationInfo != value) MachineInfoChanging?.Invoke(this, EventArgs.Empty);

                FoundationInfo = value;
            }
        }

        private static FoundationRoot FoundationInfo { get; set; }

        public event EventHandler MachineInfoChanging;
    }
}