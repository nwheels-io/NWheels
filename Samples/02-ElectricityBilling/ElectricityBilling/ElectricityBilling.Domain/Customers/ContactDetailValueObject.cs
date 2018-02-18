using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain.Basics;
using NWheels;
using NWheels.I18n;

namespace ElectricityBilling.Domain.Customers
{
    public abstract class ContactDetailValueObject
    {
        public bool IsPrimary { get; internal set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EmailContactValueObject : ContactDetailValueObject
    {
        [MemberContract.Semantics.EmailAddress]
        [MemberContract.Presentation.DefaultObjectDisplay]
        public string EmailAddress { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class PostalContactValueObject : ContactDetailValueObject
    {
        [MemberContract.Presentation.DefaultObjectDisplay]
        public PostalAddressValueObject PostalAddress { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class PhoneContactValueObject : ContactDetailValueObject
    {
        [MemberContract.Semantics.PhoneNumber]
        [MemberContract.Presentation.DefaultObjectDisplay]
        public string PhoneNumber { get; set; }
    }
}
