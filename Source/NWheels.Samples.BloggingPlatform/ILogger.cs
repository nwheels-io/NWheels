#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;
using NWheels.Modules.Security;
using NWheels.Samples.BloggingPlatform.Domain;

namespace NWheels.Samples.BloggingPlatform
{
    public interface ILogger : IApplicationEventLogger
    {
        [LogError]
        AuthorizationException UserHasNoAuthorizationForSpecifiedBlog(IUserAccountEntity user, IBlogEntity blog);

        [LogError]
        AuthorizationException UserIsNotAuthorizedToPerformRequestedOperation(IUserAccountEntity user, IBlogEntity blog);
    }
}

#endif