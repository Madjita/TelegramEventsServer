using MyLoggerNamespace.Enums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyLoggerNamespace.Helpers
{
    public static class Misc
    {
        /// <summary>
        /// Получает значение из словаря,если такое имеется
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string GetValueFromKey(this Dictionary<string, string> parameters, string Name)
        {
            if (parameters.ContainsKey(Name)) { return parameters[Name]; }
            return "";
        }

        /// <summary>
        /// формирует строку из значений и ключей словаря через "=" и ";"
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string GetParametersString(this Dictionary<string, string> parameters)
        {
            string result = "";
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                result += string.Format("{0}={1};", parameter.Key, parameter.Value);
            }
            return result;
        }
        /// <summary>
        /// Маскирует данные карты
        /// EMonth, EYear, SecureCode, VWUserPsw, CardNumber, CardNumber
        /// </summary>
        /// <param name="cardData">строка с маскируемыми данными</param>
        /// <returns>замоскированная строка</returns>
        public static string MaskCardData(this string cardData)
        {
            cardData = Regex.Replace(cardData, "(EMonth%3[dD])([0-9]{1,2})", "$1xx");
            cardData = Regex.Replace(cardData, "(EYear%3[dD])([0-9]{2,4})", "$1xx");
            cardData = Regex.Replace(cardData, "(SecureCode%3[dD])([0-9]{3})", "$1xxx");
            cardData = Regex.Replace(cardData, "(VWUserPsw%3[dD])([^%]*)", "$1xxx");
            cardData = Regex.Replace(cardData, "(Password%3[dD])([^%]*)", "$1xxx");
            cardData = Regex.Replace(cardData, "(CardNumber%3[dD])([0-9]{6})([0-9]{3,9})([0-9]{4})", "$1$2xxxxxx$4");
            cardData = Regex.Replace(cardData, "(PAN%3[dD])([0-9]{6})([0-9]{3,9})([0-9]{4})", "$1$2xxxxxx$4");
            cardData = Regex.Replace(cardData, "(cc%3[dD])([0-9]{6})([0-9]{3,9})([0-9]{4})", "$1$2xxxxxx$4");

            cardData = Regex.Replace(cardData, "(EMonth=)([0-9]{1,2})", "$1xx");
            cardData = Regex.Replace(cardData, "(EYear=)([0-9]{2,4})", "$1xx");
            cardData = Regex.Replace(cardData, "(SecureCode=)([0-9]{3})", "$1xxx");
            cardData = Regex.Replace(cardData, "(VWUserPsw=)([^;]*)", "$1xxx");
            cardData = Regex.Replace(cardData, "(Password=)([^;]*)", "$1xxx");
            cardData = Regex.Replace(cardData, "(CardNumber=)([0-9]{6})([0-9]{3,9})([0-9]{4})", "$1$2xxxxxx$4");
            cardData = Regex.Replace(cardData, "(PAN=)([0-9]{6})([0-9]{3,9})([0-9]{4})", "$1$2xxxxxx$4");
            cardData = Regex.Replace(cardData, "(CardTo=)([0-9]{6})([0-9]{3,9})([0-9]{4})", "$1$2xxxxxx$4");
            cardData = Regex.Replace(cardData, "(cc=)([0-9]{6})([0-9]{3,9})([0-9]{4})", "$1$2xxxxxx$4");
            cardData = Regex.Replace(cardData, "(CardNumber=)(\\d{4} )(\\d{2})(\\d{2} )(\\d{4} )(\\d{4,7})", "$1$2$3xxxxxx$6");
            cardData = Regex.Replace(cardData, "(PAN=)(\\d{4} )(\\d{2})(\\d{2} )(\\d{4} )(\\d{4,7})", "$1$2$3xxxxxx$6");
            cardData = Regex.Replace(cardData, "(cc=)(\\d{4} )(\\d{2})(\\d{2} )(\\d{4} )(\\d{4,7})", "$1$2$3xxxxxx$6");

            // Iyzico
            cardData = Regex.Replace(cardData, "(\"CardNumber\":)(\"\\d{4})(\\d{2})(\\d{2})(\\d{4})(\\d{4,7}\")", "$1$2$3xxxxxx$6");
            cardData = Regex.Replace(cardData, "(\"ExpireYear\":)(\"[0-9]{2,4}\")", "$1\"xxxx\"");
            cardData = Regex.Replace(cardData, "(\"ExpireMonth\":)(\"[0-9]{1,2}\")", "$1\"xx\"");
            cardData = Regex.Replace(cardData, "(\"Cvc\":)(\"[0-9]{3}\")", "$1\"xxx\"");

            return cardData;
        }


        /// <summary>
        /// Маскирует все 16-значные номера, например:
        /// 2225855555555555 → 222585XXXXXX5555
        /// </summary>
        /// <param name="PayInfo"></param>
        /// <returns></returns>
        public static string Mask16dNumbers(this string PayInfo)
        {
            string teststring = PayInfo;
            // \\D — это «не-цифра»

            // ищем 6+6+4 = 16 цифр (номер карты)
            string pattern = "\\D([0-9]{6})([0-9]{6})([0-9]{4})\\D";

            // оставляем первые 6 цифр и последние 4, а серединные 6 меняем на X
            string replacement = "$1XXXXXX$3";

            teststring = Regex.Replace(teststring, pattern, replacement);
            return teststring;
        }

        /// <summary>
        /// Маскирует последовательности цифр от 16 до 19 символов маской 6x4
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string MaskCardPan(this string input)
        {
            string pattern = "(\\D)([0-9]{6})([0-9]{6,9})([0-9]{4})(\\D)";

            string replacement = "$1$2XXXXXX$4$5";

            input = Regex.Replace(input, pattern, replacement);
            return input;
        }

        public static long IPToNumeric(this string IP)
        {
            string[] ipParts = IP.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (ipParts.Length != 4)
            {
                throw new Exception(String.Format("Provided IP address string [{0}] is invalid and can't be transformed to number", IP));
            }

            // wtf: << 24, << 16, << 8
            long numIp = Int64.Parse(ipParts[0]) * 256 * 256 * 256 + Int64.Parse(ipParts[1]) * 256 * 256 + Int64.Parse(ipParts[2]) * 256 + Int64.Parse(ipParts[3]);

            return numIp;
        }

        [SuppressMessage("Compiler", "CS0618:ObsoleteMemberUsage")]
        public static long IpToLong(this string ip)
        {
            return System.Net.IPAddress.Parse(ip).Address;
        }

        public static string LongToIp(this long intAddress)
        {
            return new System.Net.IPAddress(intAddress).ToString();
        }

        public static StringBuilder AppendLineFormat(this StringBuilder sb, string format, params object[] args)
        {
            return sb.AppendFormat(format, args).AppendLine();
        }

        public static string ConvertToString(this Dictionary<string, string> dic, bool additionalNewLineSeparator = false)
        {
            return dic == null
                ? ""
                : string.Join(additionalNewLineSeparator ? Environment.NewLine : string.Empty, dic.Select(kvp => string.Format("{0}:{1}", kvp.Key, kvp.Value)));
        }

        /// <summary>
        /// Преобразовывает словарь параметров в эквивалентную строку параметров
        /// </summary>
        /// <param name="dic">исходный словарь с параметрами</param>
        /// <param name="splitParams">символ разделитель между параметрами, по умолчанию ;</param>
        /// <param name="splitValue">символ разделитель параметр на наименование параметра и его значение, по умолчанию =</param>
        /// <returns>строка параметров</returns>
        public static string ConvertToParams(this Dictionary<string, string> dic, char splitParams = ';', char splitValue = '=', bool insensitiveKey = false, bool insensitiveValue = false)
        {
            string result = dic == null
                            ? string.Empty
                            // Фикс задачи 8673. Поле "шаблон" в бекофисе не отображается из-за переносов строки.
                            : string.Join(string.Empty, dic.Select(kvp => string.Format("{0}{1}{2}{3}", insensitiveKey ? kvp.Key.ToLower() : kvp.Key, splitValue, insensitiveValue ? kvp.Value.ToLower() : kvp.Value, splitParams)));

            // удаляем последний не нужный последний символ
            return result.Remove(result.LastIndexOf(splitParams));
        }

        public static string ConvertToParams(this Dictionary<string, object> dic, char splitParams = ';', char splitValue = '=', bool insensitiveKey = false, bool insensitiveValue = false)
        {
            var newDic = new Dictionary<string, string>();

            foreach (var kvp in dic)
            {
                newDic.Add(kvp.Key, kvp.Value.ToString());
            }

            return newDic.ConvertToParams(splitParams, splitValue, insensitiveKey, insensitiveValue);
        }

        /// <summary>
        /// Конвертирует строку параметров разделенных между собой символом разделителем и разделенные между собой сами параметры разделителем на именоваине параметра и его значение 
        /// </summary>
        /// <param name="source">строка которую нужно преобразовать в словарь</param>
        /// <param name="splitParams">символ разделитель между параметрами, по умолчанию ;</param>
        /// <param name="splitValue">символ разделитель параметр на наименование параметра и его значение, по умолчанию =</param>
        /// <param name="insensitiveKey">Регистронезависимость наименование ключей справочника</param>
        /// <param name="insensitiveValue">Регистронезависимость значений справочника</param>
        /// <param name="customKeysComparer">Специфичный алгоритм сравнения ключей справочника.</param>
        /// <returns>возвращает словарь</returns>
        /// <remarks>
        /// Алгоритм сравнения ключей применяется после их модификации в зависимости от параметра <see cref="insensitiveKey"./>
        /// </remarks>
        public static Dictionary<string, string> ConvertParamsToDictionary(
            this string source,
            char splitParams = ';',
            char splitValue = '=',
            bool insensitiveKey = true,
            bool insensitiveValue = true,
            IEqualityComparer<string> customKeysComparer = null)
        {
            Dictionary<string, string> dic = string.IsNullOrEmpty(source)
                                                ? new Dictionary<string, string>()
                                                : source.Split(new[] { splitParams }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(part => part.Split(splitValue, 2)) // Необходимо ограничить 2, на случай если part содержит splitValue, например "AAAAA=="
                                                    .ToDictionary(split => insensitiveKey ? split[0].ToLower() : split[0],
                                                                    split => insensitiveValue ? split[1].ToLower() : split[1], customKeysComparer);
            return dic;
        }
        public static string ConvertToValuesString(this Dictionary<string, string> dictionary, char splitParams = ';')
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> keyValues in dictionary)
            {
                stringBuilder.Append(keyValues.Value);
                stringBuilder.Append(";");
            }
            var result = stringBuilder.ToString();
            return result.Remove(result.LastIndexOf(splitParams));
        }

        public static List<string> ConvertParamsToList(this string source, char splitParams = ';')
        {
            List<string> list = string.IsNullOrEmpty(source)
                ? null
                : source.Split(new[] { splitParams }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return list;
        }
        /// <summary>
        /// Extracts specific keys and its values as a dictionary from the source, the source is a string of parameters separated by ';' and the value key separator is  '='.
        /// This method is safe to use with Base64 encoded strings.
        /// ParamsKeys are garanteed to be on the returned Dictionary even if they are not contained on the source. 
        /// </summary>
        /// <param name="source">The string to be converted to a dictionary</param>
        /// <param name="keys">Keys we are intending to extract</param>
        /// <returns>Dictionary<string, string></returns>
        public static Dictionary<string, string> GetSpecificParamsOnABase64SaveWay(this string source, params string[] keys)
        {
            var dic = new Dictionary<string, string>();

            var paramsList = source?.ConvertParamsToList() ?? new List<string>();

            foreach (var key in keys)
            {
                dic[key] = paramsList.FirstOrDefault(x => x.Contains(key, StringComparison.OrdinalIgnoreCase))?.Replace($"{key}=", "", StringComparison.OrdinalIgnoreCase) ?? "";
            }

            return dic;
        }

        /// <summary>
        /// Метод удаляет не используемые параметры из справочника (EMonth, ExpMonth, EYear, ExpYear, SecureCode, CVV)
        /// </summary>
        /// <param name="customParams">справочник в ктором нужно удалить не используемые параметры</param>
        public static void DeleteUnuseCustomParams(this Dictionary<string, string> customParams)
        {
            customParams.Remove("EMonth");
            customParams.Remove("ExpMonth");
            customParams.Remove("EYear");
            customParams.Remove("ExpYear");
            customParams.Remove("SecureCode");
            customParams.Remove("CVV");
        }

        /// <summary>
        /// Конвертирует строку параметров разделенных между собой символом разделителем и разделенные между собой сами параметры разделителем на именоваине параметра и его значение.
        /// Не выбрасывает исключение при наличии дублирующихся ключей, добавляет в словарь последнее значение.
        /// </summary>
        /// <param name="source">строка которую нужно преобразовать в словарь</param>
        /// <param name="splitParams">символ разделитель между параметрами, по умолчанию ;</param>
        /// <param name="splitValue">символ разделитель параметр на наименование параметра и его значение, по умолчанию =</param>
        /// <param name="insensitiveKey">Регистронезависимость наименование ключей справочника</param>
        /// <param name="insensitiveValue">Регистронезависимость значений справочника</param>
        /// <returns>возвращает словарь</returns>
        public static Dictionary<string, string> ConvertParamsWithNonUniqueKeysToDictionary(this string source, char splitParams = ';', char splitValue = '=', bool insensitiveKey = true, bool insensitiveValue = true)
        {
            if (string.IsNullOrEmpty(source))
                return new Dictionary<string, string>();

            return source.Split(new[] { splitParams }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split(splitValue))
                .Select(split => (split[0], $"{split[1]}{new String(splitValue, split.Length - 2)}"))
                .GroupBy(kvp => insensitiveKey ? kvp.Item1.ToLower() : kvp.Item1)
                .ToDictionary(g => g.Key, g => insensitiveValue ? g.Last().Item2.ToLower() : g.Last().Item2);
        }


        public static int? RunExternalCommandGetResultCode(string executable, string arguments, Logger logger = null, string loggerPrefix = "", int timeoutMs = -1, bool useShellExec = true)
        {
            Process exCmd = new Process { StartInfo = { FileName = executable, Arguments = arguments } };
            //settings up parameters for the process
            if (logger != null)
            {
                logger.WriteLine(MessageType.Info, "[Misc.RunExternalCommandGetResultCode]{0}:{1} {2}. TimeoutMs = {3}", loggerPrefix, exCmd.StartInfo.FileName, exCmd.StartInfo.Arguments, timeoutMs);
            }
            exCmd.StartInfo.UseShellExecute = useShellExec;
            exCmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (!File.Exists(executable))
            {
                if (logger != null)
                    logger.WriteLine(MessageType.Error, "[Misc.RunExternalCommandGetResultCode]{0} File {1} does not exist. ", loggerPrefix, executable);
                return -2;
            }

            exCmd.Start();
            exCmd.WaitForExit(timeoutMs);

            int exitCode = exCmd.ExitCode;
            return exitCode;
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer;
            int num;

            buffer = new byte[4096];
            num = input.Read(buffer, 0, (int)buffer.Length);

            if (num > 0)
            {
                output.Write(buffer, 0, num);
            }
        }

        public static int? RunExternalCommand(string executable, string arguments, Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("Argument inputStream can not be null.");
            }
            if (outputStream == null)
            {
                throw new ArgumentNullException("Argument outputStream can not be null.");
            }
            if (inputStream.CanRead == false)
            {
                throw new ArgumentException("Argument inputStream must be readable.");
            }
            if (outputStream.CanWrite == false)
            {
                throw new ArgumentException("Argument outputStream must be writable.");
            }

            Process exCmd = new Process
            {
                StartInfo =
                                {
                                    FileName = executable,
                                    Arguments = arguments,
                                    WindowStyle = ProcessWindowStyle.Hidden,
                                    CreateNoWindow = true,
                                    UseShellExecute = false,
                                    RedirectStandardInput = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true
                                }
            };
            exCmd.Start();

            CopyStream(inputStream, exCmd.StandardInput.BaseStream);
            exCmd.StandardInput.Flush();
            exCmd.StandardInput.Close();

            exCmd.WaitForExit();

            Stream stream = exCmd.StandardOutput.BaseStream;
            byte[] buffer = new byte[4096];
            int num = stream.Read(buffer, 0, buffer.Length);
            outputStream.Write(buffer, 0, num);

            int exitCode = exCmd.ExitCode;
            return exitCode;
        }

        public static int? RunGPGCommand(string executable, string arguments, byte[] input, string inputFullFileName, ref byte[] output, string encryptedFileName, Logger logger = null)
        {
            if (input == null)
            {
                if (logger != null)
                {
                    logger.WriteLine(MessageType.Error, "[Misc.RunExternalCommand] Argument input can not be null. Parameters: [{0}] [{1}]", executable, arguments);
                }
                return null;
            }
            try
            {
                // выполнения программы gpg.exe нам нужен входной файл. без входного файла на диске не хочет шифровать, поэтому приходиться делать финт ушам
                // создавать файл, сохранять данные - программа выполняет и шифрует и потом можно этот файл удалять
                if (string.IsNullOrEmpty(inputFullFileName))
                {
                    if (logger != null)
                    {
                        logger.WriteLine(MessageType.Error,
                                          "[Misc.RunExternalCommand] Parameter inputFullFileName can not be null.");
                    }
                    return null;
                }

                using (FileStream inputFile = new FileStream(inputFullFileName, FileMode.Create))
                {
                    inputFile.Write(input, 0, input.Length);
                }

                Process exCmd = new Process
                {
                    StartInfo =
                                    {
                                        FileName = executable,
                                        Arguments = arguments,
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                        CreateNoWindow = true,
                                        UseShellExecute = false,
                                        RedirectStandardInput = true,
                                        RedirectStandardOutput = true,
                                        RedirectStandardError = true
                                    }
                };
                logger.WriteLine(MessageType.Info,
                                          "[Misc.RunExternalCommand] FileName:{0} Arguments:{1}", executable, arguments);
                exCmd.Start();

                exCmd.StandardInput.BaseStream.Write(input, 0, input.Length);

                exCmd.StandardInput.Flush();
                exCmd.StandardInput.Close();

                exCmd.WaitForExit();

                if (!File.Exists(encryptedFileName))
                {
                    if (logger != null)
                    {
                        logger.WriteLine(MessageType.Error, "[Misc.RunExternalCommand] Fail create output file: [{0}]", encryptedFileName);
                    }
                    return null;
                }
                using (FileStream fs = new FileStream(encryptedFileName, FileMode.Open))
                {
                    output = new byte[fs.Length];
                    fs.Read(output, 0, (int)fs.Length);
                }

                return exCmd.ExitCode;
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.WriteLine(MessageType.Error, "[Misc.RunExternalCommand] Fail run external command. Parameters: [{0}] [{1}]. Message:{2}", executable, arguments, ex.Message);
                    logger.Write(ex);
                }
                return null;
            }
            finally
            {
                try
                {
                    // удаляю не нужный временные файлы
                    if (!string.IsNullOrEmpty(inputFullFileName)
                        && File.Exists(inputFullFileName))
                    {
                        File.Delete(inputFullFileName);
                    }

                    if (File.Exists(encryptedFileName))
                    {
                        File.Delete(encryptedFileName);
                    }
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        logger.WriteLine(MessageType.Error, "[Misc.RunExternalCommand] Fail delete temp input file. Parameters: [{0}] [{1}]", executable, arguments);
                        logger.Write(ex);
                    }
                }
            }
        }

        public static List<string> GetUnZipString(Stream streamData)
        {
            List<string> zipString = new List<string>();

            using (ICSharpCode.SharpZipLib.Zip.ZipFile zf = new ICSharpCode.SharpZipLib.Zip.ZipFile(streamData))
            {
                foreach (ZipEntry zipEntry in zf)
                {
                    // игнорируем директории
                    if (!zipEntry.IsFile)
                    {
                        continue;
                    }

                    using (Stream inputStream = zf.GetInputStream(zipEntry))
                    {
                        //MemoryStream outputstream = new MemoryStream();
                        //CopyStream( inputStream, outputstream );
                        //outputstream.Position = 0;

                        // вытаскиваем из заархивированного файла данные в виде потока
                        //zipString.Add( outputstream );

                        using (StreamReader reader = new StreamReader(inputStream, Encoding.Unicode))
                        {
                            zipString.Add(reader.ReadToEnd());
                        }
                    }
                }
                zf.IsStreamOwner = true;
                zf.Close();
            }
            return zipString;
        }

        public static int ToInt(this IConvertible convertible)
        {
            return convertible.ToInt32(CultureInfo.InvariantCulture);
        }

        public static T2[] SelectArray<T1, T2>(this IEnumerable<T1> enumerable, Func<T1, T2> selector)
        {
            return enumerable != null
                ? enumerable.Select(selector).ToArray()
                : null;
        }

        public static string Transliterate(this string s)
        {
            if (s == null) return "";

            var dic = new Dictionary<char, string>
            {
                {'а', "a"},
                {'б', "b"},
                {'в', "v"},
                {'г', "g"},
                {'д', "d"},
                {'е', "e"},
                {'ё', "yo"},
                {'ж', "zh"},
                {'з', "z"},
                {'и', "i"},
                {'й', "y"},
                {'к', "k"},
                {'л', "l"},
                {'м', "m"},
                {'н', "n"},
                {'о', "o"},
                {'п', "p"},
                {'р', "r"},
                {'с', "s"},
                {'т', "t"},
                {'у', "u"},
                {'ф', "f"},
                {'х', "h"},
                {'ц', "ts"},
                {'ч', "ch"},
                {'ш', "sh"},
                {'щ', "shch"},
                {'ъ', "'"},
                {'ы', "y"},
                {'ь', "'"},
                {'э', "e"},
                {'ю', "yu"},
                {'я', "ya"}
            };

            var sb = new StringBuilder();

            foreach (char c in s)
            {
                bool isUpper = char.IsUpper(c);

                char cl = char.ToLower(c);

                if (dic.ContainsKey(cl))
                {
                    string sl = dic[cl];

                    if (isUpper)
                    {
                        sb.Append(char.ToUpper(sl[0])).Append(sl.Substring(1));
                    }
                    else
                    {
                        sb.Append(sl);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Метод возвращает все индексы указаной строки
        /// </summary>
        /// <param name="str">строка в которой производиться поиск</param>
        /// <param name="value">строка которую нужно искать</param>
        /// <returns>список индексов</returns>
        public static List<int> AllIndexesOf(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return null;
            }

            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                {
                    return indexes;
                }
                indexes.Add(index);
            }
        }

        public static string StreamToString(this Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GenerateNumber(int orderLength = 9, Logger logger = null)
        {
            try
            {
                string sGuid = Guid.NewGuid().ToString();
                int symbolNum = 0;
                bool bSuccess = false;

                StringBuilder sbId = new StringBuilder();

                while (!bSuccess && symbolNum < sGuid.Length)
                {
                    char c = sGuid[symbolNum];
                    if (c >= 48 && c <= 57)
                    {
                        sbId.Append(c);
                    }

                    symbolNum++;

                    if (sbId.Length == orderLength)
                    {
                        bSuccess = true;
                    }

                    if (symbolNum == sGuid.Length)
                    {
                        sGuid = Guid.NewGuid().ToString();
                        symbolNum = 0;
                    }
                }
                return sbId.ToString();
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.WriteLine(MessageType.Error, "[Misc.GenerateNumber] Произошла ошибка при генерации числового значения длинной:[{0}]", orderLength);
                    logger.Write(ex);
                }
                return string.Empty;
            }
        }

        public static int? TryParseNullable(this string val)
        {
            int outValue;
            return int.TryParse(val, out outValue) ? (int?)outValue : null;
        }

        public static Dictionary<string, string> RemoveHtmlTagFromValues(this Dictionary<string, string> dict, string tag)
        {
            if (dict == null || dict.Count == 0)
                return dict;

            if (tag == "*") tag = ".*?";

            string pattern = $"<{tag}>.*?</{tag}>";

            foreach (var key in new List<string>(dict.Keys))
            {
                dict[key] = RemoveScriptTags(dict[key], pattern);
            }

            return dict;
        }

        private static string RemoveScriptTags(string inString, string pattern)
        {
            if (string.IsNullOrWhiteSpace(inString))
                return inString;

            return Regex.Replace(inString, pattern, string.Empty);
        }

        public static class EmailValidator
        {
            private const string AtomCharacters = "!#$%&'*+-/=?^_`{|}~";

            private static bool IsLetterOrDigit(char c)
            {
                return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
            }

            private static bool IsAtom(char c)
            {
                return IsLetterOrDigit(c) || AtomCharacters.IndexOf(c) != -1;
            }

            private static bool IsDomain(char c)
            {
                return IsLetterOrDigit(c) || c == '-';
            }

            private static bool SkipAtom(string text, ref int index)
            {
                int startIndex = index;

                while (index < text.Length && IsAtom(text[index]))
                    index++;

                return index > startIndex;
            }

            private static bool SkipSubDomain(string text, ref int index)
            {
                if (!IsDomain(text[index]) || text[index] == '-')
                    return false;

                index++;

                while (index < text.Length && IsDomain(text[index]))
                    index++;

                return true;
            }

            private static bool SkipDomain(string text, ref int index)
            {
                if (!SkipSubDomain(text, ref index))
                    return false;

                while (index < text.Length && text[index] == '.')
                {
                    index++;

                    if (index == text.Length)
                        return false;

                    if (!SkipSubDomain(text, ref index))
                        return false;
                }

                return true;
            }

            private static bool SkipQuoted(string text, ref int index)
            {
                bool escaped = false;

                // skip over leading '"'
                index++;

                while (index < text.Length)
                {
                    if (text[index] == (byte)'\\')
                    {
                        escaped = !escaped;
                    }
                    else if (!escaped)
                    {
                        if (text[index] == (byte)'"')
                            break;
                    }
                    else
                    {
                        escaped = false;
                    }

                    index++;
                }

                if (index >= text.Length || text[index] != (byte)'"')
                    return false;

                index++;

                return true;
            }

            private static bool SkipWord(string text, ref int index)
            {
                if (text[index] == (byte)'"')
                    return SkipQuoted(text, ref index);

                return SkipAtom(text, ref index);
            }

            private static bool SkipIPv4Literal(string text, ref int index)
            {
                int groups = 0;

                while (index < text.Length && groups < 4)
                {
                    int startIndex = index;
                    int value = 0;

                    while (index < text.Length && text[index] >= '0' && text[index] <= '9')
                    {
                        value = (value * 10) + (text[index] - '0');
                        index++;
                    }

                    if (index == startIndex || index - startIndex > 3 || value > 255)
                        return false;

                    groups++;

                    if (groups < 4 && index < text.Length && text[index] == '.')
                        index++;
                }

                return groups == 4;
            }

            private static bool IsHexDigit(char c)
            {
                return (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f') || (c >= '0' && c <= '9');
            }

            // This needs to handle the following forms:
            //
            // IPv6-addr = IPv6-full / IPv6-comp / IPv6v4-full / IPv6v4-comp
            // IPv6-hex  = 1*4HEXDIG
            // IPv6-full = IPv6-hex 7(":" IPv6-hex)
            // IPv6-comp = [IPv6-hex *5(":" IPv6-hex)] "::" [IPv6-hex *5(":" IPv6-hex)]
            //             ; The "::" represents at least 2 16-bit groups of zeros
            //             ; No more than 6 groups in addition to the "::" may be
            //             ; present
            // IPv6v4-full = IPv6-hex 5(":" IPv6-hex) ":" IPv4-address-literal
            // IPv6v4-comp = [IPv6-hex *3(":" IPv6-hex)] "::"
            //               [IPv6-hex *3(":" IPv6-hex) ":"] IPv4-address-literal
            //             ; The "::" represents at least 2 16-bit groups of zeros
            //             ; No more than 4 groups in addition to the "::" and
            //             ; IPv4-address-literal may be present
            private static bool SkipIPv6Literal(string text, ref int index)
            {
                bool compact = false;
                int colons = 0;

                while (index < text.Length)
                {
                    int startIndex = index;

                    while (index < text.Length && IsHexDigit(text[index]))
                        index++;

                    if (index >= text.Length)
                        break;

                    if (index > startIndex && colons > 2 && text[index] == '.')
                    {
                        // IPv6v4
                        index = startIndex;

                        if (!SkipIPv4Literal(text, ref index))
                            return false;

                        break;
                    }

                    int count = index - startIndex;
                    if (count > 4)
                        return false;

                    if (text[index] != ':')
                        break;

                    startIndex = index;
                    while (index < text.Length && text[index] == ':')
                        index++;

                    count = index - startIndex;
                    if (count > 2)
                        return false;

                    if (count == 2)
                    {
                        if (compact)
                            return false;

                        compact = true;
                        colons += 2;
                    }
                    else
                    {
                        colons++;
                    }
                }

                if (colons < 2)
                    return false;

                if (compact)
                    return colons < 6;

                return colons < 7;
            }

            /// <summary>
            /// Validate the specified email address.
            /// </summary>
            /// <returns><c>true</c> if the email address is valid; otherwise <c>false</c>.</returns>
            /// <param name="email">An email address.</param>
            public static bool Validate(string email)
            {
                int index = 0;

                if (email.Length == 0)
                    return false;

                if (!SkipWord(email, ref index) || index >= email.Length)
                    return false;

                while (index < email.Length && email[index] == '.')
                {
                    index++;

                    if (!SkipWord(email, ref index) || index >= email.Length)
                        return false;
                }

                if (index + 1 >= email.Length || email[index++] != '@')
                    return false;

                if (email[index] != '[')
                {
                    // domain
                    if (!SkipDomain(email, ref index))
                        return false;

                    return index == email.Length;
                }

                // address literal
                index++;

                // we need at least 8 more characters
                if (index + 8 >= email.Length)
                    return false;

                var ipv6 = email.Substring(index, 5);
                if (ipv6.ToLowerInvariant() == "ipv6:")
                {
                    index += "IPv6:".Length;
                    if (!SkipIPv6Literal(email, ref index))
                        return false;
                }
                else
                {
                    if (!SkipIPv4Literal(email, ref index))
                        return false;
                }

                if (index >= email.Length || email[index++] != ']')
                    return false;

                return index == email.Length;
            }
        }

        public static bool IsNumber(this object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        public static bool TryCompareNumbers(object a, object b, out int? result)
        {
            result = null;
            try
            {
                if (!(a is sbyte
                    || a is byte
                    || a is short
                    || a is int
                    || a is long
                    || b is sbyte
                    || b is byte
                    || b is short
                    || b is int
                    || b is long))
                {
                    var c = Convert.ToDouble(a);
                    var d = Convert.ToDouble(b);
                    result = c.CompareTo(d);
                }
                else
                {
                    var c = Convert.ToInt64(a);
                    var d = Convert.ToInt64(b);

                    result = c.CompareTo(d);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
