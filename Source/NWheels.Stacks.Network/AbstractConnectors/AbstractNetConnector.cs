using System;
using NWheels.Endpoints;
using NWheels.Processing.Messages;
using NWheels.Stacks.Network.AbstractConnectors;

namespace NWheels.Stacks.Network
{

    public interface IConnectorClient
    {
        void OnPacketReceived(byte[] buf);
        void OnConnectionException(Exception e);
        void OnConnectionGracefulClose();
    }

    public abstract class AbstractNetBinaryConnector : IDisposable
    {
        //===============================================================

        public delegate void RoundTripDelayInfo(TimeSpan delayTime);

        //===============================================================

        protected NetConnectorsBinaryTransport ConnectorsManager;
        protected MessagesDispatcher m_MessagesDispatcher;

        private IConnectorClient _connectorClient;
        protected readonly IFramework _framework;
        protected IServiceBus _serviceBus;
        protected readonly NetworkApiEndpointRegistration Registration;
        protected readonly INetworkEndpointLogger Logger;

        protected static byte[] GracefulCloseMessage = { 0x00, 0x00, 0x4E, 0x57, 0x47, 0x43, 0x00, 0x00 }; // "\0 \0 N W G C \0 \0" NWheels Graceful Close


        public bool IsDisposed { get; private set; }
        protected bool IsOnExcpCalled = false;
        protected bool IsOnGracefulCloseCalled = false;
        protected bool SendGracefulCloseCalled = false;

        //===============================================================

        protected AbstractNetBinaryConnector(NetConnectorsBinaryTransport connectorsManager,
            IConnectorClient connectorClient,
            IFramework framework,
            NetworkApiEndpointRegistration registration,
            INetworkEndpointLogger logger,
            IServiceBus serviceBus)
        {
            ConnectorsManager = connectorsManager;
            _connectorClient = connectorClient;
            Registration = registration;
            Logger = logger;
            _serviceBus = serviceBus;
            _framework = framework;

            ConnectorsManager.RegisterConnector(this);
        }

        ~AbstractNetBinaryConnector()
        {
            Dispose(false);
        }
        //===============================================================
        //===============================================================
        public abstract void StartKeepAliveService();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roundTripDelayInfoDelgate"></param>
        /// <param name="waitTimeoutInSec">Max time to wait for an answer, the delegate is called on any case. Use 0 for no-timeout</param>
        public abstract void StartRoundTripDelayCheck(RoundTripDelayInfo roundTripDelayInfoDelgate, int waitTimeoutInSec);
        public abstract void StartRecv();
        public abstract void Send(string buf);
        public abstract void Send(byte[] buf);
        public abstract byte[] GetRemoteIpAddress();
        public abstract string GetRemoteIpAddressAsString();
        //-----------------
        protected internal void DoOnRecv(byte[] buf)
        {
            BinaryContentMessage message  = new BinaryContentMessage(buf, null); // _serializer);
            _connectorClient.OnPacketReceived(buf);
        }
        protected internal void DoOnExcp(object o) { Exception e = o as Exception; _connectorClient.OnConnectionException(e); }
        protected internal void DoOnGracefulClose() { _connectorClient.OnConnectionGracefulClose(); }

        //===============================================================
        //===============================================================
        public void RegisterMessageDispatcher(MessagesDispatcher msgDisp)
        {
            m_MessagesDispatcher = msgDisp;
        }

        //-----------------
        public Int32 Id { get; internal set; }

        #region IDisposable Members

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                Dispose(true);
                IsDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.
                ConnectorsManager.UnregisterConnector(this);
            }
        }

        #endregion
    }
}
