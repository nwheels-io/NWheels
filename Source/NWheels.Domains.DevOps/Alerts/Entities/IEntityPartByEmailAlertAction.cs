using NWheels.Entities;
using System.Collections.Generic;

namespace NWheels.Domains.DevOps.Alerts.Entities
{
    [EntityPartContract(BaseEntityPart = typeof(IEntityPartAlertAction))]
    public interface IEntityPartByEmailAlertAction : IEntityPartAlertAction
    {
        ICollection<IEntityPartEmailRecipient> Recipients { get; } 
    }

    public abstract class EntityPartByEmailAlertAction : EntityPartAlertAction, IEntityPartByEmailAlertAction
    {
        public abstract ICollection<IEntityPartEmailRecipient> Recipients { get; }

        public override string AlertType
        {
            get { return "Alert by email"; }//nameof(EntityPartAlertByEmail);
        }
    }
}
