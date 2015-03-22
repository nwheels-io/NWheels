using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    [EntityPartContract]
    public interface IEntityPartId<out TKey>
    {
        TKey Id { get; }
    }
}
