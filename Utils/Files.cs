namespace Utils
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;

    //=================================================================================================
    /// <summary>
    /// Управление файлами.
    /// </summary>
    public class Files
    {
        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Прочитать данные из файла в байт-массив.
        /// </summary>
        /// <param name="p_sFileName">Имя файла.</param>
        /// <returns>Байт-массив с данными.</returns>
        public static byte[] ReadData(string p_sFileName)
        {
            byte[] abBuffer = null;
            using (FileStream objStream = File.Open(p_sFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                abBuffer = new byte[objStream.Length];
                objStream.Read(abBuffer, 0, (int)objStream.Length);
                objStream.Close();
            }

            return abBuffer;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Записать байт-массив в файл.
        /// </summary>
        /// <param name="p_sFileName">Имя файла.</param>
        /// <param name="p_abData">Байт-массив с данными.</param>
        public static void SaveData(string p_sFileName, byte[] p_abData)
        {
            if (p_abData == null)
            {
                throw new ArgumentNullException("p_abData");
            }

            using (FileStream objStream = File.Open(p_sFileName, FileMode.Create, FileAccess.Write))
            {
                objStream.Write(p_abData, 0, p_abData.Length);
                objStream.Close();
            }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Прочитать текст из файла.
        /// </summary>
        /// <param name="p_sFileName">Имя файла.</param>
        /// <returns>Строка с текстом.</returns>
        /// <remarks>Файл читается в кодировке UTF-8</remarks>
        public static string ReadText(string p_sFileName)
        {
            return Files.ReadText(p_sFileName, "utf-8");
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Прочитать текст из файла.
        /// </summary>
        /// <param name="p_sFileName">Имя файла.</param>
        /// <param name="p_sEncodingName">Кодировка в которой нужно прочитать файл.</param>
        /// <returns>Строка с текстом.</returns>
        public static string ReadText(string p_sFileName, string p_sEncodingName)
        {
            if (String.IsNullOrEmpty(p_sEncodingName))
            {
                throw new ArgumentNullException("p_sEncodingName");
            }

            byte[] abBuffer = Files.ReadData(p_sFileName);
            if (abBuffer == null)
            {
                return "";
            }

            Encoding objEncoding = Encoding.GetEncoding(p_sEncodingName);
            if (objEncoding == null)
            {
                throw new ArgumentOutOfRangeException("p_sEncodingName", p_sEncodingName, "Unknown encoding");
            }

            string sText = objEncoding.GetString(abBuffer, 0, abBuffer.Length);

            // check text
            for (int i = 0; i < sText.Length; i++)
            {
                UnicodeCategory eCategory = char.GetUnicodeCategory(sText, i);
                if (eCategory == UnicodeCategory.OtherNotAssigned ||
                    eCategory == UnicodeCategory.PrivateUse)
                {
                    throw new ExException("StringChecker_BadString", "Bad string, may be not in {0} encoding, file: {1} offset: {2}", objEncoding.EncodingName, p_sFileName, i);
                }
            }
            byte[] abEncodedBytes = objEncoding.GetBytes(sText);
            int iOffset;
            if (Bytes.Equal(abBuffer, abEncodedBytes, out iOffset) == false)
            {
                throw new ExException("StringChecker_BadString", "Bad string, may be not in {0} encoding, file: {1} offset: {2}", objEncoding.EncodingName, p_sFileName, iOffset);
            }

            return sText;
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Записать строку в файл.
        /// </summary>
        /// <param name="p_sFileName">Имя файла.</param>
        /// <param name="p_sData">Строка для записи.</param>
        /// <remarks>Используется кодировка UTF-8.</remarks>
        public static void SaveText(string p_sFileName, string p_sData)
        {
            Files.SaveText(p_sFileName, "utf-8", p_sData);
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Записать строку в файл.
        /// </summary>
        /// <param name="p_sFileName">Имя файла.</param>
        /// <param name="p_sEncodingName">Кодировка для записи.</param>
        /// <param name="p_sData">Строка для записи.</param>
        public static void SaveText(string p_sFileName, string p_sEncodingName, string p_sData)
        {
            if (p_sData == null)
            {
                throw new ArgumentNullException("p_sData");
            }

            if (String.IsNullOrEmpty(p_sEncodingName))
            {
                throw new ArgumentNullException("p_sEncodingName");
            }

            Encoding objEncoding = Encoding.GetEncoding(p_sEncodingName);
            if (objEncoding == null)
            {
                throw new ArgumentOutOfRangeException("p_sEncodingName", p_sEncodingName, "Unknown encoding");
            }

            byte[] abBuffer = objEncoding.GetBytes(p_sData);
            Files.SaveData(p_sFileName, abBuffer);
        }
    }
}
