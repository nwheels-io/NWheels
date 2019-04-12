using NWheels.Domain.Model;

namespace Demo.TrueTix.Domain
{
    [Entity]
    public class Genre
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}