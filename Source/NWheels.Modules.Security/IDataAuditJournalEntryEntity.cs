using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Modules.Security
{
    public interface IDataAuditJournalEntryEntity : IEntityPartCorrelationId
    {
        [PropertyContract(IsRequired = true)]
        IUserAccountEntity Who { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime When { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(IsRequired = true)]
        string ModuleName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string ComponentName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string OperationName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(IsRequired = true)]
        string AffectedEntityName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(IsRequired = true)]
        string AffectedEntityId { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(IsRequired = true)]
        string[] AffectedPropertyNames { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(IsRequired = true)]
        string[] OldPropertyValues { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(IsRequired = true)]
        string[] NewPropertyValues { get; set; }
    }
}
