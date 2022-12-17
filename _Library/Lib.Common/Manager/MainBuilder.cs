using Lib.Common.Manager.Models;
using System;
using YamlDotNet.Serialization;

namespace Lib.Common.Manager
{
    public static class MainBuilder
    {
        public static MainBook RealMainYml()
        {
            try
            {
                MainProvider provider = new();
                Deserializer deserializer = new();

                return deserializer.Deserialize<MainBook>(provider.ConfigBasic);
            }
            catch(Exception e)
            {
                Console.WriteLine($"[Main.yml] => File format problem\n{e.Message}");
                Console.ReadLine();

                return null;
            }
        }
    }
}
