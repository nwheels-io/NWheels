using System.Collections.Generic;
using NWheels.Domain.Model;

namespace Demo.TrueTix.Domain
{
    [Entity]
    public class Artist
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Genre> Genres { get; set; }
    }
}