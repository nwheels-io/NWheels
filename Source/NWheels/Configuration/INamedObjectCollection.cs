using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Configuration
{
    public interface INamedObjectCollection<TElement> : ICollection<TElement>
    {
        TElement Add(string name);
        TElement Insert(string name, CollectionItemPosition position);
        bool TryGetElementByName(string name, out TElement element);
        TElement this[string Name] { get; }
    }
}
