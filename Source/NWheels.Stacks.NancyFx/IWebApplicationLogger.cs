using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Stacks.NancyFx
{
    public interface IWebApplicationLogger : IApplicationEventLogger
    {
        [LogVerbose]
        void WebApplicationActivating(string appName, Uri url, string localPath);

        [LogInfo]
        void WebApplicationActive(string appName, Uri url);
 
        [LogInfo]
        void WebApplicationDeactivated(string appName, Uri url);

        [LogDebug]
        void ServingRequest(string appName, string verb, string pathAndQuery);
    }
}
