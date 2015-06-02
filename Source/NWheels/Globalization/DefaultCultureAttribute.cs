using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Globalization
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class DefaultCultureAttribute : Attribute
    {
        private readonly string _cultureName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DefaultCultureAttribute(string cultureName)
        {
            _cultureName = cultureName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string CultureName
        {
            get { return _cultureName; }
        }
    }
}
