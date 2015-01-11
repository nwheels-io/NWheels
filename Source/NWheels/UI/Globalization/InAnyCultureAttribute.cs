using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Globalization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InAnyCultureAttribute : Attribute
    {
        private readonly string _invariantValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InAnyCultureAttribute(string invariantValue)
        {
            _invariantValue = invariantValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string InvariantValue
        {
            get { return _invariantValue; }
        }
    }
}
