using NWheels.Entities;
using System.Collections.Generic;

namespace NWheels.Domains.DevOps.Alerts.Entities
{
    [EntityPartContract(BaseEntityPart = typeof(IEntityPartAlertAction))]
    public interface IEntityPartAlertByEmail : IEntityPartAlertAction
    {
        ICollection<IEntityPartEmailRecipient> Recipients { get; } 
    }

    public abstract class EntityPartAlertByEmail : IEntityPartAlertByEmail
    {
        public abstract ICollection<IEntityPartEmailRecipient> Recipients { get; }

        public virtual string SummaryText
        {
            get { return "EntityPartAlertByEmail"; }//nameof(EntityPartAlertByEmail);
        }
    }
}
