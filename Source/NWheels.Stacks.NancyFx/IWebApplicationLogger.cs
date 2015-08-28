using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using NWheels.Logging;

namespace NWheels.Stacks.NancyFx
{
    public interface IWebApplicationLogger : IApplicationEventLogger
    {
        [LogThread(ThreadTaskType.IncomingRequest)]
        ILogActivity Request(string verb, string path, [Detail] string query);

        [LogVerbose]
        void RequestCompleted(HttpStatusCode statusCode);

        [LogError]
        void RequestFailed(Exception error);

        [LogVerbose]
        void WebApplicationActivating(string appName, Uri url, string localPath);

        [LogInfo]
        void WebApplicationActive(string appName, Uri url);
 
        [LogInfo]
        void WebApplicationDeactivated(string appName, Uri url);

        [LogWarning]
        void FailedToDecryptSessionCookie(CryptographicException error);
    }
}
