using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public interface IPartitionedRepository<TEntity, TPartition>
    {
        IEntityRepository<TEntity> this[TPartition partition] { get; }
    }
}
