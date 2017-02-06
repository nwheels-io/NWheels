using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.Alerts.Entities
{
    public interface ISystemAlertConfigurationContext : IApplicationDataRepository
    {
        IEntityRepository<ISystemAlertConfigurationEntity> Alerts { get; }
        IEntityPartAlertAction NewEntityPartAlertAction();
        IEntityPartByEmailAlertAction NewEntityPartAlertByEmail();
        IEntityPartEmailRecipient NewEntityPartEmailRecipient();
        IEntityPartEmailAddressRecipient NewEntityPartEmailAddressRecipient();
        IEntityPartUserAccountEmailRecipient NewEntityPartUserAccountEmailRecipient();
    }
}
