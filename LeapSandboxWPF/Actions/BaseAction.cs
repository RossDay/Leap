using Vyrolan.VMCS.Triggers;
using WindowsInput;

namespace Vyrolan.VMCS.Actions
{
    internal abstract class BaseAction
    {
        protected static InputSimulator InputSimulator = new InputSimulator();
        public bool IsFiring { get; protected set; }

        protected BaseAction(BaseTrigger trigger)
        {
            trigger.Triggered += OnTriggered;
        }

        private void OnTriggered(object sender, TriggerEventArgs e)
        {
            if (e.IsTriggered)
            {
                IsFiring = true;
                Begin();
            }
            else
            {
                End();
                IsFiring = false;
            }
        }

        protected abstract void Begin();
        protected abstract void End();
    }
}
