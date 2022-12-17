using Edge.Zeus.Panels;
using Lib.Common.Components.Agreements;
using Lib.Common.Manager;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using static Lib.Common.Manager.GlobalVariables;
using System.Runtime.InteropServices;

namespace Edge.Zeus
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            Console.Title = EdgeTitle + EdgeVersion;
            init();

            if (Directory.Exists(IIOTFilePath))
            {
                DInfo = new(IIOTFilePath);
                DInfo.Delete(true);
            }

            DInfo = new DirectoryInfo(LocalFilePath).CreateSubdirectory(YamlBase.Protagonist.IIOTFileName);

            if (args.Length == 0 || args[0] != YamlBase.Protagonist.LocalName) return;

            Task.Run(() =>
            {
                while (Console.ReadKey().Key.ToString() == "H")
                {
                    HiddenConsoleWindow(0);
                }
            });

            FoundationProvider.ReadDocument();

            PipeBuilder.BuildAsync();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Runon>();

                GlobalApproach.PipeBuilder(true, Communication.EdgeService);

                EnableEdgeService = true;
            });

        private static DirectoryInfo DInfo { get; set; }

        #region program global control

        private static void init()
        {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
            HiddenConsoleWindow(1);
        }

        public static void HiddenConsoleWindow(int nCmdWindow)
        {
            ShowWindow(GetConsoleWindow(), nCmdWindow);
        }
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow(); //获取控制台窗体句柄

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); //隐藏或显示控制台窗体

        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);// 删除菜单

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);// 获取系统菜单句柄

        ///Description: user press [Ctrl+C] to stop DDS program
        public static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("====== sMMP Edge Program will be terminated.=====");
            args.Cancel = true;


        }//Console_CancelKeyPress()

        #endregion
    }
}