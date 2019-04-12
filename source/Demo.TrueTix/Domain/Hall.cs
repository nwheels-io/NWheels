using NWheels.Domain.Model;

namespace Demo.TrueTix.Domain
{
    [Entity]
    public class Hall
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Venue Venue { get; set; }
    }
}
