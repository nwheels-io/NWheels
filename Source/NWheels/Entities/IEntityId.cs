using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public interface IEntityId : IEquatable<IEntityId>
    {
        T ValueAs<T>();
        object Value { get; }
        Type ContractType { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEntityId<in TEntity> : IEntityId
    {
    }
}
