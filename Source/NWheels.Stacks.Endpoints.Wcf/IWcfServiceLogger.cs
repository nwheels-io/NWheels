using System;
using NWheels.Logging;

namespace NWheels.Stacks.Endpoints.Wcf
{
    public interface IWcfServiceLogger : IApplicationEventLogger
    {
        [LogInfo]
        void ServiceHostOpen(string listenUrl, string metadataUrl);

        [LogInfo]
        void ServiceHostClosed(string listenUrl, string metadataUrl);

        [LogThread(ThreadTaskType.ApiRequest)]
        ILogActivity HandlingRequest(string action);

        [LogDebug]
        void IncomingRequest(
            [Detail(ContentTypes = LogContentTypes.CommunicationMessage, MaxStringLength = 4096, IncludeInSingleLineText = false)] 
                string requestXml);

        [LogDebug]
        void OutgoingResponse(
            [Detail(ContentTypes = LogContentTypes.CommunicationMessage, MaxStringLength = 4096, IncludeInSingleLineText = false)] 
                string responseXml);

        [LogError]
        void FaultResponse(
            [Detail(ContentTypes = LogContentTypes.CommunicationMessage, MaxStringLength = 4096, IncludeInSingleLineText = false)] 
                string responseXml);

        [LogError]
        void OperationFailed(Exception error);
    }
}