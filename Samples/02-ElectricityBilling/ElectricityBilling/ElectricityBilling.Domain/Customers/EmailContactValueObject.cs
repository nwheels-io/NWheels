using NWheels;

namespace ElectricityBilling.Domain.Customers
{
    public class EmailContactValueObject : ContactDetailValueObject
    {
        [MemberContract.Semantics.EmailAddress]
        [MemberContract.Presentation.DefaultObjectDisplay]
        public string EmailAddress { get; set; }
    }
}