using System;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;
using NWheels.Endpoints.Core;
using NWheels.Entities;
using NWheels.Logging;

namespace NWheels.Stacks.AspNet
{
    public interface IWebApplicationLogger : IApplicationEventLogger
    {
        [LogThread(ThreadTaskType.ApiRequest)]
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

        [LogActivity(LogLevel.Audit)]
        ILogActivity StoreEntity(
            [Detail(IncludeInSingleLineText = true, Indexed = true)] string entityName,
            [Detail(IncludeInSingleLineText = true, Indexed = true)] EntityState entityState,
            [Detail(IncludeInSingleLineText = true, Indexed = true)] string entityId,
            [Detail(Indexed = true)] string user,
            [Detail] IEndpoint endpoint, 
            [Detail] IPrincipal principal);

        [LogActivity(LogLevel.Audit)]
        ILogActivity DeleteEntity(
            [Detail(IncludeInSingleLineText = true, Indexed = true)] string entityName,
            [Detail(IncludeInSingleLineText = true, Indexed = true)] string entityId,
            [Detail(Indexed = true)] string user,
            [Detail] IEndpoint endpoint,
            [Detail] IPrincipal principal);
    }
}
