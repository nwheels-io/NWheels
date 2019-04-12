using NWheels.Domain.Model;

namespace Demo.TrueTix.Domain
{
    [Entity]
    public class SecondarySale
    {
        public int Id { get; set; }
        public Party Seller { get; set; }
        public Ticket Ticket { get; set; }
        public Money Ask { get; set; }
        public string Remarks { get; set; }
    }
}
