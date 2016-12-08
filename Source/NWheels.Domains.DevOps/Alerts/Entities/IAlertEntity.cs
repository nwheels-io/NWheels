using System.Collections.Generic;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.Alerts.Entities
{
    [EntityContract]
    public interface IAlertEntity
    {
        string Id { get; set; }
        string Description { get; set; }

        ICollection<IEntityPartAlertAction> Actions { get; }

    }
}
