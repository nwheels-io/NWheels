using System.Collections.Generic;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.Alerts.Entities
{
    [EntityPartContract(BaseEntityPart = typeof(IEntityPartAlertAction))]
    public interface IEntityPartAlertByEmail : IEntityPartAlertAction
    {
        ICollection<IEntityPartEmailRecipient> Recipients { get; } 
    }
}
