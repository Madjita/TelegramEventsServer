using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.XPath;

namespace Logger.Helpers
{
    public static class XmlHelper
    {
        public static string SerializeObject<T>(T toSerialize, bool isUtf8 = false)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = isUtf8 ? new Utf8StringWriter() : new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static string SerializePaytureResponse<T>(T toSerialize)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            using (var textWriter = new StringWriter())
            using (var writer = XmlWriter.Create(textWriter, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                xmlSerializer.Serialize(writer, toSerialize, ns);
                return textWriter.ToString();
            }
        }

        public static T DeserializeFromString<T>(this string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        private static object XmlDeserializeFromString(this string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }

        // eg: get value Промсвязьбанк by name ACQNAME <CustomField Name="ACQNAME" Value="Промсвязьбанк" />
        public static string GetCustomField(XmlNode node, string fieldname)
        {
            string val = "";
            string query = string.Format("CustomField [@Name='{0}']", fieldname);
            var field = node.SelectSingleNode(query);
            if (field == null) return val;
            if (field.Attributes == null) return val;
            if (field.Attributes["Value"] == null) return val;
            val = field.Attributes["Value"].Value;
            return val;
        }

        public static XAttribute Attribute(this XElement el, string name, bool ignoreCase)   // Task #4185
        {
            XAttribute xattr;
            if (ignoreCase)
            {
                xattr = el.Attributes().Where(p => p.Name.ToString().ToLower() == name.ToLower()).FirstOrDefault();
            }
            else
            {
                xattr = el.Attribute(name);
            }
            return xattr;
        }

        public static XElement Element(this XElement el, string name, bool ignoreCase)   // Task #4185
        {
            XElement xel;
            if (ignoreCase)
            {
                xel = el.Elements().Where(p => p.Name.ToString().ToLower() == name.ToLower()).FirstOrDefault();
            }
            else
            {
                xel = el.Element(name);
            }
            return xel;
        }

        public static IEnumerable<XElement> Elements(this XElement el, string name, bool ignoreCase) // Task #4185
        {
            IEnumerable<XElement> xels;
            if (ignoreCase)
            {
                xels = el.Elements().Where(p => p.Name.ToString().ToLower() == name.ToLower());
            }
            else
            {
                xels = el.Elements(name);
            }
            return xels;
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        public static XElement GetXElement(this string xml, string name, LoadOptions option = LoadOptions.None)
        {
            if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(name))
            {
                return default(XElement);
            }

            return XElement.Parse(xml, option).Descendants(name).Single();
        }

        public static IEnumerable<XAttribute> GetXAttributes(this XElement element, string path = null, string attributeName = null)
        {
            if (string.IsNullOrWhiteSpace(path) && string.IsNullOrWhiteSpace(attributeName))
            {
                return element.Attributes();
            }

            if (!string.IsNullOrWhiteSpace(path) && string.IsNullOrWhiteSpace(attributeName))
            {
                return element.XPathSelectElement(path)?.Attributes();
            }

            if (string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(attributeName))
            {
                return element.Attributes(attributeName);
            }

            return element.XPathSelectElement(path)?.Attributes(attributeName);
        }

        public static string GetXmlAttrValue(XmlNode node, string name, string defaultvalue)
        {
            if (node.Attributes != null)
            {
                var attr = node.Attributes[name];
                if (attr != null)
                    return attr.Value;
            }
            return defaultvalue;
        }
    }
}
