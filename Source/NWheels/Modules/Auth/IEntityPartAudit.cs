using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Modules.Auth
{
    [EntityPartContract]
    public interface IEntityPartAudit
    {
        DateTime CreatedAt { get; set; }
        [Required]
        IUserAccountEntity CreatedBy { get; set; }
        DateTime ModifiedAt { get; set; }
        [Required]
        IUserAccountEntity ModifiedBy { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityPartContract]
    public interface IEntityPartAuditSoftDelete : IEntityPartAudit, IEntityPartSoftDelete
    {
        IUserAccountEntity DeletedBy { get; set; }
    }
}
