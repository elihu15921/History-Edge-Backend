using System.Collections.Generic;

namespace Lib.Common.Components.Models
{
    public class PayloadRoot
    {
        public string Version { get; set; }
        public bool Production { get; set; }
        public string MachineNo { get; set; }
        public string ReportDateTime { get; set; }
        public List<Parameter> Row { get; set; }
    }

    public class Parameter
    {
        public string AttribNo { get; set; }
        //Modify By YanHao
        //public ushort AttribValue { get; set; }
        public string AttribValue { get; set; }
    }
}
