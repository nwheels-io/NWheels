using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using NWheels.Endpoints;

//AbstractConnector
namespace NWheels.Stacks.Network
{
    public class ConnectorDisconnectedException : Exception
    {
    }


    public class SocketConnector : AbstractNetConnector, IDisposable
    {
        protected Socket Socket;
        private readonly int _receiveBufferSize;
        //
        // Keep alive handling
        //
        private bool _isKeepAliveRunning;
        private TimeOutHandle _sendKeepAliveTimeOutHandle;
        private TimeOutHandle _receiveKeepAliveTimeOutHandle;
        private UInt16 _receiveKeepAliveTimeOutCounter;
        private const uint SendKeepAliveTimeOutValue = 20;
        private const uint ReceiveKeepAliveTimeOutValue = 40;
        private static readonly byte[] _s_keepAliveMessage = { 0x00, 0x00, 0x4E, 0x57, 0x4B, 0x41, 0x00, 0x00 };     // "\0 \0 N W K A \0 \0" NWheels Keep Alive

        //
        // Round trip delay check handling
        //
        private bool _isRoundTripDelayCheckRunning;
        private TimeOutHandle _sendPingTimeOutHandle;
        private const uint SendPingTimeOutValue = 20;
        private DateTime _lastPingSentTime;
        private static readonly byte[] _s_roundTripPingMessage = { 0x00, 0x00, 0x4E, 0x57, 0x50, 0x49, 0x00, 0x00 }; // "\0 \0 N W P I \0 \0" NWheels Ping for roundtrip delay check
        private static readonly byte[] _s_roundTripPongMessage = { 0x00, 0x00, 0x4E, 0x57, 0x50, 0x4F, 0x00, 0x00 }; // "\0 \0 N W P O \0 \0" NWheels Pong for roundtrip delay check
        private RoundTripDelayInfo _roundTripDelayInfoDelgate;
        private uint _roundTripDelayWaitTimeoutInSec;
        private TimeOutHandle _waitPongTimeOutHandle;

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        private void OnSendKeepAliveTimeOutDelegate(object param)
        {
            if (IsDisposed)
            {
                return;
            }

            //MyLogger.DebugFormat("AbstractConnector.OnSendKeepAliveTimeOutDelegate() called for {0}", m_id);
            _sendKeepAliveTimeOutHandle = null;
            TcpSocketsUtils.Send(Socket, _s_keepAliveMessage, _OnSend, OnExcp); //do not use Send(), since it is virtual!!!
            ResetSendKeepAliveTimeOut();
        }

        /// <summary>
        /// A remote connector didn't send any packet for a defined period of time -
        /// this means a disconnection has occured.
        /// Throws ConnectoerDisconnectedException in case no delegate had been registered.
        /// </summary>
        /// <param name="param"></param>
        private void OnReceiveKeepAliveTimeOutDelegate(object param)
        {
            if (IsDisposed)
            {
                return;
            }

            _receiveKeepAliveTimeOutCounter++;
            string logMsg = AmISensitiveToKeepAliveTimeOut() ? "will be closed after 5 times" : "will not be closed";
            Logger.ReceivedKeepAliveTimeOut(Registration.Address.ToString(), Registration.Contract.FullName, Id, _receiveKeepAliveTimeOutCounter, logMsg);

            if (_receiveKeepAliveTimeOutHandle != null)
            {
                // In case of debugging (breakpoints), then we might reach here even if we don't need to.
                // so, in order to avoid any future redundant timeouts - cancel the current TO.
                TimeOutManager.CancelTimeOutEvent(_receiveKeepAliveTimeOutHandle);
                _receiveKeepAliveTimeOutHandle = null;
            }

            bool doResetTimer = true;

            if (_receiveKeepAliveTimeOutCounter == 5 && AmISensitiveToKeepAliveTimeOut())
            {
                doResetTimer = false;
                if (IsOnExcpCalled == false && IsOnGracefulCloseCalled == false)
                {
                    DoOnExcp(new ConnectorDisconnectedException());
                }
                this.Dispose();
            }
            if (doResetTimer)
            {
                ResetReceiveKeepAliveTimeOut();
            }
        }

        private void ResetSendKeepAliveTimeOut()
        {
            if (_isKeepAliveRunning == false || TimeOutManager == null)
            {
                return;
            }

            lock (this)
            {
                if (IsDisposed)
                {
                    return;
                }
                if (_sendKeepAliveTimeOutHandle != null)
                {
                    TimeOutManager.CancelTimeOutEvent(_sendKeepAliveTimeOutHandle);
                }

                _sendKeepAliveTimeOutHandle = TimeOutManager.AddTimeOutEvent(
                        SendKeepAliveTimeOutValue,
                        OnSendKeepAliveTimeOutDelegate,
                        null);
            }
        }

        private void ResetReceiveKeepAliveTimeOut()
        {
            if (_isKeepAliveRunning == false || TimeOutManager == null)
            {
                return;
            }
            lock (this)
            {
                if (IsDisposed)
                {
                    return;
                }

                if (_receiveKeepAliveTimeOutHandle != null)
                {
                    TimeOutManager.CancelTimeOutEvent(_receiveKeepAliveTimeOutHandle);
                }

                _receiveKeepAliveTimeOutHandle = TimeOutManager.AddTimeOutEvent(
                        ReceiveKeepAliveTimeOutValue,
                        OnReceiveKeepAliveTimeOutDelegate,
                        null);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        private void OnSendPingTimeOutDelegate(object param)
        {
            SendRoundTripPing();
        }

        // Pong reply didn't arrive
        private void OnWaitPongTimeOutDelegate(object param)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan roundTripTime = now - _lastPingSentTime;

            lock (this)
            {
                if (_waitPongTimeOutHandle != null)
                {
                    TimeOutManager.CancelTimeOutEvent(_waitPongTimeOutHandle);
                    _waitPongTimeOutHandle = null;
                }
            }

            if (_roundTripDelayInfoDelgate != null)
            {
                _roundTripDelayInfoDelgate(roundTripTime);
            }
        }

        private void ResetRoundTripDelayCheck()
        {
            if (_isRoundTripDelayCheckRunning == false || TimeOutManager == null)
            {
                return;
            }

            lock (this)
            {
                if (IsDisposed)
                {
                    return;
                }
                if (_sendPingTimeOutHandle != null)
                {
                    TimeOutManager.CancelTimeOutEvent(_sendPingTimeOutHandle);
                }
                if (_waitPongTimeOutHandle != null)
                {
                    TimeOutManager.CancelTimeOutEvent(_waitPongTimeOutHandle);
                    _waitPongTimeOutHandle = null;
                }

                _sendPingTimeOutHandle = TimeOutManager.AddTimeOutEvent(
                        SendPingTimeOutValue,
                        OnSendPingTimeOutDelegate,
                        null);
            }
        }

        protected void SendRoundTripPing()
        {
            if (IsDisposed)
            {
                return;
            }

            _sendPingTimeOutHandle = null;
            _lastPingSentTime = DateTime.UtcNow;
            TcpSocketsUtils.Send(Socket, _s_roundTripPingMessage, _OnSend, OnExcp);
            if (_roundTripDelayWaitTimeoutInSec > 0)
            {
                _waitPongTimeOutHandle = TimeOutManager.AddTimeOutEvent(
                        _roundTripDelayWaitTimeoutInSec,
                        OnWaitPongTimeOutDelegate,
                        null);
            }
        }

        protected void SendRoundTripPong()
        {
            TcpSocketsUtils.Send(Socket, _s_roundTripPongMessage, _OnSend, OnExcp);
        }

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// PATCH: Any connector which wants to be announced and be closed upon time out
        /// in the keep alive mechanism should override this function and return true.
        /// </summary>
        /// <returns></returns>
        protected virtual bool AmISensitiveToKeepAliveTimeOut()
        {
            return false;
        }

        ///////////////////////////////////////////////////////////////////////

        public SocketConnector(Int32 id,
            IConnectorClient connectorClient,
            NetworkApiEndpointRegistration registration,
            INetworkEndpointLogger logger)
            : this(null, null, id, connectorClient, registration, logger)
        {
        }

        public SocketConnector(Int32 id,
            int receiveBufferSize,
            IConnectorClient connectorClient,
            NetworkApiEndpointRegistration registration,
            INetworkEndpointLogger logger)
            : this(null, null, id, receiveBufferSize, connectorClient, registration, logger)
        {
        }

        protected SocketConnector(
            SocketConnectorsManager cc,
            Socket s,
            Int32 id,
            IConnectorClient connectorClient,
            NetworkApiEndpointRegistration registration,
            INetworkEndpointLogger logger)
            : this(cc, s, id, TcpSocketsUtils.DefualtReceiveBufferSize, connectorClient, registration, logger)
        {
        }

        protected SocketConnector(
            SocketConnectorsManager cc,
            Socket s,
            Int32 id,
            int receiveBufferSize,
            IConnectorClient connectorClient,
            NetworkApiEndpointRegistration registration,
            INetworkEndpointLogger logger)
            : base(id, connectorClient, registration, logger)
        {
            Socket = s;
            ConnectorsManager = cc;
            if (ConnectorsManager != null)
            {
                ConnectorsManager.RegisterConnector(this);
            }

            _receiveBufferSize = receiveBufferSize;
        }

        public override byte[] GetRemoteIpAddress()
        {
            byte[] ipAddress = ((IPEndPoint)Socket.RemoteEndPoint).Address.GetAddressBytes();
            return ipAddress;
        }

        public override string GetRemoteIpAddressAsString()
        {
            if (Socket == null || Socket.RemoteEndPoint == null)
                return string.Empty;

            IPAddress addr = ((IPEndPoint)Socket.RemoteEndPoint).Address;
            if (addr == null)
                return string.Empty;
            return addr.ToString();
        }

        public int GetRemoteIpPort()
        {
            if (Socket == null || Socket.RemoteEndPoint == null)
                return 0;
            return ((IPEndPoint)Socket.RemoteEndPoint).Port;
        }

        public override void StartKeepAliveService()
        {
            if (TimeOutManager == null)
            {
                throw new Exception("No timeout manager had been set for keep-alive service");
            }
            _isKeepAliveRunning = true;
            ResetSendKeepAliveTimeOut();
            ResetReceiveKeepAliveTimeOut();
        }

        public override void StartRoundTripDelayCheck(RoundTripDelayInfo roundTripDelayInfoDelgate, int waitTimeoutInSec)
        {
            if (TimeOutManager == null)
            {
                throw new Exception("No timeout manager had been set for Round Trip Delay Check");
            }
            _roundTripDelayWaitTimeoutInSec = (uint)waitTimeoutInSec;
            _roundTripDelayInfoDelgate = roundTripDelayInfoDelgate;
            _isRoundTripDelayCheckRunning = true;
            SendRoundTripPing();
        }

        public override void StartRecv()
        {
            TcpSocketsUtils.Recv(Socket, OnRecv, OnExcp, _receiveBufferSize, true);
        }

        public override void Send(string strBuffer)
        {
            Send(Encoding.UTF8.GetBytes(strBuffer));
        }

        public void SetSocketNoDelay(bool noDelay)
        {
            Socket.NoDelay = noDelay;
        }

        public override void Send(byte[] buf)
        {
            //MyLogger.DebugFormat("AbstractConnector.Send() Called on {0}, m_Socket = {1}", m_id, m_socket);
            //todo: check if sends can be done while other send is proccessed 
            TcpSocketsUtils.Send(Socket, buf, _OnSend, OnExcp);
            ResetSendKeepAliveTimeOut();
        }

        private bool IsSpecialMessage(byte[] recvBuf, byte[] specialBuf)
        {
            if (recvBuf.Length != specialBuf.Length)
            {
                return false;
            }
            for (Int32 i = 0; i < specialBuf.Length; i++)
            {
                if (recvBuf[i] != specialBuf[i])
                {
                    return false;
                }
            }

            return true;
        }

        //return false indicates stop recv loop
        private bool OnRecv(byte[] buf)
        {
            if (buf == null)
            {
                return true;
            }

            try
            {
                _receiveKeepAliveTimeOutCounter = 0;   // reset counter
                ResetReceiveKeepAliveTimeOut();
                if (IsSpecialMessage(buf, _s_keepAliveMessage))
                {
                    //MyLogger.DebugFormat("Received KeepAlive on {0}", m_id);
                    return true;
                }
                if (IsSpecialMessage(buf, _s_roundTripPingMessage))
                {
                    SendRoundTripPong();
                    return true;
                }
                if (IsSpecialMessage(buf, _s_roundTripPongMessage))
                {
                    DateTime now = DateTime.UtcNow;
                    TimeSpan roundTripTime = now - _lastPingSentTime;

                    ResetRoundTripDelayCheck();

                    if (_roundTripDelayInfoDelgate != null)
                    {
                        _roundTripDelayInfoDelgate(roundTripTime);
                    }
                    return true;
                }
                if (IsSpecialMessage(buf, GracefulCloseMessage))
                {
                    //todo:
                    //mark sokect as close
                    IsOnGracefulCloseCalled = true;

                    //call DoOnGracefulClose();
                    if (m_MessagesDispatcher != null)
                    {
                        m_MessagesDispatcher.EnqueueTask(_DoOnGracefulClose, null);
                    }
                    else
                    {
                        DoOnGracefulClose();
                    }


                    //stop recv loop
                    return false;
                }
                if (m_MessagesDispatcher != null)
                {
                    m_MessagesDispatcher.EnqueueTask(DoOnRecv, buf);
                }
                else
                {
                    DoOnRecv(buf);
                }
            }
            catch (Exception ex)
            {
                // We should not arrive here
                Logger.ConnectorOnReceiveException(Registration.Address.ToString(), Registration.Contract.FullName, ex);
            }
            return true;
        }

        internal void OnExcp(Exception e)
        {
            //todo: return also after gracefull close or after exception
            lock (this)
            {
                if (IsDisposed || IsOnExcpCalled || IsOnGracefulCloseCalled)
                {
                    return;
                }
                IsOnExcpCalled = true;
            }

            if (m_MessagesDispatcher != null)
            {
                m_MessagesDispatcher.EnqueueTask(DoOnExcp, e);
            }
            else
            {
                DoOnExcp(e);
            }

        }

        //-------------------------------------
        protected void _OnSend()
        {
        }

        //-------------------------------------
        //for message dispatcher purpose
        private void _DoOnGracefulClose(object obj)
        {
            DoOnGracefulClose();
        }
        //-------------------------------------

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        // Methods used for initiate connection

        private AutoResetEvent m_WaitHandle;

        public void Connect(string addr, int port)
        {
            m_WaitHandle = new AutoResetEvent(false);
            Socket = TcpSocketsUtils.Connect(addr, port);
            //send unique id
            TcpSocketsUtils.Send(Socket, Id.ToString(), AccOnSendId, OnExcp);
            m_WaitHandle.WaitOne();
            m_WaitHandle.Close();
            m_WaitHandle = null;
        }

        private void AccOnSendId()
        {
            //need to read the server ID
            TcpSocketsUtils.Recv(Socket, this.AccOnRecvId, OnExcp, _receiveBufferSize, false);
            //TcpSocketsUtils.Recv(m_Socket, this.AccOnRecvId, OnExcp, m_ReceiveBufferSize, false);
        }

        private bool AccOnRecvId(byte[] buf)
        {
            //recv the server ID (and ignore it)
            m_WaitHandle.Set();
            return true;
        }

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        #region IDisposable Members

        protected void SendGracefullClose()
        {
            if (SendGracefulCloseCalled)
            {
                return;
            }
            SendGracefulCloseCalled = true;
            TcpSocketsUtils.Send(Socket, GracefulCloseMessage, _OnSend, OnExcp);
        }

        protected override void Dispose(bool disposing)
        {
            Logger.ConnectorDisposing(Registration.Address.ToString(), Registration.Contract.FullName, Id);
            base.Dispose(disposing);

            if (disposing)
            {
                if (_receiveKeepAliveTimeOutHandle != null)
                {
                    TimeOutManager.CancelTimeOutEvent(_receiveKeepAliveTimeOutHandle);
                    _receiveKeepAliveTimeOutHandle = null;
                }
                if (_sendKeepAliveTimeOutHandle != null)
                {
                    TimeOutManager.CancelTimeOutEvent(_sendKeepAliveTimeOutHandle);
                    _sendKeepAliveTimeOutHandle = null;
                }
                if (_sendPingTimeOutHandle != null)
                {
                    TimeOutManager.CancelTimeOutEvent(_sendPingTimeOutHandle);
                    _sendPingTimeOutHandle = null;
                }
                if (_waitPongTimeOutHandle != null)
                {
                    TimeOutManager.CancelTimeOutEvent(_waitPongTimeOutHandle);
                    _waitPongTimeOutHandle = null;
                }

                // Release unmanaged resources.
                if (Socket != null)
                {
                    if (IsOnExcpCalled == false && IsOnGracefulCloseCalled == false)
                    {
                        SendGracefullClose();
                    }
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Close();
                    Socket = null;
                }
            }
        }

        #endregion
    }
}
