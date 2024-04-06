using System.Xml.Schema;

namespace Logger.Helpers
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Вспомогательные функции для работы с XML.
    /// </summary>
    public sealed class XmlUtils
    {
        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Prevent instantiation of XmlUtils
        /// </summary>
        private XmlUtils()
        { }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Количество милисекунд с 1.1.1970
        /// </summary>
        private static long lMillisecondsTill1970;


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Инициализация.
        /// </summary>
        static XmlUtils()
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan vTimeSpan1970 = new TimeSpan(dt1970.Ticks);
            lMillisecondsTill1970 = (long)vTimeSpan1970.TotalMilliseconds;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертация <see cref="XmlDocument"/> в строку с отступами.
        /// </summary>
        /// <param name="p_objXmlDocument">Документ.</param>
        /// <returns>Сконвертированая строка.</returns>
        public static string XmlToString(XmlDocument p_objXmlDocument)
        {
            StringWriter objStringWriter = new StringWriter();
            XmlTextWriter objXmlWriter = new XmlTextWriter(objStringWriter);

            objXmlWriter.Formatting = Formatting.Indented;
            objXmlWriter.Indentation = 1;
            objXmlWriter.IndentChar = '\t';

            p_objXmlDocument.WriteTo(objXmlWriter);

            objXmlWriter.Flush();
            objXmlWriter.Close();

            return objStringWriter.ToString();
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертация <see cref="XmlElement"/> в строку с отступами.
        /// </summary>
        /// <param name="p_objXmlElement">Элемент.</param>
        /// <returns>Сконвертированая строка.</returns>
        public static string XmlToString(XmlElement p_objXmlElement)
        {
            StringWriter objStringWriter = new StringWriter();
            XmlTextWriter objXmlWriter = new XmlTextWriter(objStringWriter);
            objXmlWriter.Formatting = Formatting.Indented;
            p_objXmlElement.WriteTo(objXmlWriter);
            objXmlWriter.Flush();
            objXmlWriter.Close();

            return objStringWriter.ToString();
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертация дочерних элементов <see cref="XmlElement"/> в строку с отступами.
        /// </summary>
        /// <param name="p_objXmlElement">Элемент.</param>
        /// <returns>Сконвертированая строка.</returns>
        public static string XmlChildrenToString(XmlElement p_objXmlElement)
        {
            StringWriter objStringWriter = new StringWriter();
            XmlTextWriter objXmlWriter = new XmlTextWriter(objStringWriter);
            objXmlWriter.Formatting = Formatting.Indented;

            foreach (XmlNode objNode in p_objXmlElement.ChildNodes)
            {
                objNode.WriteTo(objXmlWriter);
                objXmlWriter.WriteString(Environment.NewLine);
            }

            objXmlWriter.Flush();
            objXmlWriter.Close();

            return objStringWriter.ToString();
        }



        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить элемент с информацией о времени и дате.
        /// </summary>
        /// <param name="p_xmlCreator">Документ в который нужно добавить информацию.</param>
        /// <param name="p_dtDateTime">Дата и время.</param>
        /// <returns>Созданный элемент.</returns>
        /// <remarks>Добавляется элемент "Date" со следующими атрибутами "Year", "Month", "Day", "Hour", "Minute", "Second".</remarks>
        public static XmlElement DateTimeToXml(XmlDocument p_xmlCreator, DateTime p_dtDateTime)
        {
            return XmlUtils.DateTimeToXml(p_xmlCreator, p_dtDateTime, "Date");
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить элемент с информацией о времени и дате.
        /// </summary>
        /// <param name="p_xmlCreator">Документ в который нужно добавить информацию.</param>
        /// <param name="p_dtDateTime">Дата и время.</param>
        /// <param name="p_sName">Имя элемента.</param>
        /// <returns>Созданный элемент.</returns>
        /// <remarks>Добавляется элемент "Date" со следующими атрибутами "Year", "Month", "Day", "Hour", "Minute", "Second".</remarks>
        public static XmlElement DateTimeToXml(XmlDocument p_xmlCreator, DateTime p_dtDateTime, string p_sName)
        {
            if (p_xmlCreator == null)
            {
                throw new ArgumentNullException("p_xmlCreator");
            }

            XmlElement xelDate = p_xmlCreator.CreateElement(p_sName);
            XmlUtils.DateTimeToXml(xelDate, p_dtDateTime);

            return xelDate;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить элемент с информацией о времени и дате.
        /// </summary>
        /// <param name="p_xelOutput">Элемент в который нужно добавить информацию.</param>
        /// <param name="p_dtDateTime">Дата и время.</param>
        /// <remarks>Добавляется элемент "Date" со следующими атрибутами "Year", "Month", "Day", "Hour", "Minute", "Second".</remarks>
        public static void AddDateTimeToXml(XmlElement p_xelOutput, DateTime p_dtDateTime)
        {
            XmlUtils.AddDateTimeToXml(p_xelOutput, p_dtDateTime, "Date");
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить элемент с информацией о времени и дате.
        /// </summary>
        /// <param name="p_xelOutput">Элемент в который нужно добавить информацию.</param>
        /// <param name="p_dtDateTime">Дата и время.</param>
        /// <param name="p_sName">Имя элемента.</param>
        /// <remarks>Добавляется элемент со следующими атрибутами "Year", "Month", "Day", "Hour", "Minute", "Second".</remarks>
        public static void AddDateTimeToXml(XmlElement p_xelOutput, DateTime p_dtDateTime, string p_sName)
        {
            if (p_xelOutput == null)
            {
                throw new ArgumentNullException("p_xelOutput");
            }

            XmlElement xelDate = p_xelOutput.OwnerDocument.CreateElement(p_sName);
            p_xelOutput.AppendChild(xelDate);

            XmlUtils.DateTimeToXml(xelDate, p_dtDateTime);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить атрибуты с информацией о времени и дате.
        /// </summary>
        /// <param name="p_xelOutput">Элемент в который нужно добавить информацию.</param>
        /// <param name="p_dtDateTime">Дата и время.</param>
        /// <remarks>Добавляется следующие атрибуты "Year", "Month", "Day", "Hour", "Minute", "Second".</remarks>
        public static void DateTimeToXml(XmlElement p_xelOutput, DateTime p_dtDateTime)
        {
            string[] asNames = { "Year", "Month", "Day", "Hour", "Minute", "Second" };

            int[] aiParts = { p_dtDateTime.Year,
                            p_dtDateTime.Month,
                            p_dtDateTime.Day,
                            p_dtDateTime.Hour,
                            p_dtDateTime.Minute,
                            p_dtDateTime.Second
                            };

            for (int i = 0; i < aiParts.Length; i++)
            {
                XmlAttribute objAttr = p_xelOutput.OwnerDocument.CreateAttribute(asNames[i]);
                p_xelOutput.Attributes.Append(objAttr);
                objAttr.Value = aiParts[i].ToString();
            }

            XmlAttribute objAttrJSMilliseconds = p_xelOutput.OwnerDocument.CreateAttribute("JSMilliseconds");
            p_xelOutput.Attributes.Append(objAttrJSMilliseconds);
            long lMilliseconds = p_dtDateTime.Ticks / 10000L - lMillisecondsTill1970;
            objAttrJSMilliseconds.Value = lMilliseconds.ToString();

            XmlAttribute objAttrDayOfWeek = p_xelOutput.OwnerDocument.CreateAttribute("DayOfWeek");
            p_xelOutput.Attributes.Append(objAttrDayOfWeek);
            objAttrDayOfWeek.Value = p_dtDateTime.DayOfWeek.ToString();
        }




        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает строку из ноды
        /// </summary>
        /// <param name="p_xndNode">Xml нода</param>
        /// <param name="p_sXPath">XPath выражение</param>
        /// <returns>если удалось получить строку из ноды, то она, в обратном случае пустую строку</returns>
        public static string GetStringX(XmlNode p_xndNode, string p_sXPath)
        {
            return XmlUtils.GetString(p_xndNode, p_sXPath, "");
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает строку из ноды
        /// </summary>
        /// <param name="p_xndNode">Xml нода</param>
        /// <returns>строка, если есть нода, в обратном случае пустую строку</returns>
        public static string GetString(XmlNode p_xndNode)
        {
            return XmlUtils.GetString(p_xndNode, "");
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает строку из ноды
        /// </summary>
        /// <param name="p_xndNode">Xml нода</param>
        /// <param name="p_sDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить строку из ноды, то она, в обратном случае значение по умолчанию</returns>
        public static string GetString(XmlNode p_xndNode, string p_sDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_sDefaultValue;
            }

            if (p_xndNode is XmlAttribute)
            {
                return XmlUtils.GetString((XmlAttribute)p_xndNode, p_sDefaultValue);
            }
            else if (p_xndNode is XmlElement)
            {
                return XmlUtils.GetString((XmlElement)p_xndNode, p_sDefaultValue);
            }

            return p_sDefaultValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает строку из ноды
        /// </summary>
        /// <param name="p_xndNode">Xml нода</param>
        /// <param name="p_sXPath">XPath выражение</param>
        /// <param name="p_sDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить строку из ноды, то её, в обратном случае значение по умолчанию</returns>
        public static string GetString(XmlNode p_xndNode, string p_sXPath, string p_sDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_sDefaultValue;
            }

            try
            {
                return XmlUtils.GetString(p_xndNode.SelectSingleNode(p_sXPath), p_sDefaultValue);
            }
            catch (Exception)
            {
                return p_sDefaultValue;
            }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает строку из атрибута
        /// </summary>
        /// <param name="p_xatAttribute">Xml атрибут</param>
        /// <returns>строка, если есть атрибут, в обратном случае пустую строку</returns>
        public static string GetString(XmlAttribute p_xatAttribute)
        {
            return XmlUtils.GetString(p_xatAttribute, "");
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает строку из атрибута
        /// </summary>
        /// <param name="p_xatAttribute">Xml атрибут</param>
        /// <param name="p_sDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить строку из атрибута, то её, в обратном случае значение по умолчанию</returns>
        public static string GetString(XmlAttribute p_xatAttribute, string p_sDefaultValue)
        {
            if (p_xatAttribute == null ||
                String.IsNullOrEmpty(p_xatAttribute.Value))
            {
                return p_sDefaultValue;
            }

            return p_xatAttribute.Value;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает строку из элемента
        /// </summary>
        /// <param name="p_xelElement">Xml элемент</param>
        /// <returns>строка, если есть атрибут, в обратном случае пустую строку</returns>
        public static string GetString(XmlElement p_xelElement)
        {
            return XmlUtils.GetString(p_xelElement, "");
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает строку из элемента
        /// </summary>
        /// <param name="p_xelElement">Xml элемент</param>
        /// <param name="p_sDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить строку из атрибута, то её, в обратном случае значение по умолчанию</returns>
        public static string GetString(XmlElement p_xelElement, string p_sDefaultValue)
        {
            if (p_xelElement == null ||
                String.IsNullOrEmpty(p_xelElement.InnerText))
            {
                return p_sDefaultValue;
            }

            return p_xelElement.InnerText;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает int из xml ноды
        /// </summary>
        /// <param name="p_xndNode">нода</param>
        /// <param name="p_sXPath">XPath выражение</param>
        /// <returns>если удалось получить значение то его, в обратном случае 0</returns>
        public static int GetInt(XmlNode p_xndNode, string p_sXPath)
        {
            return XmlUtils.GetInt(p_xndNode, p_sXPath, 0);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает int из xml ноды
        /// </summary>
        /// <param name="p_xndNode">нода</param>
        /// <param name="p_sXPath">XPath выражение</param>
        /// <param name="p_iDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить значение то его, в обратном случае значение по умолчанию</returns>
        public static int GetInt(XmlNode p_xndNode, string p_sXPath, int p_iDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_iDefaultValue;
            }

            try
            {
                return XmlUtils.GetInt(p_xndNode.SelectSingleNode(p_sXPath), p_iDefaultValue);
            }
            catch (Exception)
            {
                return p_iDefaultValue;
            }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает int из xml ноды
        /// </summary>
        /// <param name="p_xndNode">нода</param>
        /// <returns>если удалось получить значение то его, в обратном случае 0</returns>
        public static int GetInt(XmlNode p_xndNode)
        {
            return XmlUtils.GetInt(p_xndNode, 0);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает int из xml ноды
        /// </summary>
        /// <param name="p_xndNode">нода</param>
        /// <param name="p_iDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить значение то его, в обратном случае значение по умолчанию</returns>
        public static int GetInt(XmlNode p_xndNode, int p_iDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_iDefaultValue;
            }

            if (p_xndNode is XmlAttribute)
            {
                return XmlUtils.GetInt((XmlAttribute)p_xndNode, p_iDefaultValue);
            }
            else if (p_xndNode is XmlElement)
            {
                return XmlUtils.GetInt((XmlElement)p_xndNode, p_iDefaultValue);
            }

            return p_iDefaultValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает int из xml атрибута
        /// </summary>
        /// <param name="p_xatAttribute">атрибут</param>
        /// <returns>если удалось получить значение то его, в обратном случае 0</returns>
        public static int GetInt(XmlAttribute p_xatAttribute)
        {
            return XmlUtils.GetInt(p_xatAttribute, 0);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает int из xml атрибута
        /// </summary>
        /// <param name="p_xatAttribute">атрибут</param>
        /// <param name="p_iDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить значение то его, в обратном случае значение по умолчанию</returns>
        public static int GetInt(XmlAttribute p_xatAttribute, int p_iDefaultValue)
        {
            if (p_xatAttribute == null ||
                String.IsNullOrEmpty(p_xatAttribute.Value))
            {
                return p_iDefaultValue;
            }

            return XmlUtils.GetInt(p_xatAttribute.Value, p_iDefaultValue);
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает int из xml элемента
        /// </summary>
        /// <param name="p_xelElement">элемент</param>
        /// <returns>если удалось получить значение то его, в обратном случае 0</returns>
        public static int GetInt(XmlElement p_xelElement)
        {
            return XmlUtils.GetInt(p_xelElement, 0);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает int из xml элемента
        /// </summary>
        /// <param name="p_xelElement">элемент</param>
        /// <param name="p_iDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить значение то его, в обратном случае значение по умолчанию</returns>
        public static int GetInt(XmlElement p_xelElement, int p_iDefaultValue)
        {
            if (p_xelElement == null ||
                String.IsNullOrEmpty(p_xelElement.InnerText))
            {
                return p_iDefaultValue;
            }

            return XmlUtils.GetInt(p_xelElement.InnerText, p_iDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает int из строки
        /// </summary>
        /// <param name="p_sValue">строка с числом</param>
        /// <returns>int если удалось его достать, в обратном случае 0</returns>
        public static int GetInt(string p_sValue)
        {
            return XmlUtils.GetInt(p_sValue, 0);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает int из строки
        /// </summary>
        /// <param name="p_sValue">строка с числом</param>
        /// <param name="p_iDefaultValue">значение по умолчанию</param>
        /// <returns>int если удалось его достать, в обратном случае значение по умолчанию</returns>
        public static int GetInt(string p_sValue, int p_iDefaultValue)
        {
            int iValue = p_iDefaultValue;
            if (!String.IsNullOrEmpty(p_sValue))
            {
                try
                {
                    iValue = int.Parse(p_sValue);
                }
                catch (Exception)
                { }
            }

            return iValue;
        }

        public static int? GetNullableInt(XmlAttribute p_xatAttribute)
        {
            if (p_xatAttribute == null ||
                 String.IsNullOrEmpty(p_xatAttribute.Value))
            {
                return null;
            }

            try
            {
                return int.Parse(p_xatAttribute.Value);
            }
            catch
            {
                return null;
            }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает bool из xml ноды
        /// </summary>
        /// <param name="p_xndNode">нода</param>
        /// <param name="p_sXPath">XPath выражение</param>
        /// <returns>если удалось получить значение то его, в обратном случае false</returns>
        public static bool GetBool(XmlNode p_xndNode, string p_sXPath)
        {
            return XmlUtils.GetBool(p_xndNode, p_sXPath, false);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает bool из xml ноды
        /// </summary>
        /// <param name="p_xndNode">нода</param>
        /// <param name="p_sXPath">XPath выражение</param>
        /// <param name="p_bDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить значение то его, в обратном случае значение по умолчанию</returns>
        public static bool GetBool(XmlNode p_xndNode, string p_sXPath, bool p_bDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_bDefaultValue;
            }

            try
            {
                return XmlUtils.GetBool(p_xndNode.SelectSingleNode(p_sXPath), p_bDefaultValue);
            }
            catch (Exception)
            {
                return p_bDefaultValue;
            }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает bool из xml ноды
        /// </summary>
        /// <param name="p_xndNode">нода</param>
        /// <returns>если удалось получить значение то его, в обратном случае false</returns>
        public static bool GetBool(XmlNode p_xndNode)
        {
            return XmlUtils.GetBool(p_xndNode, false);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает bool из xml ноды
        /// </summary>
        /// <param name="p_xndNode">нода</param>
        /// <param name="p_bDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить значение то его, в обратном случае значение по умолчанию</returns>
        public static bool GetBool(XmlNode p_xndNode, bool p_bDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_bDefaultValue;
            }

            if (p_xndNode is XmlAttribute)
            {
                return XmlUtils.GetBool((XmlAttribute)p_xndNode, p_bDefaultValue);
            }
            else if (p_xndNode is XmlElement)
            {
                return XmlUtils.GetBool((XmlElement)p_xndNode, p_bDefaultValue);
            }

            return p_bDefaultValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает bool из xml атрибута
        /// </summary>
        /// <param name="p_xatAttribute">атрибут</param>
        /// <returns>если удалось получить значение то его, в обратном случае false</returns>
        public static bool GetBool(XmlAttribute p_xatAttribute)
        {
            return XmlUtils.GetBool(p_xatAttribute, false);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает bool из xml атрибута
        /// </summary>
        /// <param name="p_xatAttribute">атрибут</param>
        /// <param name="p_bDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить значение то его, в обратном случае значение по умолчанию</returns>
        public static bool GetBool(XmlAttribute p_xatAttribute, bool p_bDefaultValue)
        {
            if (p_xatAttribute == null ||
                String.IsNullOrEmpty(p_xatAttribute.Value))
            {
                return p_bDefaultValue;
            }

            return XmlUtils.GetBool(p_xatAttribute.Value, p_bDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает bool из xml элемента
        /// </summary>
        /// <param name="p_xelElement">элемент</param>
        /// <returns>если удалось получить значение то его, в обратном случае false</returns>
        public static bool GetBool(XmlElement p_xelElement)
        {
            return XmlUtils.GetBool(p_xelElement, false);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает bool из xml элемента
        /// </summary>
        /// <param name="p_xelElement">элемент</param>
        /// <param name="p_bDefaultValue">значение по умолчанию</param>
        /// <returns>если удалось получить значение то его, в обратном случае значение по умолчанию</returns>
        public static bool GetBool(XmlElement p_xelElement, bool p_bDefaultValue)
        {
            if (p_xelElement == null ||
                String.IsNullOrEmpty(p_xelElement.InnerText))
            {
                return p_bDefaultValue;
            }

            return XmlUtils.GetBool(p_xelElement.InnerText, p_bDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает bool из строки
        /// </summary>
        /// <param name="p_sValue">строка с булевым значением</param>
        /// <returns>int если удалось его достать, в обратном случае false</returns>
        public static bool GetBool(string p_sValue)
        {
            return XmlUtils.GetBool(p_sValue, false);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает bool из строки
        /// </summary>
        /// <param name="p_sValue">строка с булевым значением</param>
        /// <param name="p_bDefaultValue">значение по умолчанию.</param>
        /// <returns>будево значение если удалось его достать, в обратном случае значение по умолчанию</returns>
        public static bool GetBool(string p_sValue, bool p_bDefaultValue)
        {
            bool bValue = p_bDefaultValue;
            if (!String.IsNullOrEmpty(p_sValue))
            {
                try
                {
                    bValue = bool.Parse(p_sValue);
                }
                catch (Exception)
                { }
            }

            return bValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает Guid из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с guid.</param>
        /// <param name="p_sXPath">Путь до значения.</param>
        /// <returns>Guid если удалось получить его, в обратном случае <see cref="Guid.Empty"/>.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static Guid GetGuid(XmlNode p_xndNode, string p_sXPath)
        {
            return XmlUtils.GetGuid(p_xndNode, p_sXPath, Guid.Empty);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает Guid из <see cref="XmlNode"/>
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с guid.</param>
        /// <param name="p_sXPath">Путь до значения.</param>
        /// <param name="p_gDefaultValue">Значение по умолчанию.</param>
        /// <returns>Guid если удалось получить его из строки, в обратном случае значение по умолчанию.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static Guid GetGuid(XmlNode p_xndNode, string p_sXPath, Guid p_gDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_gDefaultValue;
            }

            try
            {
                return XmlUtils.GetGuid(p_xndNode.SelectSingleNode(p_sXPath), p_gDefaultValue);
            }
            catch (Exception)
            {
                return p_gDefaultValue;
            }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает Guid из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с guid.</param>
        /// <returns>Guid если удалось получить его, в обратном случае <see cref="Guid.Empty"/>.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static Guid GetGuid(XmlNode p_xndNode)
        {
            return XmlUtils.GetGuid(p_xndNode, Guid.Empty);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает Guid из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с guid.</param>
        /// <param name="p_gDefaultValue">Значение по умолчанию.</param>
        /// <returns>Guid если удалось получить его из строки, в обратном случае значение по умолчанию.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static Guid GetGuid(XmlNode p_xndNode, Guid p_gDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_gDefaultValue;
            }

            if (p_xndNode is XmlAttribute)
            {
                return XmlUtils.GetGuid((XmlAttribute)p_xndNode, p_gDefaultValue);
            }
            else if (p_xndNode is XmlElement)
            {
                return XmlUtils.GetGuid((XmlElement)p_xndNode, p_gDefaultValue);
            }

            return p_gDefaultValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает Guid из <see cref="XmlAttribute"/>.
        /// </summary>
        /// <param name="p_xatAttribute"><see cref="XmlAttribute"/> с guid.</param>
        /// <returns>Guid если удалось получить его, в обратном случае <see cref="Guid.Empty"/>.</returns>
        public static Guid GetGuid(XmlAttribute p_xatAttribute)
        {
            return XmlUtils.GetGuid(p_xatAttribute, Guid.Empty);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает Guid из <see cref="XmlAttribute"/>.
        /// </summary>
        /// <param name="p_xatAttribute"><see cref="XmlAttribute"/> с guid.</param>
        /// <param name="p_gDefaultValue">Значение по умолчанию.</param>
        /// <returns>Guid если удалось получить его из строки, в обратном случае значение по умолчанию.</returns>
        public static Guid GetGuid(XmlAttribute p_xatAttribute, Guid p_gDefaultValue)
        {
            if (p_xatAttribute == null ||
                String.IsNullOrEmpty(p_xatAttribute.Value))
            {
                return p_gDefaultValue;
            }

            return XmlUtils.GetGuid(p_xatAttribute.Value, p_gDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает Guid из <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement"><see cref="XmlElement"/> с guid.</param>
        /// <returns>Guid если удалось получить его, в обратном случае <see cref="Guid.Empty"/>.</returns>
        public static Guid GetGuid(XmlElement p_xelElement)
        {
            return XmlUtils.GetGuid(p_xelElement, Guid.Empty);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает Guid из <see cref="XmlElement"/>
        /// </summary>
        /// <param name="p_xelElement"><see cref="XmlElement"/> с guid.</param>
        /// <param name="p_gDefaultValue">Значение по умолчанию.</param>
        /// <returns>Guid если удалось получить его из строки, в обратном случае значение по умолчанию.</returns>
        /// <remarks>Значение берётся из <see cref="XmlElement.InnerText"/></remarks>
        public static Guid GetGuid(XmlElement p_xelElement, Guid p_gDefaultValue)
        {
            if (p_xelElement == null ||
                String.IsNullOrEmpty(p_xelElement.InnerText))
            {
                return p_gDefaultValue;
            }

            return XmlUtils.GetGuid(p_xelElement.InnerText, p_gDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает Guid из строки.
        /// </summary>
        /// <param name="p_sValue">Строка с guid.</param>
        /// <returns>Guid если удалось получить его из строки, в обратном случае <see cref="Guid.Empty"/></returns>
        public static Guid GetGuid(string p_sValue)
        {
            return XmlUtils.GetGuid(p_sValue, Guid.Empty);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает Guid из строки
        /// </summary>
        /// <param name="p_sValue">Строка с guid</param>
        /// <param name="p_gDefaultValue">Значение по умолчанию.</param>
        /// <returns>Guid если удалось получить его из строки, в обратном случае значение по умолчанию.</returns>
        public static Guid GetGuid(string p_sValue, Guid p_gDefaultValue)
        {
            Guid gValue = p_gDefaultValue;
            if (!String.IsNullOrEmpty(p_sValue))
            {
                try
                {
                    gValue = new Guid(p_sValue);
                }
                catch (Exception)
                { }
            }

            return gValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="double"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="double"/>.</param>
        /// <param name="p_sXPath">Путь до значения.</param>
        /// <returns>Полученный <see cref="double"/> если удалось получить, в обратном случае 0.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static double GetDouble(XmlNode p_xndNode, string p_sXPath)
        {
            return XmlUtils.GetDouble(p_xndNode, p_sXPath, 0);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="double"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="double"/>.</param>
        /// <param name="p_sXPath">Путь до значения.</param>
        /// <param name="p_dDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="double"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static double GetDouble(XmlNode p_xndNode, string p_sXPath, double p_dDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_dDefaultValue;
            }

            try
            {
                return XmlUtils.GetDouble(p_xndNode.SelectSingleNode(p_sXPath), p_dDefaultValue);
            }
            catch (Exception)
            {
                return p_dDefaultValue;
            }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="double"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="double"/>.</param>
        /// <returns>Полученный <see cref="double"/> если удалось получить, в обратном случае 0.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static double GetDouble(XmlNode p_xndNode)
        {
            return XmlUtils.GetDouble(p_xndNode, 0);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="double"/>.</param>
        /// <param name="p_dDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="double"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static double GetDouble(XmlNode p_xndNode, double p_dDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_dDefaultValue;
            }

            if (p_xndNode is XmlAttribute)
            {
                return XmlUtils.GetDouble((XmlAttribute)p_xndNode, p_dDefaultValue);
            }
            else if (p_xndNode is XmlElement)
            {
                return XmlUtils.GetDouble((XmlElement)p_xndNode, p_dDefaultValue);
            }

            return p_dDefaultValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="double"/> из <see cref="XmlAttribute"/>.
        /// </summary>
        /// <param name="p_xatAttribute"><see cref="XmlAttribute"/> с <see cref="double"/>.</param>
        /// <returns>Полученный <see cref="double"/> если удалось получить, в обратном случае 0.</returns>
        public static double GetDouble(XmlAttribute p_xatAttribute)
        {
            return XmlUtils.GetDouble(p_xatAttribute, 0);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="double"/> из <see cref="XmlAttribute"/>.
        /// </summary>
        /// <param name="p_xatAttribute"><see cref="XmlAttribute"/> с <see cref="double"/>.</param>
        /// <param name="p_dDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="double"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        public static double GetDouble(XmlAttribute p_xatAttribute, double p_dDefaultValue)
        {
            if (p_xatAttribute == null ||
                String.IsNullOrEmpty(p_xatAttribute.Value))
            {
                return p_dDefaultValue;
            }

            return XmlUtils.GetDouble(p_xatAttribute.Value, p_dDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="double"/> из <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement"><see cref="XmlElement"/> с <see cref="double"/>.</param>
        /// <returns>Полученный <see cref="double"/> если удалось получить, в обратном случае 0.</returns>
        public static double GetDouble(XmlElement p_xelElement)
        {
            return XmlUtils.GetDouble(p_xelElement, 0);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="double"/> из <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement"><see cref="XmlElement"/> с <see cref="double"/>.</param>
        /// <param name="p_dDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="double"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        public static double GetDouble(XmlElement p_xelElement, double p_dDefaultValue)
        {
            if (p_xelElement == null ||
                String.IsNullOrEmpty(p_xelElement.InnerText))
            {
                return p_dDefaultValue;
            }

            return XmlUtils.GetDouble(p_xelElement.InnerText, p_dDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="double"/> из строки.
        /// </summary>
        /// <param name="p_sValue">Строка с <see cref="double"/>.</param>
        /// <returns>Полученный <see cref="double"/> если удалось получить, в обратном случае 0.</returns>
        public static double GetDouble(string p_sValue)
        {
            return XmlUtils.GetDouble(p_sValue, 0);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="double"/> из строки.
        /// </summary>
        /// <param name="p_sValue">Строка с <see cref="double"/>.</param>
        /// <param name="p_dDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="double"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        public static double GetDouble(string p_sValue, double p_dDefaultValue)
        {
            double dValue = p_dDefaultValue;
            if (!String.IsNullOrEmpty(p_sValue))
            {
                try
                {
                    dValue = double.Parse(p_sValue);
                }
                catch (Exception)
                {
                    try
                    {
                        dValue = double.Parse(p_sValue, CultureInfo.InvariantCulture.NumberFormat);
                    }
                    catch (Exception)
                    { }
                }
            }

            return dValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="DateTime"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="DateTime"/>.</param>
        /// <param name="p_sXPath">Путь до значения.</param>
        /// <returns>Полученный <see cref="DateTime"/> если удалось получить, в обратном случае <see cref="DateTime.MinValue"/>.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static DateTime GetDateTime(XmlNode p_xndNode, string p_sXPath)
        {
            return XmlUtils.GetDateTime(p_xndNode, p_sXPath, DateTime.MinValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="DateTime"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="DateTime"/>.</param>
        /// <param name="p_sXPath">Путь до значения.</param>
        /// <param name="p_dtDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="DateTime"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static DateTime GetDateTime(XmlNode p_xndNode, string p_sXPath, DateTime p_dtDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_dtDefaultValue;
            }

            try
            {
                return XmlUtils.GetDateTime(p_xndNode.SelectSingleNode(p_sXPath), p_dtDefaultValue);
            }
            catch (Exception)
            {
                return p_dtDefaultValue;
            }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="DateTime"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="DateTime"/>.</param>
        /// <returns>Полученный <see cref="DateTime"/> если удалось получить, в обратном случае <see cref="DateTime.MinValue"/>.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static DateTime GetDateTime(XmlNode p_xndNode)
        {
            return XmlUtils.GetDateTime(p_xndNode, DateTime.MinValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="DateTime"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="DateTime"/>.</param>
        /// <param name="p_dtDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="DateTime"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static DateTime GetDateTime(XmlNode p_xndNode, DateTime p_dtDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_dtDefaultValue;
            }

            if (p_xndNode is XmlAttribute)
            {
                return XmlUtils.GetDateTime((XmlAttribute)p_xndNode, p_dtDefaultValue);
            }
            else if (p_xndNode is XmlElement)
            {
                return XmlUtils.GetDateTime((XmlElement)p_xndNode, p_dtDefaultValue);
            }

            return p_dtDefaultValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="DateTime"/> из <see cref="XmlAttribute"/>.
        /// </summary>
        /// <param name="p_xatAttribute"><see cref="XmlAttribute"/> с <see cref="DateTime"/>.</param>
        /// <returns>Полученный <see cref="DateTime"/> если удалось получить, в обратном случае <see cref="DateTime.MinValue"/>.</returns>
        public static DateTime GetDateTime(XmlAttribute p_xatAttribute)
        {
            return XmlUtils.GetDateTime(p_xatAttribute, DateTime.MinValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="DateTime"/> из <see cref="XmlAttribute"/>.
        /// </summary>
        /// <param name="p_xatAttribute"><see cref="XmlAttribute"/> с <see cref="DateTime"/></param>
        /// <param name="p_dtDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="DateTime"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        public static DateTime GetDateTime(XmlAttribute p_xatAttribute, DateTime p_dtDefaultValue)
        {
            if (p_xatAttribute == null ||
                String.IsNullOrEmpty(p_xatAttribute.Value))
            {
                return p_dtDefaultValue;
            }

            return XmlUtils.GetDateTime(p_xatAttribute.Value, p_dtDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="DateTime"/> из <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement"><see cref="XmlElement"/> с <see cref="DateTime"/></param>
        /// <returns>Полученный <see cref="DateTime"/> если удалось получить, в обратном случае <see cref="DateTime.MinValue"/>.</returns>
        public static DateTime GetDateTime(XmlElement p_xelElement)
        {
            return XmlUtils.GetDateTime(p_xelElement, DateTime.MinValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="DateTime"/> из <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement"><see cref="XmlElement"/> с <see cref="DateTime"/></param>
        /// <param name="p_dtDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="DateTime"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        public static DateTime GetDateTime(XmlElement p_xelElement, DateTime p_dtDefaultValue)
        {
            if (p_xelElement == null ||
                String.IsNullOrEmpty(p_xelElement.InnerText))
            {
                return p_dtDefaultValue;
            }

            return XmlUtils.GetDateTime(p_xelElement.InnerText, p_dtDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="DateTime"/> из строки.
        /// </summary>
        /// <param name="p_sValue">Строка с <see cref="DateTime"/></param>
        /// <returns>Полученный <see cref="DateTime"/> если удалось получить, в обратном случае <see cref="DateTime.MinValue"/>.</returns>
        public static DateTime GetDateTime(string p_sValue)
        {
            return XmlUtils.GetDateTime(p_sValue, DateTime.MinValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="DateTime"/> из строки.
        /// </summary>
        /// <param name="p_sValue">Строка с <see cref="DateTime"/></param>
        /// <param name="p_dtDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="DateTime"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        public static DateTime GetDateTime(string p_sValue, DateTime p_dtDefaultValue)
        {
            DateTime dtValue = p_dtDefaultValue;
            if (!String.IsNullOrEmpty(p_sValue))
            {
                try
                {
                    dtValue = DateTime.Parse(p_sValue, CultureInfo.InvariantCulture.DateTimeFormat);
                }
                catch (Exception)
                { }
            }

            return dtValue;
        }

        public static DateTime GetDateTimeFromXml(string p_sValue, DateTime p_dtDefaultValue)
        {
            DateTime dtValue = p_dtDefaultValue;
            if (!String.IsNullOrEmpty(p_sValue))
            {
                try
                {
                    string[] asNames = { "Year", "Month", "Day", "Hour", "Minute", "Second" };
                    int[] aiParts = { 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < aiParts.Length; i++)
                    {
                        int iStart = p_sValue.IndexOf(asNames[i]);
                        iStart = p_sValue.IndexOf('"', iStart + 1);
                        int iFinish = p_sValue.IndexOf('"', iStart + 1);
                        string sParam = p_sValue.Substring(iStart + 1, iFinish - iStart - 1);
                        try
                        {
                            aiParts[i] = Int32.Parse(sParam);
                        }
                        catch { }
                    }
                    dtValue = new DateTime(aiParts[0], aiParts[1], aiParts[2], aiParts[3], aiParts[4], aiParts[5]);
                }
                catch (Exception)
                { }
            }

            return dtValue;
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="TimeSpan"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="TimeSpan"/>.</param>
        /// <param name="p_sXPath">Путь до значения.</param>
        /// <returns>Полученный <see cref="TimeSpan"/> если удалось получить, в обратном случае <see cref="TimeSpan.MinValue"/>.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static TimeSpan GetTimeSpan(XmlNode p_xndNode, string p_sXPath)
        {
            return XmlUtils.GetTimeSpan(p_xndNode, p_sXPath, TimeSpan.MinValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="TimeSpan"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="TimeSpan"/>.</param>
        /// <param name="p_sXPath">Путь до значения.</param>
        /// <param name="p_tsDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="TimeSpan"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static TimeSpan GetTimeSpan(XmlNode p_xndNode, string p_sXPath, TimeSpan p_tsDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_tsDefaultValue;
            }

            try
            {
                return XmlUtils.GetTimeSpan(p_xndNode.SelectSingleNode(p_sXPath), p_tsDefaultValue);
            }
            catch (Exception)
            {
                return p_tsDefaultValue;
            }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="TimeSpan"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="TimeSpan"/>.</param>
        /// <returns>Полученный <see cref="TimeSpan"/> если удалось получить, в обратном случае <see cref="TimeSpan.MinValue"/>.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static TimeSpan GetTimeSpan(XmlNode p_xndNode)
        {
            return XmlUtils.GetTimeSpan(p_xndNode, TimeSpan.MinValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="TimeSpan"/> из <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="p_xndNode"><see cref="XmlNode"/> с <see cref="TimeSpan"/>.</param>
        /// <param name="p_tsDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="TimeSpan"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то значение берётся из <see cref="XmlAttribute.Value"/>, 
        /// в обратном случае из <see cref="XmlElement.InnerText"/>.</remarks>
        public static TimeSpan GetTimeSpan(XmlNode p_xndNode, TimeSpan p_tsDefaultValue)
        {
            if (p_xndNode == null)
            {
                return p_tsDefaultValue;
            }

            if (p_xndNode is XmlAttribute)
            {
                return XmlUtils.GetTimeSpan((XmlAttribute)p_xndNode, p_tsDefaultValue);
            }
            else if (p_xndNode is XmlElement)
            {
                return XmlUtils.GetTimeSpan((XmlElement)p_xndNode, p_tsDefaultValue);
            }

            return p_tsDefaultValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="TimeSpan"/> из <see cref="XmlAttribute"/>.
        /// </summary>
        /// <param name="p_xatAttribute"><see cref="XmlAttribute"/> с <see cref="TimeSpan"/>.</param>
        /// <returns>Полученный <see cref="TimeSpan"/> если удалось получить, в обратном случае <see cref="TimeSpan.MinValue"/>.</returns>
        public static TimeSpan GetTimeSpan(XmlAttribute p_xatAttribute)
        {
            return XmlUtils.GetTimeSpan(p_xatAttribute, TimeSpan.MinValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="TimeSpan"/> из <see cref="XmlAttribute"/>.
        /// </summary>
        /// <param name="p_xatAttribute"><see cref="XmlAttribute"/> с <see cref="TimeSpan"/>.</param>
        /// <param name="p_tsDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="TimeSpan"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        public static TimeSpan GetTimeSpan(XmlAttribute p_xatAttribute, TimeSpan p_tsDefaultValue)
        {
            if (p_xatAttribute == null ||
                String.IsNullOrEmpty(p_xatAttribute.Value))
            {
                return p_tsDefaultValue;
            }

            return XmlUtils.GetTimeSpan(p_xatAttribute.Value, p_tsDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="TimeSpan"/> из <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement"><see cref="XmlElement"/> с <see cref="TimeSpan"/>.</param>
        /// <returns>Полученный <see cref="TimeSpan"/> если удалось получить, в обратном случае <see cref="TimeSpan.MinValue"/>.</returns>
        public static TimeSpan GetTimeSpan(XmlElement p_xelElement)
        {
            return XmlUtils.GetTimeSpan(p_xelElement, TimeSpan.MinValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="TimeSpan"/> из <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement"><see cref="XmlElement"/> с <see cref="TimeSpan"/>.</param>
        /// <param name="p_tsDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="TimeSpan"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        public static TimeSpan GetTimeSpan(XmlElement p_xelElement, TimeSpan p_tsDefaultValue)
        {
            if (p_xelElement == null ||
                String.IsNullOrEmpty(p_xelElement.InnerText))
            {
                return p_tsDefaultValue;
            }

            return XmlUtils.GetTimeSpan(p_xelElement.InnerText, p_tsDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="TimeSpan"/> из строки.
        /// </summary>
        /// <param name="p_sValue">Строка с <see cref="TimeSpan"/>.</param>
        /// <returns>Полученный <see cref="TimeSpan"/> если удалось получить, в обратном случае <see cref="TimeSpan.MinValue"/>.</returns>
        public static TimeSpan GetTimeSpan(string p_sValue)
        {
            return XmlUtils.GetTimeSpan(p_sValue, TimeSpan.MinValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Возвращает <see cref="TimeSpan"/> из строки.
        /// </summary>
        /// <param name="p_sValue">Строка с <see cref="TimeSpan"/>.</param>
        /// <param name="p_tsDefaultValue">Значение по умолчанию.</param>
        /// <returns>Полученный <see cref="TimeSpan"/> если удалось получить, в обратном случае значение по умолчанию.</returns>
        public static TimeSpan GetTimeSpan(string p_sValue, TimeSpan p_tsDefaultValue)
        {
            TimeSpan tsValue = p_tsDefaultValue;
            if (!String.IsNullOrEmpty(p_sValue))
            {
                try
                {
                    tsValue = TimeSpan.Parse(p_sValue);
                }
                catch (Exception)
                { }
            }

            return tsValue;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертирует значение в строку.
        /// </summary>
        /// <param name="p_iValue">Значение.</param>
        /// <returns>Строку со значением.</returns>
        public static string ValueToString(int p_iValue)
        {
            return p_iValue.ToString();
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертирует значение в строку.
        /// </summary>
        /// <param name="p_bValue">Значение.</param>
        /// <returns>Строку со значением.</returns>
        public static string ValueToString(bool p_bValue)
        {
            return p_bValue.ToString();
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертирует значение в строку.
        /// </summary>
        /// <param name="p_gValue">Значение.</param>
        /// <returns>Строку со значением.</returns>
        public static string ValueToString(Guid p_gValue)
        {
            return p_gValue.ToString();
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертирует значение в строку.
        /// </summary>
        /// <param name="p_dValue">Значение.</param>
        /// <returns>Строку со значением.</returns>
        public static string ValueToString(double p_dValue)
        {
            return p_dValue.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертирует значение в строку.
        /// </summary>
        /// <param name="p_dtValue">Значение.</param>
        /// <returns>Строку со значением.</returns>
        public static string ValueToString(DateTime p_dtValue)
        {
            return p_dtValue.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конвертирует значение в строку.
        /// </summary>
        /// <param name="p_tsValue">Значение.</param>
        /// <returns>Строку со значением.</returns>
        public static string ValueToString(TimeSpan p_tsValue)
        {
            return p_tsValue.ToString();
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Установить значение <see cref="XmlAttribute"/> в <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement">Элемент.</param>
        /// <param name="p_sAttributeName">Имя атрибута.</param>
        /// <param name="p_iAttributeValue">Значение.</param>
        /// <returns>Установленный <see cref="XmlAttribute"/>.</returns>
        /// <remarks>если атрибута нет он создаётся.</remarks>
        public static XmlAttribute SetAttribute(XmlElement p_xelElement, string p_sAttributeName, int p_iAttributeValue)
        {
            return XmlUtils.SetAttribute(p_xelElement, p_sAttributeName, XmlUtils.ValueToString(p_iAttributeValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Установить значение <see cref="XmlAttribute"/> в <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement">Элемент.</param>
        /// <param name="p_sAttributeName">Имя атрибута.</param>
        /// <param name="p_bAttributeValue">Значение.</param>
        /// <returns>Установленный <see cref="XmlAttribute"/>.</returns>
        /// <remarks>если атрибута нет он создаётся.</remarks>
        public static XmlAttribute SetAttribute(XmlElement p_xelElement, string p_sAttributeName, bool p_bAttributeValue)
        {
            return XmlUtils.SetAttribute(p_xelElement, p_sAttributeName, XmlUtils.ValueToString(p_bAttributeValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Установить значение <see cref="XmlAttribute"/> в <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement">Элемент.</param>
        /// <param name="p_sAttributeName">Имя атрибута.</param>
        /// <param name="p_gAttributeValue">Значение.</param>
        /// <returns>Установленный <see cref="XmlAttribute"/>.</returns>
        /// <remarks>если атрибута нет он создаётся.</remarks>
        public static XmlAttribute SetAttribute(XmlElement p_xelElement, string p_sAttributeName, Guid p_gAttributeValue)
        {
            return XmlUtils.SetAttribute(p_xelElement, p_sAttributeName, XmlUtils.ValueToString(p_gAttributeValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Установить значение <see cref="XmlAttribute"/> в <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement">Элемент.</param>
        /// <param name="p_sAttributeName">Имя атрибута.</param>
        /// <param name="p_dAttributeValue">Значение.</param>
        /// <returns>Установленный <see cref="XmlAttribute"/>.</returns>
        /// <remarks>если атрибута нет он создаётся.</remarks>
        public static XmlAttribute SetAttribute(XmlElement p_xelElement, string p_sAttributeName, double p_dAttributeValue)
        {
            return XmlUtils.SetAttribute(p_xelElement, p_sAttributeName, XmlUtils.ValueToString(p_dAttributeValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Установить значение <see cref="XmlAttribute"/> в <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement">Элемент.</param>
        /// <param name="p_sAttributeName">Имя атрибута.</param>
        /// <param name="p_dtAttributeValue">Значение.</param>
        /// <returns>Установленный <see cref="XmlAttribute"/>.</returns>
        /// <remarks>если атрибута нет он создаётся.</remarks>
        public static XmlAttribute SetAttribute(XmlElement p_xelElement, string p_sAttributeName, DateTime p_dtAttributeValue)
        {
            return XmlUtils.SetAttribute(p_xelElement, p_sAttributeName, XmlUtils.ValueToString(p_dtAttributeValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Установить значение <see cref="XmlAttribute"/> в <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement">Элемент.</param>
        /// <param name="p_sAttributeName">Имя атрибута.</param>
        /// <param name="p_tsAttributeValue">Значение.</param>
        /// <returns>Установленный <see cref="XmlAttribute"/>.</returns>
        /// <remarks>если атрибута нет он создаётся.</remarks>
        public static XmlAttribute SetAttribute(XmlElement p_xelElement, string p_sAttributeName, TimeSpan p_tsAttributeValue)
        {
            return XmlUtils.SetAttribute(p_xelElement, p_sAttributeName, XmlUtils.ValueToString(p_tsAttributeValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Установить значение <see cref="XmlAttribute"/> в <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelElement">Элемент.</param>
        /// <param name="p_sAttributeName">Имя атрибута.</param>
        /// <param name="p_sAttributeValue">Значение.</param>
        /// <returns>Установленный <see cref="XmlAttribute"/>.</returns>
        /// <remarks>если атрибута нет он создаётся.</remarks>
        public static XmlAttribute SetAttribute(XmlElement p_xelElement, string p_sAttributeName, string p_sAttributeValue)
        {
            XmlAttribute xatAttribute = p_xelElement.Attributes[p_sAttributeName];
            if (xatAttribute == null)
            {
                xatAttribute = p_xelElement.OwnerDocument.CreateAttribute(p_sAttributeName);
                p_xelElement.Attributes.Append(xatAttribute);
            }
            xatAttribute.Value = p_sAttributeValue;

            return xatAttribute;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить <see cref="XmlElement"/> в родительский элемент.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_iElementValue">Значение.</param>
        /// <returns>Добавленный элемент с установленным значением.</returns>
        public static XmlElement AddElement(XmlElement p_xelParent, string p_sElementName, int p_iElementValue)
        {
            return XmlUtils.AddElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_iElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Устанавливает значение <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_iElementValue">Значение.</param>
        /// <returns>Элемент с установленным значением.</returns>
        /// <remarks>Если элемента нет, то он создаётся.</remarks>
        public static XmlElement SetElement(XmlElement p_xelParent, string p_sElementName, int p_iElementValue)
        {
            return XmlUtils.SetElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_iElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить <see cref="XmlElement"/> в родительский элемент.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_bElementValue">Значение.</param>
        /// <returns>Добавленный элемент с установленным значением.</returns>
        public static XmlElement AddElement(XmlElement p_xelParent, string p_sElementName, bool p_bElementValue)
        {
            return XmlUtils.AddElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_bElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Устанавливает значение <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_bElementValue">Значение.</param>
        /// <returns>Элемент с установленным значением.</returns>
        /// <remarks>Если элемента нет, то он создаётся.</remarks>
        public static XmlElement SetElement(XmlElement p_xelParent, string p_sElementName, bool p_bElementValue)
        {
            return XmlUtils.SetElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_bElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить <see cref="XmlElement"/> в родительский элемент.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_gElementValue">Значение.</param>
        /// <returns>Добавленный элемент с установленным значением.</returns>
        public static XmlElement AddElement(XmlElement p_xelParent, string p_sElementName, Guid p_gElementValue)
        {
            return XmlUtils.AddElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_gElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Устанавливает значение <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_gElementValue">Значение.</param>
        /// <returns>Элемент с установленным значением.</returns>
        /// <remarks>Если элемента нет, то он создаётся.</remarks>
        public static XmlElement SetElement(XmlElement p_xelParent, string p_sElementName, Guid p_gElementValue)
        {
            return XmlUtils.SetElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_gElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить <see cref="XmlElement"/> в родительский элемент.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_dElementValue">Значение.</param>
        /// <returns>Добавленный элемент с установленным значением.</returns>
        public static XmlElement AddElement(XmlElement p_xelParent, string p_sElementName, double p_dElementValue)
        {
            return XmlUtils.AddElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_dElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Устанавливает значение <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_dElementValue">Значение.</param>
        /// <returns>Элемент с установленным значением.</returns>
        /// <remarks>Если элемента нет, то он создаётся.</remarks>
        public static XmlElement SetElement(XmlElement p_xelParent, string p_sElementName, double p_dElementValue)
        {
            return XmlUtils.SetElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_dElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить <see cref="XmlElement"/> в родительский элемент.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_dtElementValue">Значение.</param>
        /// <returns>Добавленный элемент с установленным значением.</returns>
        public static XmlElement AddElement(XmlElement p_xelParent, string p_sElementName, DateTime p_dtElementValue)
        {
            return XmlUtils.AddElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_dtElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Устанавливает значение <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_dtElementValue">Значение.</param>
        /// <returns>Элемент с установленным значением.</returns>
        /// <remarks>Если элемента нет, то он создаётся.</remarks>
        public static XmlElement SetElement(XmlElement p_xelParent, string p_sElementName, DateTime p_dtElementValue)
        {
            return XmlUtils.SetElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_dtElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить <see cref="XmlElement"/> в родительский элемент.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_tsElementValue">Значение.</param>
        /// <returns>Добавленный элемент с установленным значением.</returns>
        public static XmlElement AddElement(XmlElement p_xelParent, string p_sElementName, TimeSpan p_tsElementValue)
        {
            return XmlUtils.AddElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_tsElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Устанавливает значение <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_tsElementValue">Значение.</param>
        /// <returns>Элемент с установленным значением.</returns>
        /// <remarks>Если элемента нет, то он создаётся.</remarks>
        public static XmlElement SetElement(XmlElement p_xelParent, string p_sElementName, TimeSpan p_tsElementValue)
        {
            return XmlUtils.SetElement(p_xelParent, p_sElementName, XmlUtils.ValueToString(p_tsElementValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Устанавливает значение <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_sElementValue">Значение.</param>
        /// <returns>Элемент с установленным значением.</returns>
        /// <remarks>Если элемента нет, то он создаётся.</remarks>
        public static XmlElement SetElement(XmlElement p_xelParent, string p_sElementName, string p_sElementValue)
        {
            XmlElement xelElement = p_xelParent[p_sElementName];
            if (xelElement == null)
            {
                xelElement = p_xelParent.OwnerDocument.CreateElement(p_sElementName);
                p_xelParent.AppendChild(xelElement);
            }

            if (!String.IsNullOrEmpty(p_sElementValue))
            {
                xelElement.InnerText = p_sElementValue;
            }
            else
            {
                xelElement.InnerText = "";
            }

            return xelElement;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить <see cref="XmlElement"/> в родительский элемент.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <returns>Добавленый элемент.</returns>
        public static XmlElement AddElement(XmlElement p_xelParent, string p_sElementName)
        {
            return XmlUtils.AddElement(p_xelParent, p_sElementName, null);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить <see cref="XmlElement"/> в родительский элемент.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sElementName">Имя элемента.</param>
        /// <param name="p_sElementValue">Значение.</param>
        /// <returns>Добавленный элемент с установленным значением.</returns>
        public static XmlElement AddElement(XmlElement p_xelParent, string p_sElementName, string p_sElementValue)
        {
            XmlElement xelNewElement = p_xelParent.OwnerDocument.CreateElement(p_sElementName);
            if (!String.IsNullOrEmpty(p_sElementValue))
            {
                xelNewElement.InnerText = p_sElementValue;
            }

            p_xelParent.AppendChild(xelNewElement);

            return xelNewElement;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавляет корневой элемент в <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="p_xdmDocument">Документ.</param>
        /// <param name="p_sRootName">Имя корневого элемента.</param>
        /// <returns>Корневой элемент.</returns>
        /// <remarks>Если корневой элемент уже есть, то ничего не создаётся и возвращается именно существующий корневой элемент.</remarks>
        public static XmlElement SetRoot(XmlDocument p_xdmDocument, string p_sRootName)
        {
            XmlElement xelRootElement = p_xdmDocument.DocumentElement;
            if (xelRootElement == null)
            {
                xelRootElement = p_xdmDocument.CreateElement(p_sRootName);
                p_xdmDocument.AppendChild(xelRootElement);
            }
            return xelRootElement;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Создать документ.
        /// </summary>
        /// <param name="p_sRootName">Имя корневого элемента.</param>
        /// <returns>Документ.</returns>
        public static XmlDocument CreateDoc(string p_sRootName)
        {
            XmlDocument xdmNewDoc = new XmlDocument();
            XmlUtils.SetRoot(xdmNewDoc, p_sRootName);
            return xdmNewDoc;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlAttribute"/>. Если <see cref="XmlAttribute"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя атрибута.</param>
        /// <param name="p_sDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static string GetCreateAttr(XmlElement p_xelParent, string p_sName, string p_sDefaultValue)
        {
            XmlAttribute xatAttribute = p_xelParent.Attributes[p_sName];
            if (xatAttribute == null)
            {
                XmlUtils.SetAttribute(p_xelParent, p_sName, p_sDefaultValue);
                return p_sDefaultValue;
            }
            return XmlUtils.GetString(xatAttribute, p_sDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlElement"/>. Если <see cref="XmlElement"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя элемента.</param>
        /// <param name="p_sDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static string GetCreateElem(XmlElement p_xelParent, string p_sName, string p_sDefaultValue)
        {
            XmlElement xelElement = p_xelParent[p_sName];
            if (xelElement == null)
            {
                XmlUtils.SetElement(p_xelParent, p_sName, p_sDefaultValue);
                return p_sDefaultValue;
            }
            return XmlUtils.GetString(xelElement, p_sDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlAttribute"/>. Если <see cref="XmlAttribute"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя атрибута.</param>
        /// <param name="p_iDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static int GetCreateAttr(XmlElement p_xelParent, string p_sName, int p_iDefaultValue)
        {
            XmlAttribute xatAttribute = p_xelParent.Attributes[p_sName];
            if (xatAttribute == null)
            {
                XmlUtils.SetAttribute(p_xelParent, p_sName, p_iDefaultValue);
                return p_iDefaultValue;
            }
            return XmlUtils.GetInt(xatAttribute, p_iDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlElement"/>. Если <see cref="XmlElement"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя элемента.</param>
        /// <param name="p_iDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static int GetCreateElem(XmlElement p_xelParent, string p_sName, int p_iDefaultValue)
        {
            XmlElement xelElement = p_xelParent[p_sName];
            if (xelElement == null)
            {
                XmlUtils.SetElement(p_xelParent, p_sName, p_iDefaultValue);
                return p_iDefaultValue;
            }
            return XmlUtils.GetInt(xelElement, p_iDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlAttribute"/>. Если <see cref="XmlAttribute"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя атрибута.</param>
        /// <param name="p_gDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static Guid GetCreateAttr(XmlElement p_xelParent, string p_sName, Guid p_gDefaultValue)
        {
            XmlAttribute xatAttribute = p_xelParent.Attributes[p_sName];
            if (xatAttribute == null)
            {
                XmlUtils.SetAttribute(p_xelParent, p_sName, p_gDefaultValue);
                return p_gDefaultValue;
            }
            return XmlUtils.GetGuid(xatAttribute, p_gDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlElement"/>. Если <see cref="XmlElement"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя элемента.</param>
        /// <param name="p_gDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static Guid GetCreateElem(XmlElement p_xelParent, string p_sName, Guid p_gDefaultValue)
        {
            XmlElement xelElement = p_xelParent[p_sName];
            if (xelElement == null)
            {
                XmlUtils.SetElement(p_xelParent, p_sName, p_gDefaultValue);
                return p_gDefaultValue;
            }
            return XmlUtils.GetGuid(xelElement, p_gDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlAttribute"/>. Если <see cref="XmlAttribute"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя атрибута.</param>
        /// <param name="p_bDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static bool GetCreateAttr(XmlElement p_xelParent, string p_sName, bool p_bDefaultValue)
        {
            XmlAttribute xatAttribute = p_xelParent.Attributes[p_sName];
            if (xatAttribute == null)
            {
                XmlUtils.SetAttribute(p_xelParent, p_sName, p_bDefaultValue);
                return p_bDefaultValue;
            }
            return XmlUtils.GetBool(xatAttribute, p_bDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlElement"/>. Если <see cref="XmlElement"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя элемента.</param>
        /// <param name="p_bDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static bool GetCreateElem(XmlElement p_xelParent, string p_sName, bool p_bDefaultValue)
        {
            XmlElement xelElement = p_xelParent[p_sName];
            if (xelElement == null)
            {
                XmlUtils.SetElement(p_xelParent, p_sName, p_bDefaultValue);
                return p_bDefaultValue;
            }
            return XmlUtils.GetBool(xelElement, p_bDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlAttribute"/>. Если <see cref="XmlAttribute"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя атрибута.</param>
        /// <param name="p_dDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static double GetCreateAttr(XmlElement p_xelParent, string p_sName, double p_dDefaultValue)
        {
            XmlAttribute xatAttribute = p_xelParent.Attributes[p_sName];
            if (xatAttribute == null)
            {
                XmlUtils.SetAttribute(p_xelParent, p_sName, p_dDefaultValue);
                return p_dDefaultValue;
            }
            return XmlUtils.GetDouble(xatAttribute, p_dDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlElement"/>. Если <see cref="XmlElement"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя элемента.</param>
        /// <param name="p_dDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static double GetCreateElem(XmlElement p_xelParent, string p_sName, double p_dDefaultValue)
        {
            XmlElement xelElement = p_xelParent[p_sName];
            if (xelElement == null)
            {
                XmlUtils.SetElement(p_xelParent, p_sName, p_dDefaultValue);
                return p_dDefaultValue;
            }
            return XmlUtils.GetDouble(xelElement, p_dDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlAttribute"/>. Если <see cref="XmlAttribute"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя атрибута.</param>
        /// <param name="p_dtDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static DateTime GetCreateAttr(XmlElement p_xelParent, string p_sName, DateTime p_dtDefaultValue)
        {
            XmlAttribute xatAttribute = p_xelParent.Attributes[p_sName];
            if (xatAttribute == null)
            {
                XmlUtils.SetAttribute(p_xelParent, p_sName, p_dtDefaultValue);
                return p_dtDefaultValue;
            }
            return XmlUtils.GetDateTime(xatAttribute, p_dtDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlElement"/>. Если <see cref="XmlElement"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя элемента.</param>
        /// <param name="p_dtDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static DateTime GetCreateElem(XmlElement p_xelParent, string p_sName, DateTime p_dtDefaultValue)
        {
            XmlElement xelElement = p_xelParent[p_sName];
            if (xelElement == null)
            {
                XmlUtils.SetElement(p_xelParent, p_sName, p_dtDefaultValue);
                return p_dtDefaultValue;
            }
            return XmlUtils.GetDateTime(xelElement, p_dtDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlAttribute"/>. Если <see cref="XmlAttribute"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя атрибута.</param>
        /// <param name="p_tsDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static TimeSpan GetCreateAttr(XmlElement p_xelParent, string p_sName, TimeSpan p_tsDefaultValue)
        {
            XmlAttribute xatAttribute = p_xelParent.Attributes[p_sName];
            if (xatAttribute == null)
            {
                XmlUtils.SetAttribute(p_xelParent, p_sName, p_tsDefaultValue);
                return p_tsDefaultValue;
            }
            return XmlUtils.GetTimeSpan(xatAttribute, p_tsDefaultValue);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить значение из <see cref="XmlElement"/>. Если <see cref="XmlElement"/> не существует, то он создаётся.
        /// </summary>
        /// <param name="p_xelParent">Родительский <see cref="XmlElement"/>.</param>
        /// <param name="p_sName">Имя элемента.</param>
        /// <param name="p_tsDefaultValue">Значение по-умолчанию.</param>
        /// <returns>Полученное значение.</returns>
        public static TimeSpan GetCreateElem(XmlElement p_xelParent, string p_sName, TimeSpan p_tsDefaultValue)
        {
            XmlElement xelElement = p_xelParent[p_sName];

            if (xelElement == null)
            {
                XmlUtils.SetElement(p_xelParent, p_sName, p_tsDefaultValue);
                return p_tsDefaultValue;
            }

            return XmlUtils.GetTimeSpan(xelElement, p_tsDefaultValue);
        }




        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить <see cref="XmlNode"/> из <see cref="XmlElement"/>. Если нода не существует, то она создаётся.
        /// </summary>
        /// <param name="p_xelParent"></param>
        /// <param name="p_sPath"></param>
        /// <returns></returns>
        public static XmlNode GetCreateNode(XmlElement p_xelParent, string p_sPath)
        {
            XmlElement xelParent = p_xelParent;

            string[] asParts = p_sPath.Split('/');
            for (int i = 0; i < asParts.Length; i++)
            {
                if (asParts[i] == "")
                {
                    throw new ArgumentException("Bad path: empty part");
                }
                if (asParts[i].StartsWith("@"))
                {
                    if (i != asParts.Length - 1)
                    {
                        throw new ArgumentException("Bad path: attribute not at the end");
                    }
                    return XmlUtils.SetAttribute(xelParent, asParts[i].Substring(1), "");
                }

                XmlElement xelElement = xelParent[asParts[i]];
                if (xelElement == null)
                {
                    xelElement = xelParent.OwnerDocument.CreateElement(asParts[i]);
                    xelParent.AppendChild(xelElement);
                }
                xelParent = xelElement;
            }

            return xelParent;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_xelNode">Нода.</param>
        /// <param name="p_iValue">Значение.</param>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то для установки значения используется <see cref="XmlAttribute.Value"/>, 
        /// во всех остальных случаях <see cref="XmlElement.InnerText"/></remarks>
        public static void SetNodeValue(XmlNode p_xelNode, int p_iValue)
        {
            XmlUtils.SetNodeValue(p_xelNode, XmlUtils.ValueToString(p_iValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_xelNode">Нода.</param>
        /// <param name="p_bValue">Значение.</param>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то для установки значения используется <see cref="XmlAttribute.Value"/>, 
        /// во всех остальных случаях <see cref="XmlElement.InnerText"/></remarks>
        public static void SetNodeValue(XmlNode p_xelNode, bool p_bValue)
        {
            XmlUtils.SetNodeValue(p_xelNode, XmlUtils.ValueToString(p_bValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_xelNode">Нода.</param>
        /// <param name="p_gValue">Значение.</param>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то для установки значения используется <see cref="XmlAttribute.Value"/>, 
        /// во всех остальных случаях <see cref="XmlElement.InnerText"/></remarks>
        public static void SetNodeValue(XmlNode p_xelNode, Guid p_gValue)
        {
            XmlUtils.SetNodeValue(p_xelNode, XmlUtils.ValueToString(p_gValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_xelNode">Нода.</param>
        /// <param name="p_dValue">Значение.</param>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то для установки значения используется <see cref="XmlAttribute.Value"/>, 
        /// во всех остальных случаях <see cref="XmlElement.InnerText"/></remarks>
        public static void SetNodeValue(XmlNode p_xelNode, double p_dValue)
        {
            XmlUtils.SetNodeValue(p_xelNode, XmlUtils.ValueToString(p_dValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_xelNode">Нода.</param>
        /// <param name="p_dtValue">Значение.</param>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то для установки значения используется <see cref="XmlAttribute.Value"/>, 
        /// во всех остальных случаях <see cref="XmlElement.InnerText"/></remarks>
        public static void SetNodeValue(XmlNode p_xelNode, DateTime p_dtValue)
        {
            XmlUtils.SetNodeValue(p_xelNode, XmlUtils.ValueToString(p_dtValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_xelNode">Нода.</param>
        /// <param name="p_tsValue">Значение.</param>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то для установки значения используется <see cref="XmlAttribute.Value"/>, 
        /// во всех остальных случаях <see cref="XmlElement.InnerText"/></remarks>
        public static void SetNodeValue(XmlNode p_xelNode, TimeSpan p_tsValue)
        {
            XmlUtils.SetNodeValue(p_xelNode, XmlUtils.ValueToString(p_tsValue));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_xelNode">Нода.</param>
        /// <param name="p_sValue">Значение.</param>
        /// <remarks>Если передан <see cref="XmlAttribute"/> то для установки значения используется <see cref="XmlAttribute.Value"/>, 
        /// во всех остальных случаях <see cref="XmlElement.InnerText"/></remarks>
        public static void SetNodeValue(XmlNode p_xelNode, string p_sValue)
        {
            if (p_xelNode is XmlElement)
            {
                ((XmlElement)p_xelNode).InnerText = p_sValue;
            }
            else if (p_xelNode is XmlAttribute)
            {
                ((XmlAttribute)p_xelNode).Value = p_sValue;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Node isn't an attribute or element");
            }
        }

        /*
		//---------------------------------------------------------------------------------------------------
		/// <summary>
		/// Проверяет Xml на соответсвие Xsd.
		/// </summary>
		/// <param name="p_xelXml">Элемент с Xml для проверки.</param>
		/// <param name="p_sSchema">Схема.</param>
		/// <returns>Созданный элемент с корректным элементом.</returns>
		public static void ValidateXml( XmlElement p_xelXml, string p_sSchema )
		{
			XmlUtils.ValidateXml( XmlUtils.XmlToString( p_xelXml ), p_sSchema );
		}

		
		//---------------------------------------------------------------------------------------------------
		/// <summary>
		/// Проверяет Xml на соответсвие Xsd.
		/// </summary>
		/// <param name="p_sXml">Xml для проверки.</param>
		/// <param name="p_sSchema">Схема.</param>
		/// <returns>Корректный документ.</returns>
		public static XmlDocument ValidateXml( string p_sXml, string p_sSchema )
		{
			XmlSchema objXmlSchema = XmlSchema.Read( new StringReader( p_sSchema ), null );

			XmlValidatingReader objValidatingReader = new XmlValidatingReader( p_sXml, XmlNodeType.Element, null );
			objValidatingReader.Schemas.Add( objXmlSchema );
			objValidatingReader.ValidationType = ValidationType.Schema;
			//objValidatingReader.ValidationEventHandler += new ValidationEventHandler( XmlUtils.ValidationHandler );

			XmlDocument xdmXml = new XmlDocument();
			xdmXml.Load( objValidatingReader );

			return xdmXml;
		}
		*/
        public static string ObjectToXmlSerialization(object objSerializable)
        {
            return ObjectToXmlSerialization(objSerializable, new System.Xml.Serialization.XmlSerializer(objSerializable.GetType()));
        }

        public static string ObjectToXmlSerialization(object objSerializable, bool ReplaceHeader)
        {
            return ObjectToXmlSerialization(objSerializable, new System.Xml.Serialization.XmlSerializer(objSerializable.GetType()), ReplaceHeader);
        }

        /// <summary>
        /// Serializes object into XML string
        /// </summary>
        /// <param name="objSerializable"></param>
        /// <returns></returns>
        public static string ObjectToXmlSerialization(object objSerializable, System.Xml.Serialization.XmlSerializer objSerializer)
        {
            return ObjectToXmlSerialization(objSerializable, objSerializer, true);
        }

        /// <summary>
        /// Serializes object into XML string
        /// </summary>
        /// <param name="objSerializable"></param>
        /// <returns></returns>
        public static string ObjectToXmlSerialization(object objSerializable, System.Xml.Serialization.XmlSerializer objSerializer, bool ReplaceHeader)
        {
            System.Xml.Serialization.XmlSerializerNamespaces objEmptyNamespace = new System.Xml.Serialization.XmlSerializerNamespaces();
            objEmptyNamespace.Add(string.Empty, string.Empty);

            System.IO.MemoryStream objMemoryStream = new System.IO.MemoryStream();
            objSerializer.Serialize(objMemoryStream, objSerializable, objEmptyNamespace);

            objMemoryStream.Seek(0, 0);
            byte[] objBuffer = new Byte[objMemoryStream.Length];
            objMemoryStream.Read(objBuffer, 0, objBuffer.Length);

            string sXml = System.Text.Encoding.UTF8.GetString(objBuffer, 0, objBuffer.Length);
            return ReplaceHeader ? sXml.Replace("<?xml version=\"1.0\"?>", string.Empty) : sXml;
        }

        /// <summary>
        /// Deserializes XML string to object
        /// </summary>
        /// <typeparam name="T">Type of object being deserialized</typeparam>
        /// <param name="Input">Xml string containing object</param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string Input)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return DeserializeObject<T>(Input, serializer);
        }

        /// <summary>
        /// Deserializes XML string to object with custom XmlSerializer
        /// </summary>
        /// <typeparam name="T">Type of object being deserialized</typeparam>
        /// <param name="Input">Xml string containing object</param>
        /// <param name="Serializer">Custom XmlSerializer object</param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string Input, XmlSerializer Serializer)
        {
            MemoryStream memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Input));
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8);
            return (T)Serializer.Deserialize(memoryStream);
        }

        //---------------------------------------------------------------------------------------------------
        /// <summary>
        /// Validation Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void ValidationHandler(object sender, ValidationEventArgs args)
        {
            //string sMessage = String.Format( "Severity: {0}\nMessage:{1}\n\n", args.Severity, args.Message );
            //Console.WriteLine( sMessage );

            throw args.Exception;
        }
    }
}
