using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Modules.Security
{
    [EntityPartContract]
    public interface IEntityPartAudit
    {
        DateTime CreatedAt { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(IsRequired = true)]
        IUserAccountEntity CreatedBy { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime ModifiedAt { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract(IsRequired = true)]
        IUserAccountEntity ModifiedBy { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityPartContract]
    public interface IEntityPartAuditSoftDelete : IEntityPartAudit, IEntityPartSoftDelete
    {
        IUserAccountEntity DeletedBy { get; set; }
    }
}
