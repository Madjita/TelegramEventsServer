namespace Utils
{
    using System;
    using System.IO;

    /// <summary>
    /// Вспомогательные функции для работы с байт-массивами.
    /// </summary>
    public class Bytes
    {
        /// <summary>
        /// Сравнение двух байт-массивов
        /// </summary>
        /// <param name="p_abOne">Байт-массив для сравнения.</param>
        /// <param name="p_abTwo">Байт-массив для сравнения.</param>
        /// <returns>true если равны</returns>
        /// <remarks>В качестве байт-массивов можно передавать null. Если два байт-массива null, результат сравнения true.</remarks>
        public static bool Equal(byte[] p_abOne, byte[] p_abTwo)
        {
            int iOffset;
            return Bytes.Equal(p_abOne, p_abTwo, out iOffset);
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Сравнение двух байт-массивов
        /// </summary>
        /// <param name="p_abOne">Байт-массив для сравнения.</param>
        /// <param name="p_abTwo">Байт-массив для сравнения.</param>
        /// <param name="o_iOffset">Смещение в байт-массвие первого различного байта. 0 если массивы равны.</param>
        /// <returns>true если равны.</returns>
        public static bool Equal(byte[] p_abOne, byte[] p_abTwo, out int o_iOffset)
        {
            o_iOffset = 0;

            if (p_abOne == null || p_abTwo == null)
            {
                if (p_abOne == null && p_abTwo == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // check size
            if (p_abOne.Length != p_abTwo.Length)
            {
                return false;
            }

            // check content
            for (int i = 0; i < p_abOne.Length; i++)
            {
                if (p_abOne[i] != p_abTwo[i])
                {
                    o_iOffset = i;
                    return false;
                }
            }

            return true;
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Считать все байты из потока и вернуть байт-массив.
        /// </summary>
        /// <param name="p_objStream">Потока с данными.</param>
        /// <returns>Байт-массив с даннми из потока.</returns>
        /// <remarks>Чтение производится через промежуточный буфер размером в 2048 байт.</remarks>
        public static byte[] ReadStream(Stream p_objStream)
        {
            return Bytes.ReadStream(p_objStream, 2048);
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Считать все байты из потока и вернуть байт-массив.
        /// </summary>
        /// <param name="p_objStream">Потока с данными.</param>
        /// <param name="p_iBufferSize">Размер промежуточного буфера в байтах.</param>
        /// <returns>Байт-массив с даннми из потока.</returns>
        public static byte[] ReadStream(Stream p_objStream, int p_iBufferSize)
        {
            MemoryStream objDataStream = new MemoryStream();
            byte[] abtTemp = new byte[p_iBufferSize];
            int iRes = -1;
            while ((iRes = p_objStream.Read(abtTemp, 0, p_iBufferSize)) > 0)
            {
                objDataStream.Write(abtTemp, 0, iRes);
            }

            return objDataStream.GetBuffer();
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертирует байт-массив в строку из шестнадцатиричных символов.
        /// </summary>
        /// <param name="p_abtData">Байт-массив для конвертации.</param>
        /// <returns>Сконвертированная строка.</returns>
        public static string ToHexString(byte[] p_abtData)
        {
            StringWriter swrTemp = new StringWriter();
            foreach (byte b in p_abtData)
            {
                swrTemp.Write("{0:X2}", b);
            }
            return swrTemp.ToString();
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертирует строку из шестнадцатиричных символов в байт-массив.
        /// </summary>
        /// <param name="p_sData">Строка для конвертации.</param>
        /// <returns>Сконвертированный байт-массив.</returns>
        public static byte[] FromHexString(string p_sData)
        {
            if (p_sData.Length % 2 == 1)
            {
                throw new Exception("Can't parse string into byte[]. Its length is odd.");
            }

            byte[] abtTemp = new byte[p_sData.Length / 2];
            for (int i = 0; i < p_sData.Length; i += 2)
            {
                byte b = Byte.Parse(p_sData.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                abtTemp[i / 2] = b;
            }
            return abtTemp;
        }
    }
}
