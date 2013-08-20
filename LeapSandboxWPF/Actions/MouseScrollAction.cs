using Vyrolan.VMCS.Triggers;

namespace Vyrolan.VMCS.Actions
{
    class MouseScrollAction : BaseAction
    {
        public int Lines { get; set; }

        public MouseScrollAction(BaseTrigger trigger)
            : base(trigger)
        {
        }

        protected override void Begin()
        {
            InputSimulator.Mouse.VerticalScroll(Lines);
        }

        protected override void End()
        {
        }
    }

}
