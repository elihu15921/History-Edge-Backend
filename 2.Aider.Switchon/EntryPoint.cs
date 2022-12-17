using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Edge.Switchon
{
    internal class EntryPoint
    {
        private static void Main(string[] args)
        {
            string sFileName = "sMMP_Edge.bat";

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + sFileName)) return;

            Process.Start(new ProcessStartInfo()
            {
                FileName = sFileName,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            });

            Console.WriteLine(args);

            Task.Delay(TimeSpan.FromSeconds(3)).Wait();
        }
    }
}
