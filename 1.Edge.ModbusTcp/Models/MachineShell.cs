using Lib.Common.Components.Agreements;
using System.Collections.Generic;

namespace Edge.ModbusTcp.Models
{
    internal class MachineShell
    {
        public string MachineNo { get; set; }
        public bool Production { get; set; }
        public string Vesion { get; set; }
        public string Address { get; set; }
        public string CollectionMode { get; set; }
        public string CommandMode { get; set; }
        public string Quartz { get; set; }
        public int Port { get; set; }
        public List<MessageBox> MessageBoxes { get; set; }
    }

    internal class MessageBox
    {
        public HostChannel Channel { get; set; }
        public int FunctionCode { get; set; }
        public byte SlaveAddress { get; set; }
        public ushort StartAddress { get; set; }
        public ushort Offset { get; set; }
        public List<PickPoint> PickPoints { get; set; }
    }

    internal class PickPoint
    {
        public int PointNo { get; set; }
        public string AttribName { get; set; }
        //Add By YanHao - 20210325
        public string AttribType { get; set; }
    }
}
