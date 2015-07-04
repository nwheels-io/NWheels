using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization
{
    public interface IIdentityInfo : IIdentity
    {
        bool IsOfType(Type accountEntityType);
        bool IsInRole(string userRole);
        string[] GetUserRoles();
        string LoginName { get; }
        string QualifiedLoginName { get; }
        string PersonFullName { get; }
        string EmailAddress { get; }
    }
}
