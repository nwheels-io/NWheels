using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Entities.Core;

namespace NWheels.Core
{
    public interface IDbConnectionStringResolver
    {
        string ResolveConnectionString(string configuredValue, IStorageInitializer storage, IAccessControlContext context);
        string[] GetAllConnectionStrings(IStorageInitializer storage);
        Type DomainContextType { get; }
    }
}
