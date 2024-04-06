using System.Security.Cryptography;
using System.Text;

namespace Utils
{
    public static class Extensions
    {
        private static string sHashKey = "xlc8BHXbZwwRQgcgHw+Yv0aZyNeRvmY80VRIaYYhra5naGVc4rGlhsMPmTNZprCHIT507PFHLK4x1pxmQ+ZMLFbtFKj/UzN/evWS6sNNheXLSNmpyaG3SCLyXZOSXO1/D6A+k/dPKSSvxZUuucSJwWrnlYbFmD96Al7XNuoRAglI2Rd68jHAtrIhT0sVcQZqipS9kwfPYKNolQZLEhmE+hoXCAe33lrhoXH7XLsdVhxW9kFDw8HijoKmpHSqNFJMmPG8AuGOlLmEVYbXyeNM4itrcYy4r6P02QtCLOdY/8CSK5BLIev4+f9e/vf7IiTGX4d3v4gdsN5rm+nn+G5jrkeNdYXmKoVK/vOgtgsuw9c4haYl6HkBaQsAI4mH1tP7ELOnHRjo/sLb3RPRLP7fqivgX8RfpXuYVeYIfN0SLJUkA9OO857kmv1mDCj1zNPr2eSMoeBBU3ui7g+khDMQYlQqQgRt7EzqWJfXw793W1577IfIy1IEQ7fpm1l6Ghb7D/oVqgz68wKlctwi+W4cxtR+X9aSayEfVKtwQ3FRFG7SnSMrsEULBMoS9IVBK3BR/cvZ/0vs+cuKVRbrttSblIVM4rGHu2DVWM73+dnt/i1BmTiTInbt8J4P9BBndWtnK7iNpG/xBptdEmVOG7+83t1bEd6DU7S+P0MgUpUC9So=";


        public static string AddSpaces(this string p_sValue, bool bAfter, int p_iCount)
        {
            StringBuilder sbValue = new StringBuilder();
            sbValue.Append(p_sValue);
            for (int i = 0; i < p_iCount - p_sValue.Length; i++)
            {
                if (bAfter)
                {
                    sbValue.Append(" ");
                }
                else
                {
                    sbValue.Insert(0, " ");
                }
            }
            return sbValue.ToString();
        }
        public static bool CheckTimeOffSetIsValid(this int? timeOffSet) => timeOffSet.HasValue && CheckTimeOffSetIsValid(timeOffSet.Value);
        public static bool CheckTimeOffSetIsValid(this int timeOffSet) => timeOffSet <= 14 && timeOffSet >= -12;
        public static string GetValue(this string sData, int iStartPos, int iLength)
        {
            return sData.Substring(iStartPos, iLength).Trim();
        }
        public static string Get(this Dictionary<string, string> Dictionary, string Key, string Default)
        {
            string sResult = Dictionary.Get(Key);
            return String.IsNullOrEmpty(sResult) ? Default : sResult;
        }

        public static string Get(this Dictionary<string, string> Dictionary, string Key)
        {
            foreach (KeyValuePair<string, string> curPair in Dictionary)
            {
                if (curPair.Key.ToLower() == Key.ToLower())
                {
                    return curPair.Value.Trim();
                }
            }
            return "";
        }

        public static Dictionary<string, string> Set(this Dictionary<string, string> Dictionary, string Key, string Value)
        {
            foreach (string curKey in Dictionary.Keys)
            {
                if (curKey.ToLower() == Key.ToLower())
                {
                    Dictionary[curKey] = Value;
                    return Dictionary;
                }
            }

            Dictionary.Add(Key, Value);

            return Dictionary;
        }

        public static Dictionary<string, string> AddSafe(this Dictionary<string, string> dictionary, string Key, string Value)
        {
            if (!dictionary.ContainsKey(Key))
                dictionary.Add(Key, Value);

            return dictionary;
        }

        public static T Set<T, T1>(this T Dictionary, string Key, T1 Value) where T : IDictionary<string, T1> where T1 : class
        {
            foreach (string curKey in Dictionary.Keys)
            {
                if (curKey.ToLower() == Key.ToLower())
                {
                    Dictionary[curKey] = Value;
                    return Dictionary;
                }
            }

            Dictionary.Add(Key, Value);

            return Dictionary;
        }


        public static Dictionary<string, string> Add(this Dictionary<string, string> Dictionary, Dictionary<string, string> DictionaryToAdd)
        {
            foreach (KeyValuePair<string, string> curKeyPair in DictionaryToAdd)
            {
                Dictionary.Set(curKeyPair.Key, curKeyPair.Value);
            }

            return Dictionary;
        }

        public static Dictionary<string, string> Delete(this Dictionary<string, string> Dictionary, string Key)
        {
            if (String.IsNullOrEmpty(Key))
            {
                return Dictionary;
            }
            string sKey = String.Empty;
            foreach (KeyValuePair<string, string> curKeyPair in Dictionary)
            {
                if (curKeyPair.Key.ToLower() == Key.ToLower())
                {
                    sKey = curKeyPair.Key;
                    break;
                }
            }

            if (!String.IsNullOrEmpty(sKey))
            {
                Dictionary.Remove(sKey);
            }
            return Dictionary;
        }

        public static string GetValueByPriority(this Dictionary<string, string> Dictionary, params string[] Keys)
        {
            foreach (string curString in Keys)
            {
                string sValue = Dictionary.Get(curString);
                if (!String.IsNullOrEmpty(sValue))
                {
                    return sValue;
                }
            }
            return "";
        }

        public static string FillParams(this string Template, Dictionary<string, string> Params)
        {
            if (String.IsNullOrEmpty(Template))
            {
                return String.Empty;
            }
            string sResult = Template;
            foreach (KeyValuePair<string, string> curPair in Params)
            {
                sResult = sResult.Replace(String.Format("{{{0}}}", curPair.Key.ToLower()), curPair.Value);

                // Предусматриваем возможность задать в шаблоне строку, типа "{какие то символы{paramXXX}какие то символы}",
                // и потом передать параметры Param1, Param2 и т.д.
                bool bLastDigit = "0123456789".Contains(sResult[sResult.Length - 1]);
                if (bLastDigit)
                {
                    string sKey = curPair.Key;
                    for (int i = 0; i <= 9; sKey = sKey.Replace(i.ToString(), ""), i++)
                        ;
                }
            }

            return sResult;
        }

        public static string GetHash(this string sPassword, params string[] Params)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(sPassword);
            foreach (string curChunk in Params)
            {
                sb.Append(curChunk);
            }
            HMACSHA512 hashProvider = new HMACSHA512(Convert.FromBase64String(sHashKey));
            return Convert.ToBase64String(hashProvider.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
        }
        public static Int64 IPToInt(this string IPString)
        {
            //(first octet * 256³) + (second octet * 256²) + (third octet * 256) + (fourth octet)
            string[] arrIPChunks = IPString.Split(new char[] { '.' });
            if (arrIPChunks.Length != 4)
            {
                return 0L;
            }
            try
            {
                Int64 iFirstOctet = Convert.ToInt64(arrIPChunks[0]);
                Int64 iSecondOctet = Convert.ToInt64(arrIPChunks[1]);
                Int64 iThirdOctet = Convert.ToInt64(arrIPChunks[2]);
                Int64 iFourthOctet = Convert.ToInt64(arrIPChunks[3]);
                return iFirstOctet * 16777216 + iSecondOctet * 65536 + iThirdOctet * 256 + iFourthOctet;
            }
            catch
            {
                return 0L;
            }
        }
        /*
        public static Int64 IPToInt( this string IPString )
        {
            //(first octet * 256³) + (second octet * 256²) + (third octet * 256) + (fourth octet)
            string[] arrIPChunks = IPString.Split( new char[] { '.' } );

            if( arrIPChunks.Length != 4 )
            {
                return 0;
            }

            try
            {
                int iFirstOctet = Convert.ToInt32( arrIPChunks[ 0 ] );
                int iSecondOctet = Convert.ToInt32( arrIPChunks[ 1 ] );
                int iThirdOctet = Convert.ToInt32( arrIPChunks[ 2 ] );
                int iFourthOctet = Convert.ToInt32( arrIPChunks[ 3 ] );

                return iFirstOctet * 16777216 + iSecondOctet * 65536 + iThirdOctet * 256 + iFourthOctet;
            }
            catch
            {
                return 0;
            }
        }
        */
        public static Dictionary<string, string> GetParams(this string Data, string Delimiters = ";")
        {
            Dictionary<string, string> colDictionary = new Dictionary<string, string>();
            foreach (string sKeyVal in (String.IsNullOrEmpty(Data) ? "" : Data).Split(Delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                int iEqPos = sKeyVal.IndexOf("=");
                if (iEqPos == -1)
                {
                    continue;
                }
                string sName = (sKeyVal.Remove(iEqPos) ?? "").Trim();
                string sVal = iEqPos == sKeyVal.Length - 1 ? "" : sKeyVal.Substring(iEqPos + 1);

                if (!String.IsNullOrEmpty(sName))
                {
                    colDictionary.Set(sName, sVal);
                }
            }

            return colDictionary;
        }

        public static bool IsValid(this Dictionary<string, string> dicData, params string[] Parameters)
        {
            foreach (string curParam in Parameters)
            {
                if (String.IsNullOrEmpty(dicData.Get(curParam)))
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetString(this Dictionary<string, string> Params, string Delimiter = ";")
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> curPair in Params ?? new Dictionary<string, string>())
            {
                sb.AppendFormat("{0}{1}={2}", sb.Length == 0 ? "" : Delimiter, curPair.Key, curPair.Value);
            }

            return sb.ToString();
        }

        public static string GetString(this List<string> Params, string Delimiter = ";")
        {
            StringBuilder sb = new StringBuilder();

            foreach (string curString in Params ?? new List<string>())
            {
                sb.AppendFormat("{0}{1}", sb.Length == 0 ? "" : Delimiter, curString);
            }

            return sb.ToString();
        }

        public static string CreatePassword(int Length = 12)
        {
            string sSymbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder sb = new StringBuilder();

            Random rnd = new Random(DateTime.Now.Second);
            for (int i = 1; i <= Length; i++)
            {
                sb.Append(sSymbols[rnd.Next(sSymbols.Length)]);
            }
            return sb.ToString();
        }

        public static string Templanate(this string Template, Dictionary<string, string> colParams)
        {
            foreach (KeyValuePair<string, string> curPair in colParams)
            {
                string sParamTempl = "{" + curPair.Key.ToLower() + "}";
                int iIndex = Template.ToLower().IndexOf(sParamTempl);
                while (iIndex != -1)
                {
                    Template = Template.Remove(iIndex, sParamTempl.Length).Insert(iIndex, curPair.Value);
                    iIndex = Template.ToLower().IndexOf(sParamTempl);
                }
            }

            return Template;
        }

        public static string RemoveNonDigits(this string sString, string Except = "")
        {
            if (string.IsNullOrEmpty(sString)) return sString;

            char[] arrExcept = Except.ToCharArray();
            StringBuilder sb = new StringBuilder();
            foreach (Char curChar in sString)
            {
                if ((curChar >= '0' && curChar <= '9') ||
                    arrExcept.FirstOrDefault(chr => chr == curChar) != default(Char))
                {
                    sb.Append(curChar);
                }
            }
            return sb.ToString();
        }

        internal static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source,
                            Func<TSource, TSource> nextItem,
                            Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        internal static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

    }
}
