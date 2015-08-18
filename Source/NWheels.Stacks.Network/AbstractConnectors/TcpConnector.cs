using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using NWheels.Endpoints;
using NWheels.Concurrency;

//AbstractConnector
namespace NWheels.Stacks.Network
{
    public class ConnectorDisconnectedException : Exception
    {
    }


    public class TcpConnector : AbstractNetConnector
    {
        protected Socket Socket;
        private readonly int _receiveBufferSize;
        private readonly IFramework _framework;

        //
        // Keep alive handling
        //
        private bool _isKeepAliveRunning;
        private ITimeoutHandle _sendKeepAliveTimeOutHandle;
        private ITimeoutHandle _receiveKeepAliveTimeOutHandle;
        private UInt16 _receiveKeepAliveTimeOutCounter;
        private const uint SendKeepAliveTimeOutValue = 20000;
        private const uint ReceiveKeepAliveTimeOutValue = 40000;
        private static readonly byte[] _s_keepAliveMessage = { 0x00, 0x00, 0x4E, 0x57, 0x4B, 0x41, 0x00, 0x00 };     // "\0 \0 N W K A \0 \0" NWheels Keep Alive

        //
        // Round trip delay check handling
        //
        private bool _isRoundTripDelayCheckRunning;
        private ITimeoutHandle _sendPingTimeOutHandle;
        private const uint SendPingTimeOutValue = 20000;
        private DateTime _lastPingSentTime;
        private static readonly byte[] _s_roundTripPingMessage = { 0x00, 0x00, 0x4E, 0x57, 0x50, 0x49, 0x00, 0x00 }; // "\0 \0 N W P I \0 \0" NWheels Ping for roundtrip delay check
        private static readonly byte[] _s_roundTripPongMessage = { 0x00, 0x00, 0x4E, 0x57, 0x50, 0x4F, 0x00, 0x00 }; // "\0 \0 N W P O \0 \0" NWheels Pong for roundtrip delay check
        private RoundTripDelayInfo _roundTripDelayInfoDelgate;
        private uint _roundTripDelayWaitTimeoutInSec;
        private ITimeoutHandle _waitPongTimeOutHandle;

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        private void OnSendKeepAliveTimeOutDelegate()
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
        private void OnReceiveKeepAliveTimeOutDelegate()
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
                _receiveKeepAliveTimeOutHandle.CancelTimer();
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
                Dispose();
            }
            if (doResetTimer)
            {
                ResetReceiveKeepAliveTimeOut();
            }
        }

        private void ResetSendKeepAliveTimeOut()
        {
            if ( _isKeepAliveRunning == false )
            {
                return;
            }

            lock (this)
            {
                if (IsDisposed)
                {
                    return;
                }
                if ( _sendKeepAliveTimeOutHandle != null )
                {
                    _sendKeepAliveTimeOutHandle.ResetDueTime(TimeSpan.FromMilliseconds(SendKeepAliveTimeOutValue));
                }
                else
                {
                    _sendKeepAliveTimeOutHandle = _framework.NewTimer(
                        "SendKeepAliveTimeOut",
                        Id.ToString(),
                        TimeSpan.FromMilliseconds(SendKeepAliveTimeOutValue),
                        OnSendKeepAliveTimeOutDelegate);
                }
            }
        }

        private void ResetReceiveKeepAliveTimeOut()
        {
            if (_isKeepAliveRunning == false )
            {
                return;
            }
            lock (this)
            {
                if (IsDisposed)
                {
                    return;
                }

                if ( _receiveKeepAliveTimeOutHandle != null )
                {
                    _receiveKeepAliveTimeOutHandle.ResetDueTime(TimeSpan.FromMilliseconds(ReceiveKeepAliveTimeOutValue));
                }
                else
                {
                    _receiveKeepAliveTimeOutHandle = _framework.NewTimer(
                        "ReceiveKeepAliveTimeOut",
                        Id.ToString(),
                        TimeSpan.FromMilliseconds(ReceiveKeepAliveTimeOutValue),
                        OnReceiveKeepAliveTimeOutDelegate);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        private void OnSendPingTimeOutDelegate()
        {
            SendRoundTripPing();
        }

        // Pong reply didn't arrive
        private void OnWaitPongTimeOutDelegate()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan roundTripTime = now - _lastPingSentTime;

            lock (this)
            {
                if (_waitPongTimeOutHandle != null)
                {
                    _waitPongTimeOutHandle.CancelTimer();
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
            if (_isRoundTripDelayCheckRunning == false )
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
                    _sendPingTimeOutHandle.CancelTimer();
                }
                if (_waitPongTimeOutHandle != null)
                {
                    _waitPongTimeOutHandle.CancelTimer();
                    _waitPongTimeOutHandle = null;
                }

                _sendPingTimeOutHandle = _framework.NewTimer(
                    "SendPingTimeOut",
                    Id.ToString(),
                    TimeSpan.FromMilliseconds(SendPingTimeOutValue),
                    OnSendPingTimeOutDelegate);
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
                _waitPongTimeOutHandle = _framework.NewTimer(
                    "WaitPongTimeOut",
                    Id.ToString(),
                    TimeSpan.FromSeconds(_roundTripDelayWaitTimeoutInSec),
                    OnWaitPongTimeOutDelegate);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal TcpConnector(
            TcpConnectorsManager cc,
            Socket s,
            Int32 id,
            int receiveBufferSize,
            IConnectorClient connectorClient,
            NetworkApiEndpointRegistration registration,
            IFramework framework,
            INetworkEndpointLogger logger)
            : base(id, connectorClient, registration, logger)
        {
            Socket = s;
            ConnectorsManager = cc;
            if (ConnectorsManager != null)
            {
                ConnectorsManager.RegisterConnector(this);
            }

            _framework = framework;
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
            _isKeepAliveRunning = true;
            ResetSendKeepAliveTimeOut();
            ResetReceiveKeepAliveTimeOut();
        }

        public override void StartRoundTripDelayCheck(RoundTripDelayInfo roundTripDelayInfoDelgate, int waitTimeoutInSec)
        {
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void _OnSend()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        //for message dispatcher purpose
        private void _DoOnGracefulClose(object obj)
        {
            DoOnGracefulClose();
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        // Methods used for initiate connection

        //private AutoResetEvent _waitHandle;

        public void Connect(string addr, int port)
        {
            //_waitHandle = new AutoResetEvent(false);
            Socket = TcpSocketsUtils.Connect(addr, port);
            //send unique id
            //TcpSocketsUtils.Send(Socket, Id.ToString(), AccOnSendId, OnExcp);
            //_waitHandle.WaitOne();
            //_waitHandle.Close();
            //_waitHandle = null;
        }

        //private void AccOnSendId()
        //{
        //    //need to read the server ID
        //    TcpSocketsUtils.Recv(Socket, AccOnRecvId, OnExcp, _receiveBufferSize, false);
        //}

        //private bool AccOnRecvId(byte[] buf)
        //{
        //    //recv the server ID (and ignore it)
        //    _waitHandle.Set();
        //    return true;
        //}

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
                    _receiveKeepAliveTimeOutHandle.CancelTimer();
                    _receiveKeepAliveTimeOutHandle = null;
                }
                if (_sendKeepAliveTimeOutHandle != null)
                {
                    _sendKeepAliveTimeOutHandle.CancelTimer();
                    _sendKeepAliveTimeOutHandle = null;
                }
                if (_sendPingTimeOutHandle != null)
                {
                    _sendPingTimeOutHandle.CancelTimer();
                    _sendPingTimeOutHandle = null;
                }
                if (_waitPongTimeOutHandle != null)
                {
                    _waitPongTimeOutHandle.CancelTimer();
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
