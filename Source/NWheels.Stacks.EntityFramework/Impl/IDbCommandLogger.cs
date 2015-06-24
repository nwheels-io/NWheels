using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Stacks.EntityFramework.Impl
{
    public interface IDbCommandLogger : IApplicationEventLogger
    {
        [LogDebug]
        void ExecutingSql([Detail(MaxStringLength = 1024, IncludeInSingleLineText = true)] string statement);
    }
}
