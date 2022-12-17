using Lib.Common.Components.Agreements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Lib.Common.Components.Verification
{
    public class WebClientHelper
    {
        public static Tuple<Condition, string> SendPost(HttpMethodType type, string url, JObject jObject)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Timeout = 3000;
                request.Method = type.ToString();
                request.ContentType = "application/json; charset=utf-8";
                request.Headers.Add("authorization", "token apikey");

                byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject, Formatting.Indented));

                request.ContentLength = buffer.Length;

                request.GetRequestStream().Write(buffer, 0, buffer.Length);

                using StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream(), Encoding.UTF8);

                return new Tuple<Condition, string>(Condition.Success, reader.ReadToEnd());
            }
            catch (Exception e)
            {
                return new Tuple<Condition, string>(Condition.Error, e.Message);
            }
        }
    }
}
