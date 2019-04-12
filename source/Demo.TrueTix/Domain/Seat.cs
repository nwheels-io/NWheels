using NWheels.Domain.Model;

namespace Demo.TrueTix.Domain
{
    [Entity]    
    public class Seat
    {
        public int Id { get; set; }
        public SeatingPlanRow Row { get; set; }
        public string Label { get; set; }
        public int Number { get; set; }
    }
}
