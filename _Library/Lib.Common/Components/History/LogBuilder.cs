using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System.IO;

namespace Lib.Common.Components.History
{
    public class LogBuilder
    {
        public static void WriteLog(LogEventLevel eventLevel, string message)
        {
            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .CreateLogger();

            switch (eventLevel)
            {
                case LogEventLevel.Debug:
                    Log.Debug(message);
                    break;

                case LogEventLevel.Information:
                    Log.Information(message);
                    break;

                case LogEventLevel.Warning:
                    Log.Warning(message);
                    break;

                case LogEventLevel.Error:
                    Log.Error(message);
                    break;

                case LogEventLevel.Fatal:
                    Log.Fatal(message);
                    break;
            }

            Log.CloseAndFlush();
        }

        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
