using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [EntityContract(IsAbstract = true)]
    public interface IBaseLogDimensionsEntity
    {
        [PropertyContract.EntityId]
        string Id { get; }

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
