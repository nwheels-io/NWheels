using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.TrueTix.Domain
{
    public class Show
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Artist Artist { get; set; }
        public List<Genre> Genres { get; set; }
    }
}
