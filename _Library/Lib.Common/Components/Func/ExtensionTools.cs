using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Lib.Common.Components.Func
{
    public static class ExtensionTools
    {
        /// <summary>
        /// First capitalized
        /// </summary>
        public static string FormatFirstCapitalized(this string value)
        {
            TextInfo textInfo = new CultureInfo("es-ES", false).TextInfo;

            return value == null ? null : textInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Verification GUID
        /// </summary>
        public static bool IsGuidByParse(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            return Guid.TryParse(value, out _);
        }

        public static bool IsJsonFormat(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            if ((value.StartsWith("{") && value.EndsWith("}")) || (value.StartsWith("[") && value.EndsWith("]")))
            {
                try
                {
                    var obj = JToken.Parse(value);

                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
            }

            return false;
        }

        public static string ToMD5(this string value)
        {
            using var md5 = MD5.Create();

            var data = md5.ComputeHash(Encoding.UTF8.GetBytes(value));

            StringBuilder result = new();

            for (int i = 0; i < data.Length; i++) result.Append(data[i].ToString("x2"));

            return result.ToString();
        }
    }
}
