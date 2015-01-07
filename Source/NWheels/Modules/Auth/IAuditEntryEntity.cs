using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Auth
{
    public interface IAuditEntryEntity : IEntityPartCorrelationId
    {
        DateTime When { get; set; }
        IUserAccountEntity Who { get; set; }
        string What { get; set; }
        string Where { get; set; }
        string AffectedEntityType { get; set; }
        string AffectedEntityId { get; set; }
    }
}
