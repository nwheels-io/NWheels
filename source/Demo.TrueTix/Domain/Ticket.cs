using NWheels.Domain.Model;

namespace Demo.TrueTix.Domain
{
    [Entity]
    public class Ticket
    {
        public int Id { get; set; }
        public Performance Performance { get; set; }
        public string Barcode { get; set; }
        public Seat Seat { get; set; }
        public Money FacePrice { get; set; }
        public Party Owner { get; set; }
    }
}
