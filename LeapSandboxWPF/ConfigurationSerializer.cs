using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Vyrolan.VMCS
{
    internal class ConfigurationSerializer
    {
        public static string ToXml(object obj)
        {
            var type = obj.GetType();
            var baseType = type;
            while (baseType.BaseType != typeof(object))
                baseType = baseType.BaseType;
            var baseTypeName = baseType.Name.Replace("Base", "");

            var xml = new StringBuilder();
            xml.AppendFormat("<{0} type=\"{1}\" ", baseTypeName, type.Name);

            var props = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var param = prop.GetCustomAttributes(typeof(ConfigurationParameterAttribute), false).Cast<ConfigurationParameterAttribute>().FirstOrDefault();
                if (param == null) continue;

                xml.AppendFormat("{0}=\"{1}\" ", param.ParameterName, prop.GetValue(obj, null));
            }

            xml.Append(" />");
            return xml.ToString();
        }

        public static object FromXml(XmlNode xml)
        {
            var ns = "Vyrolan.VMCS." + xml.Name + "s.";
            var typeName = xml.Attributes.GetNamedItem("type").Value;
            var type = Type.GetType(ns+typeName);

            var cons = type.GetConstructor(new[] { typeof(String) });
            var obj = cons.Invoke(new[] { xml.Attributes.GetNamedItem("name").Value });

            var props = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var param = prop.GetCustomAttributes(typeof(ConfigurationParameterAttribute), false).Cast<ConfigurationParameterAttribute>().FirstOrDefault();
                if (param == null || param.ParameterName == "name") continue;

                prop.SetValue(obj, Convert.ChangeType(xml.Attributes.GetNamedItem(param.ParameterName).Value, prop.PropertyType), null);
            }

            return obj;
        }
    }
}
