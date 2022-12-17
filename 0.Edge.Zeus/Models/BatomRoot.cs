using Newtonsoft.Json;

namespace Edge.Zeus.Models
{
    public class BatomRoot
    {
        [JsonProperty(PropertyName = "header")]
        public Header Header { get; set; }

        [JsonProperty(PropertyName = "content")]
        public Content Content { get; set; }
    }

    public class Header
    {
        [JsonProperty(PropertyName = "method_fullname")]
        public string MethodFullname { get; set; }

        [JsonProperty(PropertyName = "method_version")]
        public string MethodVersion { get; set; }
    }

    public class Content
    {
        [JsonProperty(PropertyName = "req_data")]
        public Req_Data ReqData { get; set; }
    }

    public class Req_Data
    {
        [JsonProperty(PropertyName = "attrib_no")]
        public string Attrib_no { get; set; }

        [JsonProperty(PropertyName = "attrib_value")]
        public string AttribValue { get; set; }

        [JsonProperty(PropertyName = "machine_no")]
        public string MachineNo { get; set; }

        [JsonProperty(PropertyName = "plot_no")]
        public string PlotNo { get; set; }

        [JsonProperty(PropertyName = "carrier_no")]
        public string CarrierNo { get; set; }

        [JsonProperty(PropertyName = "report_datetime")]
        public string ReportDatetime { get; set; }
    }
}
