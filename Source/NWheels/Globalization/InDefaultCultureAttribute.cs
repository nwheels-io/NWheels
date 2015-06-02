using System;

namespace NWheels.Globalization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InDefaultCultureAttribute : Attribute
    {
        private readonly string _localizedValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InDefaultCultureAttribute(string localizedValue)
        {
            _localizedValue = localizedValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string LocalizedValue
        {
            get { return _localizedValue; }
        }
    }
}
