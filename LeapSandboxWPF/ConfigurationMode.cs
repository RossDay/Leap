using System;
using System.Collections.Generic;
using System.Linq;

namespace Vyrolan.VMCS
{
    internal class ConfigurationMode : IEquatable<ConfigurationMode>
    {
        private Dictionary<string, string> _Mappings = new Dictionary<string, string>();
        internal IDictionary<string, string> Mappings { get { return _Mappings; } }
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
            return ConfigurationSerializer.ModeToXml(this);
        }

        public static ConfigurationMode FromXml(System.Xml.XmlNode xml)
        {
            return ConfigurationSerializer.ModeFromXml(xml);
        } 
        #endregion

        #region Equals / GetHashCode
        public override bool Equals(object obj)
        {
            var mode = obj as ConfigurationMode;
            if (mode == null) return false;
            return Equals(mode);
        }

        public bool Equals(ConfigurationMode other)
        {
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        } 
        #endregion
    }
}
