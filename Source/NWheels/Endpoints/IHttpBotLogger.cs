using System;
using NWheels.Logging;

namespace NWheels.Endpoints
{
    public interface IHttpBotLogger : IApplicationEventLogger
    {
        [LogDebug]
        void SendingHttpGet(Uri url);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogDebug]
        void SendingHttpPost(Uri url, [Detail] string formData, [Detail] string uploadFiles);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogDebug]
        void ResponseSuccess(string status, [Detail] string url);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        void ResponseFailure(string status, [Detail] string url);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogDebug]
        void ResponsePayload(string format, [Detail] string contents);
    }
}
