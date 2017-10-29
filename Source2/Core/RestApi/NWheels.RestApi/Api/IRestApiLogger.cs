using System;
using NWheels.Kernel.Api.Logging;

namespace NWheels.RestApi.Api
{
    [LoggerComponent]
    public interface IRestApiLogger
    {
        [LogVerbose]
        void RestApiRequestCompleted(string resourceUrl, string verb);

        [LogError]
        void RestApiRequestFailed(string resourceUrl, string verb, Exception error);

        [LogError]
        void RestApiBadRequest(string resourceUrl, string verb);
    }
}