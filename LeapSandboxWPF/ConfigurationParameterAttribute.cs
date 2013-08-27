using System;

namespace Vyrolan.VMCS
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    internal sealed class ConfigurationParameterAttribute : Attribute
    {
        public string ParameterName { get; private set; }

        public ConfigurationParameterAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }
    }
}
