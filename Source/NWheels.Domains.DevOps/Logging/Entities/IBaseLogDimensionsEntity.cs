using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.Logging.Entities
{
    [EntityContract(IsAbstract = true)]
    public interface IBaseLogDimensionsEntity
    {
        [PropertyContract.EntityId]
        string Id { get; set; }

        [PropertyContract.Calculated]
        string Machine { get; }

        [PropertyContract.Calculated]
        string Environment { get; }

        [PropertyContract.Calculated]
        string Node { get; }

        [PropertyContract.Calculated]
        string Instance { get; }

        [PropertyContract.Calculated]
        string Replica { get; }
    }
}
