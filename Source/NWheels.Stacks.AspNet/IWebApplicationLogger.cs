using System;
using System.Net;
using System.Security.Cryptography;
using NWheels.Logging;

namespace NWheels.Stacks.AspNet
{
    public interface IWebApplicationLogger : IApplicationEventLogger
    {
        [LogThread(ThreadTaskType.IncomingRequest)]
        ILogActivity Request(string verb, string path, [Detail] string query);

        [LogVerbose]
        void RequestCompleted(HttpStatusCode statusCode);

        [LogError]
        void RequestFailed(Exception error);

        [LogError]
        void CommandFailed(string command, Exception error);

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
