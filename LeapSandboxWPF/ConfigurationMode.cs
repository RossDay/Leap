using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Vyrolan.VMCS
{
    internal class ConfigurationMode
    {
        private Dictionary<string, string> _Mappings = new Dictionary<string, string>();
        public string Name { get; private set; }

        public ConfigurationMode(string name)
        {
            Name = name;
        }

        public bool HasTrigger(string triggerName)
        {
            return _Mappings.ContainsKey(triggerName);
        }
        public string GetActionForTrigger(string triggerName)
        {
            string action;
            if (!_Mappings.TryGetValue(triggerName, out action))
                action = null;
            return action;
        }

        #region To/From XML
        public string ToXml()
        {
            var xml = new StringBuilder();
            xml.AppendFormat("<Mode name=\"{0}\">", Name).AppendLine();
            foreach (var map in _Mappings)
                xml.AppendFormat("  <Map trigger=\"{0}\" action=\"{1}\" />", map.Key, map.Value).AppendLine();
            xml.AppendLine("</Mode>");
            return xml.ToString();
        }

        public static ConfigurationMode FromXml(XmlNode xml)
        {
            var name = xml.Attributes.GetNamedItem("name").Value;
            var mode = new ConfigurationMode(name);

            foreach (XmlNode map in xml.SelectNodes("/Mode/Map"))
            {
                var trigger = map.Attributes.GetNamedItem("trigger").Value;
                var action = map.Attributes.GetNamedItem("action").Value;
                if (mode._Mappings.ContainsKey(trigger))
                    continue; // must be bad config
                mode._Mappings.Add(trigger, action);
            }

            return mode;
        } 
        #endregion

        #region Equals / GetHashCode
        public override bool Equals(object obj)
        {
            var mode = obj as ConfigurationMode;
            if (mode == null) return false;
            return Name.Equals(mode.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        } 
        #endregion
    }
}
