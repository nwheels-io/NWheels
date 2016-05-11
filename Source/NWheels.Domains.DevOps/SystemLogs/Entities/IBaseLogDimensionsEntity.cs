using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [EntityContract(IsAbstract = true)]
    public interface IBaseLogDimensionsEntity
    {
        [PropertyContract.EntityId]
        string Id { get; set; }

        //[PropertyContract.Calculated]
        string Machine { get; set; }

        //[PropertyContract.Calculated]
        string Environment { get; set; }

        //[PropertyContract.Calculated]
        string Node { get; set; }

        //[PropertyContract.Calculated]
        string Instance { get; set; }

        //[PropertyContract.Calculated]
        string Replica { get; set; }
    }
}
