using NWheels;

namespace ElectricityBilling.Domain.Customers
{
    public class PhoneContactValueObject : ContactDetailValueObject
    {
        [MemberContract.Semantics.PhoneNumber]
        [MemberContract.Presentation.DefaultObjectDisplay]
        public string PhoneNumber { get; set; }
    }
}