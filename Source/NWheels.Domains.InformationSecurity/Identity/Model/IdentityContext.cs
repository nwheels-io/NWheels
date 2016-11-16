using System;
using NWheels.Api;
using NWheels.Api.Ddd;

namespace NWheels.Domains.InformationSecurity.Identity.Model
{
    [DomainModel.BoundedContext]
    public class IdentityContext
    {
        protected IEntityRepository<UserAccount> UserAccounts { get; set; }
    }
}
