using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Contracts
{
    public static class PropertyContract
    {
        [AttributeUsage(AttributeTargets.Property)]
        public class RequiredAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class ReadOnlyAttribute : Attribute
        {
        }

        public static class Presentation
        {
            [AttributeUsage(AttributeTargets.Property)]
            public class LabelAttribute : Attribute
            {
                public LabelAttribute(string value)
                {
                }
            }
        }
    }
}
