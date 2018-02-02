#if false // HttpBot is excluded because it isn't covered with any tests

    using System;
using NWheels.Kernel.Api.Logging;

namespace NWheels.Communication.Api.Http
{
    public interface IHttpBotLogger
    {
        [LogDebug]
        void SendingHttpGet(Uri url);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogDebug]
        void SendingHttpPost(Uri url, string formData, string uploadFiles);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogDebug]
        void ResponseSuccess(string status, string url);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        void ResponseFailure(string status, string url);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogDebug]
        void ResponsePayload(string format, string contents);
    }
}

#endif