using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vyrolan.VMCS.Actions;
using Vyrolan.VMCS.Gestures;
using Vyrolan.VMCS.Triggers;

namespace Vyrolan.VMCS
{
    internal class Configuration
    {
        private static Configuration _Instance = new Configuration();
        public static Configuration Instance { get { return _Instance; } }
        private Configuration() { }

        private readonly object _Lock = new object();

        private readonly Dictionary<string, BaseTrigger> _Triggers = new Dictionary<string, BaseTrigger>();
        private readonly Dictionary<string, BaseAction> _Actions = new Dictionary<string, BaseAction>();
        private readonly Dictionary<string, ConfigurationMode> _Modes = new Dictionary<string, ConfigurationMode>();
        public IDictionary<string, BaseTrigger> Triggers { get { lock(_Lock) return _Triggers; } }
        public IDictionary<string, BaseAction> Actions { get { lock (_Lock) return _Actions; } }
        public IDictionary<string, ConfigurationMode> Modes { get { lock (_Lock) return _Modes; } }

        [ConfigurationParameter("mouseSensitivityX")]
        public int MouseSensitivityX { get; set; }
        [ConfigurationParameter("mouseSensitivityY")]
        public int MouseSensitivityY { get; set; }

        public event EventHandler<TriggerEventArgs> TriggerChanged;
        void OnTriggered(object sender, TriggerEventArgs e)
        {
            if (TriggerChanged != null)
                TriggerChanged(sender, e);
        }

        public string ToXml()
        {
            var xml = new StringBuilder();
            xml.AppendLine("<Configuration>");
            xml.Append(ConfigurationSerializer.SettingsToXml());

            xml.AppendLine("<Triggers>");
            foreach (var trigger in Triggers.Values)
                xml.Append("  ").AppendLine(trigger.ToXml());
            xml.AppendLine("</Triggers>");
            
            xml.AppendLine("<Actions>");
            foreach (var action in Actions.Values)
                xml.Append("  ").AppendLine(action.ToXml());
            xml.AppendLine("</Actions>");

            xml.AppendLine("<Modes>");
            foreach (var mode in Modes.Values)
                xml.Append(mode.ToXml());
            xml.AppendLine("</Modes>");

            xml.AppendLine("</Configuration>");
            return xml.ToString();
        }

    }
}
