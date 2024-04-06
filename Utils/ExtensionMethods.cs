using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class ExtensionMethods
    {
        public static string ConvertToKeyValuesStringForQuery(this Dictionary<string, object> dictionary)
        {
            if (dictionary is null)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, object?> keyValues in dictionary)
            {
                if (keyValues.Value is null)
                    continue;

                stringBuilder.Append(keyValues.Key);
                stringBuilder.Append('=');
                stringBuilder.Append(keyValues.Value);
                stringBuilder.Append('&');
            }
            var result = stringBuilder.ToString();
            return result.Remove(result.LastIndexOf('&'));
        }

        public static string ConvertToKeyValuesStringForQuery(this Dictionary<string, string> dictionary)
        {
            if (dictionary is null)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> keyValues in dictionary)
            {
                if (keyValues.Value is null)
                    continue;

                stringBuilder.Append(keyValues.Key);
                stringBuilder.Append('=');
                stringBuilder.Append(keyValues.Value);
                stringBuilder.Append('&');
            }
            var result = stringBuilder.ToString();
            return result.Remove(result.LastIndexOf('&'));
        }
    }
}
