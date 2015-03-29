using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Entities
{
    [EntityPartContract]
    public interface IEntityPartId<out TKey>
    {
        [PropertyContract.Key]
        TKey Id { get; }
    }
}
