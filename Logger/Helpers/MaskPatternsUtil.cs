using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyLoggerNamespace.Helpers
{   
    /// <summary>
    /// Хелпер с методами расширения для маскирования записей
    /// </summary>
    internal static class MaskPatternsUtil
    {
        private const RegexOptions _matchOptions = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;

        #region функции маскирования
        private static Func<string, string> _maskFPan = (s) => $"{s.Substring(0, 6)}XXXXXX{s.Substring(s.Length - 4, 4)}";    // mask PAN with XXXXXX in the middle
        private static Func<string, string> _maskFToXXX = (s) => new string('X', string.IsNullOrEmpty(s) ? 0 : s.Count());    // change all symbols to XXXXX
        private static Func<string, string> _maskCutXXX = (s) => "XXX";                                                       // replase a long string with "XXX"
        private static Func<string, string> _maskBin = (s) => $"{s.Substring(0, 6)}{new string('X', s.Count() - 6)}";         // mask Bin with XXXXXX after first 6 digits
        private static Func<string, string> _maskBigBody = (s) =>
        {
            if (string.IsNullOrWhiteSpace(s) || s.Count() <= 250)
                return s;
            return s.Substring(0, 250);
        };
        #endregion

        #region строки маскирования

        private static readonly string _maskPDF = " PDF ";

        #endregion

        #region регулярки для маскирования request'ов и respons'ов нашего api
        private static readonly Regex _apiPatternVW1 = new Regex("\"(PaReq|PaRes|VWUserPsw)\":\"([\\w*\\s*/*]+)", _matchOptions);
        private static readonly Regex _apiPatternVW2 = new Regex(@"\\?""(PaReq|PaRes|VWUserPsw|Password|PAN|pass|CVV|CVC2|SecureCode|CardNumber|DPAN)\\?"":\s*\\?""(.*?)\\?""", _matchOptions | RegexOptions.Multiline);

        private static readonly Regex _apiPatternPayInfo1 = new Regex(@"(PayInfo|_payInfo)=([^&]*)", _matchOptions);
        private static readonly Regex _apiPatternPayInfo2 = new Regex("\"(PayInfo|_payInfo)\":\"([^\"]*)", _matchOptions);

        private static readonly Regex _apiPatternPares = new Regex(@"(PaRes|[\W]DATA|ApplePayToken)=([\w*\s*/*]+)", _matchOptions);
        private static readonly Regex _apiPatternEmail = new Regex(@"(?:%22|"")(EmailContent)(?:%22|"")(?:%22|["": ]|$|\n)*(.+?)(?:%22|"",)", _matchOptions);
        private static readonly Regex _apiPatternAttaches = new Regex(@"(?:%22|"")(Attaches)(?:%22|"")(?:%22|[[{"": ]|$|\n)*([\s\S]+?)(?:])", _matchOptions);
        #endregion

        #region регулярки для маскирования логов
        private static readonly Regex _logPatternPan1 = new Regex("(PAN|DPAN)%3d(\\d{12,20})%26", _matchOptions);
        private static readonly Regex _logPatternPan2 = new Regex("(PanHash)=([^;&]*)", _matchOptions);

        private static readonly Regex _logPatternCard1 = new Regex("(PAN|CardNumber|Number|DPAN)=\"?(\\d{12,20})\"?", _matchOptions);
        private static readonly Regex _logPatternCard2 = new Regex("\"(PAN|CardNumber|Number|DPAN)\":\"(\\d{12,20})\"", _matchOptions);
        private static readonly Regex _logPatternCard3 = new Regex(@"(CardTo)=(\d{12,20})", _matchOptions);

        private static readonly Regex _logPatternCvv1 = new Regex("(SecureCode|CVC2|CVV)=\"?(\\d{0,4})\"?", _matchOptions);
        private static readonly Regex _logPatternCvv2 = new Regex("(CVV)%3d(\\d{0,4})%26", _matchOptions);
        private static readonly Regex _logPatternCvv3 = new Regex("\"(SecureCode|CVC2|CVV)\":\"(\\d{0,4})\"", _matchOptions);

        private static readonly Regex _logPatternPass1 = new Regex(@"(Password)=(.*?(?=[,;&])|.*)", _matchOptions);
        private static readonly Regex _logPatternPass2 = new Regex(@"(RepeatPassword)=(.*?(?=[,;&])|.*)", _matchOptions);
        private static readonly Regex _logPatternPass3 = new Regex(@"(pass)=(.*?(?=[,;&])|.*)", _matchOptions);
        private static readonly Regex _logPatternPass4 = new Regex(@"(VWUserPsw)=(.*?(?=[,;&])|.*)", _matchOptions);
        private static readonly Regex _logPatternPass5 = new Regex("(\"Password\"):(\".*?(?=\"[,;&])|.*\")", _matchOptions);

        private static readonly Regex _logPatternBin1 = new Regex("(Bin\")=\"?(\\d{6,20})", _matchOptions);
        private static readonly Regex _logPatternBin2 = new Regex("(Bin>)(\\d{6,20})", _matchOptions);

        private static readonly Regex _logPatternPDF = new Regex(@"PDF.+EOF", _matchOptions);
        #endregion

        private static readonly Dictionary<Regex, Func<string, string>> _patternsFunc = new Dictionary<Regex, Func<string, string>>
        {
            { _apiPatternVW1,_maskCutXXX },
            { _apiPatternVW2, _maskCutXXX },
            { _apiPatternPayInfo1, _maskCutXXX },
            { _apiPatternPayInfo2, _maskCutXXX}, // для json
            { _apiPatternPares, _maskCutXXX },
            { _apiPatternEmail, _maskBigBody },
            { _apiPatternAttaches, _maskBigBody }
        };

        private static readonly Dictionary<Regex, Func<string, string>> _patternsFuncForLog = new Dictionary<Regex, Func<string, string>>()
        {
            { _logPatternPan1, _maskFPan },
            { _logPatternPan2, _maskCutXXX },
            { _logPatternCard1, _maskFPan },
            { _logPatternCard2, _maskFPan },
            { _logPatternCard3, _maskFPan },
            { _logPatternCvv1, _maskFToXXX },
            { _logPatternCvv2, _maskFToXXX },
            { _logPatternCvv3, _maskFToXXX },
            { _logPatternPass1, _maskFToXXX },
            { _logPatternPass2, _maskFToXXX },
            { _logPatternPass3, _maskFToXXX },
            { _logPatternPass4, _maskFToXXX },
            { _logPatternPass5, _maskCutXXX},
            { _logPatternBin1, _maskBin },
            { _logPatternBin2, _maskBin }
        };

        /// <summary>
        /// Паттерны для замены лога файла (непонятные символы) на котороткий и однозначный лог
        /// </summary>
        private static Dictionary<Regex, string> _patternsFile = new Dictionary<Regex, string>
        {
            { _logPatternPDF, _maskPDF }
        };

        static MaskPatternsUtil()
        {
            // наполняем словать _patternsFunc общими паттернами для логов
            foreach (var item in _patternsFuncForLog)
                _patternsFunc.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Маскирует строку по паттернам для API
        /// </summary>
        /// <param name="data">Исходная строка</param>
        /// <returns></returns>
        public static string MaskedApi(this string data) => FindPatternAndMasked(data, _patternsFunc);

        /// <summary>
        /// Маскирует строку по паттернам для логов
        /// </summary>
        /// <param name="data">Исходная строка</param>
        /// <returns></returns>
        public static string MaskedLog(this string data) => FindPatternAndMasked(data, _patternsFuncForLog);

        /// <summary>
        /// Перебирает словарь regex'ов и функций маскирования. При нахождении соотв. маскирует. Если не нашел то возвращает исходную строку
        /// </summary>
        /// <param name="data">Исходная строка</param>
        /// <returns></returns>
        private static string FindPatternAndMasked(string data, Dictionary<Regex, Func<string, string>> patternDic)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                foreach (var keyPair in patternDic)
                {
                    var matchGroups = keyPair.Key.Matches(data);

                    foreach (Match matchGroup in matchGroups)
                        if (matchGroup.Success && matchGroup.Groups.Count == 3)
                            data = data.Replace(matchGroup.Groups[0].Value, $"{matchGroup.Groups[1].Value}={keyPair.Value(matchGroup.Groups[2].Value)}");
                }

                foreach (var keyPair in _patternsFile)
                {
                    if (keyPair.Key.IsMatch(data))
                    {
                        data = keyPair.Value;
                    }
                }
            }

            return data;
        }
    }
}
