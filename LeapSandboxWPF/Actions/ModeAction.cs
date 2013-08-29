using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vyrolan.VMCS.Actions
{
    class ModeAction : BaseAction
    {
        private static IDictionary<string, ModeAction> _Instances = new Dictionary<string, ModeAction>();
        public static ModeAction Create(string name)
        {
            lock (_Instances)
            {
                ModeAction a;
                if (!_Instances.TryGetValue(name, out a))
                {
                    a = new ModeAction(name);
                    _Instances.Add(name, a);
                }
                return a;
            }
        }

        public string ModeName { get; private set; }
        public bool IsActivate { get; private set; }

        private ModeAction(string name)
            : base(name)
        {
            ModeName = name.Substring(2);
            IsActivate = name.StartsWith("AM:");
        }

        public bool IsMatch(string modeName, bool isActivate)
        {
            return ModeName.Equals(modeName) && IsActivate.Equals(isActivate);
        }

        protected override void BeginImpl()
        {
        }
        protected override void EndImpl()
        {
        }
    }
}
