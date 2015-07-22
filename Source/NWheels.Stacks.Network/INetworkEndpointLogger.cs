using System;
using NWheels.Logging;

namespace NWheels.Stacks.Network
{
    public interface INetworkEndpointLogger : IApplicationEventLogger
    {

        [LogInfo]
        void NetworkEndpointListening(string listenUrl, string contract);

        [LogInfo]
        void NetworkEndpointClosed(string listenUrl, string contract);

        [LogError]
        void NetworkEndpointOpenFailed(string listenUrl, string contract, Exception error);

        //--------------------------------------------------------------------------------------

        [LogError]
        void SendFailed(string listenUrl, string contract, Exception e);

        [LogError]
        void StartListeningFailed(string listenUrl, string contract, Exception e);

        [LogDebug]
        void WaitingAcceptOnPort(string listenUrl, string contract);

        //--------------------------------------------------------------------------------------

        [LogDebug]
        void NewConnectorOnSend(string listenUrl, string contract);

        [LogDebug]
        void NewConnectorOnReceive(string listenUrl, string contract, string receivedData);

        [LogDebug]
        void NewConnectorOnReceiveParseFailed(string listenUrl, string contract, string receivedData);

        [LogError]
        void NewConnectorExceptionOccurred(string listenUrl, string contract, Exception e);
        
        //--------------------------------------------------------------------------------------

        [LogError]
        void ConnectorOnReceiveException(string listenUrl, string contract, Exception e);

        [LogDebug]
        void ConnectorDisposing(string listenUrl, string contract, int id);

        [LogDebug]
        void ReceivedKeepAliveTimeOut(string listenUrl, string contract, int connectorId, int timesCalled, string message);
    }
}
