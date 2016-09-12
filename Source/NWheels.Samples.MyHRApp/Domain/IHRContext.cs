using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.Security;
using NWheels.Entities;
using NWheels.Samples.MyHRApp.Authorization;
using NWheels.Stacks.MongoDb;

namespace NWheels.Samples.MyHRApp.Domain
{
    public interface IHRContext : IApplicationDataRepository, IAutoIncrementIdDataRepository, IUserAccountDataRepository
    {
        IEntityRepository<IDepartmentEntity> Departments { get; }
        IEntityRepository<IEmployeeEntity> Employees { get; }
        IEntityRepository<IPositionEntity> Positions { get; }

        IHRUserAccountEntity NewHRUserAccount();
        IHRAdminAccessControlList NewHRAdminAccessControlList();
    }
}
