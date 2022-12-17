using Lib.Common.Components.Agreements;
using Lib.Common.Components.Func;
using Lib.Common.Manager;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Lib.Common.Manager.GlobalVariables;

namespace Edge.Zeus.Panels
{
    internal static class PipeBuilder
    {
        internal static void BuildAsync()
        {
            if (YamlBase.Modules == null) return;

            Task.Run(async () =>
            {
                YamlBase.Modules.ForEach(async c =>
                {
                    switch (c.Launcher)
                    {
                        case nameof(Communication.CsvFile):
                            await Task.Run(() =>
                            {
                                if (c.Enable)
                                {
                                    EnableCsvFile = true;
                                    GlobalApproach.PipeBuilder(true, Communication.CsvFile);
                                };
                            });
                            break;

                        case nameof(Communication.ModbusTcp):
                            await Task.Run(() =>
                            {
                                if (c.Enable)
                                {
                                    EnableModbusTcp = true;
                                    GlobalApproach.PipeBuilder(true, Communication.ModbusTcp);
                                    IConstruction factory = SimpleFactory.BuildService(c.Arguments.Replace(" ", ""));
                                    factory.Start();
                                };
                            });
                            break;

                        case nameof(Communication.OpcUa):
                            await Task.Run(() =>
                            {
                                if (c.Enable)
                                {
                                    EnableOpcUa = true;
                                    GlobalApproach.PipeBuilder(true, Communication.OpcUa);
                                };
                            });
                            break;
                    }
                });

                while (!EnableEdgeService) ;

                if (EnableCsvFile == false) GlobalApproach.PipeBuilder(false, Communication.CsvFile);
                if (EnableModbusTcp == false) GlobalApproach.PipeBuilder(false, Communication.ModbusTcp);
                if (EnableOpcUa == false) GlobalApproach.PipeBuilder(false, Communication.OpcUa);

                Console.WriteLine("\n Local IP => " + GeneralTools.GetLocalIP());

                string EquallyDivided = new('*', 14);

                Console.WriteLine($"{Welcome(WelcomeTitle)}\n {EquallyDivided} {ServiceTitle} {EquallyDivided}\n");

                await Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    if (!YamlBase.Propertie.Debug) ShowWindow(GetConsoleWindow(), 0);
                });

                static string Welcome(string title) => Figgle.FiggleFonts.Standard.Render(title);

                [DllImport("kernel32.dll")]
                static extern IntPtr GetConsoleWindow();

                [DllImport("user32.dll")]
                static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
            });
        }
    }
}