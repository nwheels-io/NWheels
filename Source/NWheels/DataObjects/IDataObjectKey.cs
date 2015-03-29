using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public interface IDataObjectKey : IEquatable<IDataObjectKey>, IComparable<IDataObjectKey>
    {
        IDataObjectKey<T> As<T>();
        object Value { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDataObjectKey<T> : IDataObjectKey
    {
        new T Value { get; set; }
    }
}
