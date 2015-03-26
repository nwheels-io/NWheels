using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Auth
{
    public interface IDataAuditJournalEntryEntity : IEntityPartCorrelationId
    {
        IUserAccountEntity Who { get; set; }
        DateTime When { get; set; }
        string ModuleName { get; set; }
        string ComponentName { get; set; }
        string OperationName { get; set; }
        string AffectedEntityName { get; set; }
        string AffectedEntityId { get; set; }
        string[] AffectedPropertyNames { get; set; }
        string[] OldPropertyValues { get; set; }
        string[] NewPropertyValues { get; set; }
    }
}
