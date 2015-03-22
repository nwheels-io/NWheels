using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    [EntityPartContract]
    public interface IEntityPartCorrelationId
    {
        Guid CorrelationId { get; set; }
    }
}
