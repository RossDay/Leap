using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vyrolan.VMCS.Actions
{
    class ModeAction : BaseAction
    {
        public string ModeName { get; private set; }
        public bool IsActivate { get; private set; }

        public ModeAction(string name)
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
