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
    public interface IHttpApiEndpointLogger : IApplicationEventLogger
    {
        [LogThread(ThreadTaskType.ApiRequest)]
        ILogActivity Request(string verb, string path, [Detail] string query);

        [LogVerbose]
        void RequestCompleted(HttpStatusCode statusCode);

        [LogError]
        void RequestFailed(Exception error);

        [LogVerbose]
        void EndpointActivating(Uri url, Type contract);

        [LogInfo]
        void EndpointActive(Uri url, Type contract);
 
        [LogInfo]
        void EndpointDeactivated(Uri url, Type contract);
    }
}
