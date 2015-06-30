using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Domains.Security
{
    public interface IUserAccountDataRepository : IApplicationDataRepository
    {
        IEntityRepository<IUserAccountEntity> AllUsers { get; }
        IEntityRepository<IBackEndUserAccountEntity> BackEndUsers { get; }
        IEntityRepository<IFrontEndUserAccountEntity> FrontEndUsers { get; }
        IBackEndUserAccountEntity NewBackEndUser();
        IFrontEndUserAccountEntity NewFrontEndUser();
        IPasswordEntity NewPassword();
    }
}
