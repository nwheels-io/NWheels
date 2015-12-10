using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;

namespace NWheels.Core
{
    public interface IDatabaseNameResolver
    {
        string ResolveDatabaseName(string configuredDatabaseName, IAccessControlContext context);
        Type DomainContextType { get; }
    }
}
