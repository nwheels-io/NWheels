using System;

namespace NWheels.DevOps.Model
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class FromEnvVarAttribute : Attribute
    {
        public FromEnvVarAttribute(string varName = null)
        {
            VarName = varName;
        }

        public string VarName { get; }
    }
}