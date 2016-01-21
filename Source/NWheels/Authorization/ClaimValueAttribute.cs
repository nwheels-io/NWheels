using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ClaimValueAttribute : Attribute
    {
        public ClaimValueAttribute(string value)
        {
            if ( string.IsNullOrEmpty(value) )
            {
                throw new ArgumentException("Cliam value cannot be empty or null", "value");
            }

            this.Value = value;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public string Value { get; private set; }
    }
}
