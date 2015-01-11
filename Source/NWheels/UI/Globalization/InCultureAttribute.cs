using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Globalization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class InCultureAttribute : Attribute
    {
        private readonly string _cultureName;
        private readonly string _localizedValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InCultureAttribute(string cultureName, string localizedValue)
        {
            _cultureName = cultureName;
            _localizedValue = localizedValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string CultureName
        {
            get { return _cultureName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string LocalizedValue
        {
            get { return _localizedValue; }
        }
    }
}
