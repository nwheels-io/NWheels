using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public interface IEntityId : IEquatable<IEntityId>
    {
        object Value { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEntityId<TEntity> : IEntityId
        where TEntity : class
    {
    }
}
