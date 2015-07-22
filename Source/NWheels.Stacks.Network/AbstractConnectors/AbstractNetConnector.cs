using System;
using NWheels.Endpoints;

namespace NWheels.Stacks.Network
{

    public interface IConnectorClient
    {
        void OnPacketReceived(byte[] buf);
        void OnConnectionException(Exception e);
        void OnConnectionGracefulClose();
    }

    public abstract class AbstractNetConnector : IDisposable
    {
        //===============================================================

        public delegate void RoundTripDelayInfo(TimeSpan delayTime);

        //===============================================================

        protected Int32 Id;
        protected AbstractNetConnectorsManager ConnectorsManager;
        protected MessagesDispatcher m_MessagesDispatcher;
        protected ITimeOutUtils TimeOutManager;

        private IConnectorClient _connectorClient;
        protected readonly NetworkApiEndpointRegistration Registration;
        protected readonly INetworkEndpointLogger Logger;

        public static byte[] GracefulCloseMessage = { 0x00, 0x00, 0x4E, 0x57, 0x47, 0x43, 0x00, 0x00 }; // "\0 \0 N W G C \0 \0" NWheels Graceful Close


        public bool IsDisposed { get; private set; }
        protected bool IsOnExcpCalled = false;
        protected bool IsOnGracefulCloseCalled = false;
        protected bool SendGracefulCloseCalled = false;

        //===============================================================

        protected AbstractNetConnector(int id, IConnectorClient connectorClient, NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger)
        {
            Id = id;
            _connectorClient = connectorClient;
            Registration = registration;
            Logger = logger;
        }

        ~AbstractNetConnector()
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
        protected internal void DoOnRecv(object o) { byte[] buf = o as byte[]; _connectorClient.OnPacketReceived(buf); }
        protected internal void DoOnExcp(object o) { Exception e = o as Exception; _connectorClient.OnConnectionException(e); }
        protected internal void DoOnGracefulClose() { _connectorClient.OnConnectionGracefulClose(); }

        //===============================================================
        //===============================================================
        public void RegisterMessageDispatcher(MessagesDispatcher MsgDisp)
        {
            m_MessagesDispatcher = MsgDisp;
        }

        public void SetTimeOutManager(ITimeOutUtils timeOutManager)
        {
            TimeOutManager = timeOutManager;
        }

        //-----------------
        public Int32 ID { get { return Id; } }

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
                if (ConnectorsManager != null)
                {
                    ConnectorsManager.UnregisterConnector(this);
                    ConnectorsManager = null;
                }
            }
        }

        #endregion
    }
}
