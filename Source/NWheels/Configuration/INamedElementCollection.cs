using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Configuration
{
    public interface INamedElementCollection<TElement> : ICollection<TElement>
    {
        bool TryGetElementByName(string name, out TElement element);
        TElement this[string Name] { get; }
    }
}
