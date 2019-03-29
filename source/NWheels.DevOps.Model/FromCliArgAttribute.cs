using System;

namespace NWheels.DevOps.Model
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class FromCliArgAttribute : Attribute
    {
        public FromCliArgAttribute(string shortName = null, string longName = null)
        {
            ShortName = shortName;
            LongName = longName;
        }

        public string ShortName { get; }
        public string LongName { get; }
    }
}
