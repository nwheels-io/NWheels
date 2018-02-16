using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain.Basics;
using NWheelsTempApiLib;

namespace ElectricityBilling.Domain.Customers
{
    public abstract class ContactDetailValueObject
    {
        public abstract string ToDisplayString(ILocalizationService localization);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EmailContactValueObject : ContactDetailValueObject
    {
        public override string ToDisplayString(ILocalizationService localization)
        {
            return this.EmailAddress;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MemberContract.Semantics.Email]
        public string EmailAddress { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class PostalContactValueObject : ContactDetailValueObject
    {
        public override string ToDisplayString(ILocalizationService localization)
        {
            return PostalAddress.ToDisplayString(localization);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PostalAddressValueObject PostalAddress { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class PhoneContactValueObject : ContactDetailValueObject
    {
        public override string ToDisplayString(ILocalizationService localization)
        {
            return PostalAddress.ToDisplayString(localization);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PostalAddressValueObject PostalAddress { get; set; }
    }
}
