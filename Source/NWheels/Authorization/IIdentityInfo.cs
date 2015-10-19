using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Core;

namespace NWheels.Authorization
{
    public interface IIdentityInfo : IIdentity
    {
        bool IsOfType(Type accountEntityType);
        bool IsInRole(string userRole);
        string[] GetUserRoles();
        IRuntimeEntityAccessRule<TEntity> GetEntityAccessRule<TEntity>();
        string UserId { get; }
        string LoginName { get; }
        string QualifiedLoginName { get; }
        string PersonFullName { get; }
        string EmailAddress { get; }
    }
}
