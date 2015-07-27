using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public interface IObjectCollection<T> : ICollection<T>
    {
        T Add();
        T Insert(CollectionItemPosition position);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CollectionItemPosition
    {
        HeadMost,
        Head,
        Middle,
        Tail,
        TailMost
    }
}
