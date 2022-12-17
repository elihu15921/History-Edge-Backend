using Newtonsoft.Json;

namespace Edge.Zeus.Models
{
    public class ResponseRoot
    {
        [JsonProperty(PropertyName = "is_ok")]
        public string IsOk { get; set; }

        [JsonProperty(PropertyName = "result")]
        public Result Result { get; set; }
    }

    public class Result
    {
        [JsonProperty(PropertyName = "error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "sql_code")]
        public Notice SqlCode { get; set; }

        [JsonProperty(PropertyName = "exception")]
        public Notice Exception { get; set; }
    }

    public class Notice
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
