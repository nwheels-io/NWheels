using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public interface IEntityId : IEquatable<IEntityId>
    {
        object GetValue();
        T GetValue<T>();
        Tuple<T1, T2> GetValue<T1, T2>();
        Tuple<T1, T2, T3> GetValue<T1, T2, T3>();
        Tuple<T1, T2, T3, T4> GetValue<T1, T2, T3, T4>();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEntityId<in TEntity> : IEntityId
        where TEntity : class
    {
    }
}
