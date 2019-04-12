namespace NWheels.Domain.Model
{
    [ValueObject]
    public abstract class ContactMethod
    {
    }

    [ValueObject]
    public class EmailContactMethod : ContactMethod
    {
        public string Email { get; set; }
    }

    [ValueObject]
    public class PhoneContactMethod : ContactMethod
    {
        public string Phone { get; set; }
    }

    [ValueObject]
    public class PostContactMethod : ContactMethod
    {
        public PostalAddress Address { get; set; }
    }

    [ValueObject]
    public class SocialContactMethod : ContactMethod
    {
        public string NetworkName { get; set; }
        public string AccountId { get; set; }
    }
}
