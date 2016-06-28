using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    public interface IWriteOnlyCollection<in T>
    {
        void Add(T item);
        int Count { get; }
    }
}
