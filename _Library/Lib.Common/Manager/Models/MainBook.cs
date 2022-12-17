using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Lib.Common.Manager.Models
{
    public sealed class MainBook
    {
        [YamlMember(Alias = "Program", ApplyNamingConventions = false)]
        public Protagonist Protagonist { get; set; }

        [YamlMember(Alias = "Modules", ApplyNamingConventions = false)]
        public List<Module> Modules { get; set; }

        [YamlMember(Alias = "Propertie", ApplyNamingConventions = false)]
        public Propertie Propertie { get; set; }
    }

    public sealed class Protagonist
    {
        [YamlMember(Alias = "Name", ApplyNamingConventions = false)]
        public string LocalName { get; set; }

        [YamlMember(Alias = "Foundation.Document.Name", ApplyNamingConventions = false)]
        public string FoundationDocumentName { get; set; }

        [YamlMember(Alias = "IIOT.File.Name", ApplyNamingConventions = false)]
        public string IIOTFileName { get; set; }        
    }

    public sealed class Propertie
    {
        [YamlMember(Alias = "Debug", ApplyNamingConventions = false)]
        public bool Debug { get; set; }
    }

    public sealed class Module
    {
        [YamlMember(Alias = "Enable", ApplyNamingConventions = false)]
        public bool Enable { get; set; }

        [YamlMember(Alias = "Launcher", ApplyNamingConventions = false)]
        public string Launcher { get; set; }

        [YamlMember(Alias = "Frequency", ApplyNamingConventions = false)]
        public int Frequency { get; set; }

        [YamlMember(Alias = "Arguments", ApplyNamingConventions = false)]
        public string Arguments { get; set; }

        [YamlMember(Alias = "IIOT.Path", ApplyNamingConventions = false)]
        public string FilePath { get; set; }
    }
}
