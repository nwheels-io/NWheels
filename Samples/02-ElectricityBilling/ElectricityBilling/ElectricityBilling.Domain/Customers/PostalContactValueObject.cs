using ElectricityBilling.Domain.Basics;
using NWheels;

namespace ElectricityBilling.Domain.Customers
{
    public class PostalContactValueObject : ContactDetailValueObject
    {
        [MemberContract.Presentation.DefaultObjectDisplay]
        public PostalAddressValueObject PostalAddress { get; set; }
    }
}