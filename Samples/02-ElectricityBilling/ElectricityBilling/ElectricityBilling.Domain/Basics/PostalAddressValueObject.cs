using System;
using System.Collections.Generic;
using System.Text;
using NWheelsTempApiLib;

namespace ElectricityBilling.Domain.Basics
{
    public class PostalAddressValueObject
    {
        public string ToDisplayString(ILocalizationService localization)
        {
            return ThisObject.FormatDisplayString(localization);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Street { get; }
        public string Number { get; }
        public string City { get; }
        public string State { get; }
        public string Country { get; }
        public string Zip { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PostalAddressValueObject PostalAddress { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MemberContract.InjectedDependency]
        protected IThisDomainObjectServices ThisObject { get; }

    }
}
