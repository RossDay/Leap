using System;

namespace LeapSandboxWPF
{
    class TriggerEventArgs : EventArgs
    {
        public bool IsTriggered { get; set; }
    }

    class BaseTrigger
    {
        public string Name { get; set; }

        private bool _IsTriggered;
        public bool IsTriggered
        {
            get { return _IsTriggered; }
            set
            {
                if (!_IsTriggered.Equals(value))
                {
                    _IsTriggered = value;
                    OnTriggered();
                }
            }
        }

        public event EventHandler<TriggerEventArgs> Triggered;
        protected void OnTriggered()
        {
            if (Triggered != null)
                Triggered(this, new TriggerEventArgs { IsTriggered = IsTriggered });
        }
    }
}
