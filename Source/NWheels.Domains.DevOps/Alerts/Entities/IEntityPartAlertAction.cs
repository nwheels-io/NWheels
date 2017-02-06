using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.Alerts.Entities
{
    [EntityPartContract(IsAbstract=true)]
    public interface IEntityPartAlertAction
    {
        [PropertyContract.Calculated]
        string AlertType { get; }
    }

    public abstract class EntityPartAlertAction : IEntityPartAlertAction
    {
        [EntityImplementation.CalculatedProperty]
        public abstract string AlertType { get; }
    }
}
