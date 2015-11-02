using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Authorization
{
    public interface IAuthorizationLogger : IApplicationEventLogger
    {
        [LogError]
        SecurityException NoRuleDefinedForEntity(Type contract);
    }
}
