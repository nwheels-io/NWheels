using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using NWheels.Endpoints;

namespace NWheels.Stacks.Network
{
    public class SocketConnectorsManager : AbstractNetConnectorsManager
    {
        //
        // when waiting for an incomming connection - whom to open the connection to:
        //
        public enum AddressListenMode
        {
            External,       // Only outside of the local host
            Internal,       // Only internally to the local host
            Any             // All incoming connections are accepted
        }

        //=====================================================================================
        internal class NewServerConnection
        {
            private readonly NetworkApiEndpointRegistration _registration;
            private readonly INetworkEndpointLogger _logger;
            private Int32 _newId;

            internal Socket Socket;
            internal SocketConnectorsManager ConnectorsManager;

            internal NewServerConnection(NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger)
            {
                _registration = registration;
                _logger = logger;
            }

            //bool - to match OnRecvDlgt 
            internal bool OnRecv(byte[] receivedBuffer)
            {

                string inString = Encoding.UTF8.GetString(receivedBuffer);

                _logger.NewConnectorOnReceive(_registration.Address.ToString(), _registration.Contract.FullName, inString);

                if (Int32.TryParse(inString, out _newId))
                {
                    if (_newId == 0) _newId = ConnectorsManager.GenerateNewConnectorId();
                    TcpSocketsUtils.Send(Socket, ConnectorsManager.Id.ToString(), this.OnSend, this.OnExcp);

                }
                else
                {
                    _logger.NewConnectorOnReceiveParseFailed(_registration.Address.ToString(), _registration.Contract.FullName, inString);
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Close();
                }
                return true;
            }

            internal void OnExcp(Exception e)
            {
                _logger.NewConnectorExceptionOccurred(_registration.Address.ToString(), _registration.Contract.FullName, e);
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }

            internal void OnSend()
            {
                _logger.NewConnectorOnSend(_registration.Address.ToString(), _registration.Contract.FullName);
                ConnectorsManager.OnNewConnector(Socket, _newId);
            }
        }
        //=====================================================================================
        internal class NewClientConnection
        {

            internal Socket Socket;
            internal SocketConnectorsManager ConnectorsManager;

            ///////////////////////////////////////////////////////////////////////
            // Buffer size
            ///////////////////////////////////////////////////////////////////////

            private readonly int _receiveBufferSize;
            private readonly NetworkApiEndpointRegistration _registration;
            private readonly INetworkEndpointLogger _logger;

            ///////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////

            internal NewClientConnection(int receiveBufferSize, NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger)
            {
                _receiveBufferSize = receiveBufferSize;
                _registration = registration;
                _logger = logger;
            }

            internal void OnSend()
            {
                _logger.NewConnectorOnSend(_registration.Address.ToString(), _registration.Contract.FullName);
                TcpSocketsUtils.Recv(Socket, this.OnRecv, this.OnExcp,_receiveBufferSize,false);
            }

            //bool - to match OnRecvDlgt 
            internal bool OnRecv(byte[] receivedBuffer)
            {
                string InString = Encoding.UTF8.GetString(receivedBuffer);

                _logger.NewConnectorOnReceive(_registration.Address.ToString(), _registration.Contract.FullName, InString);
                Int32 newId;
                if (Int32.TryParse(InString, out newId))
                {
                    ConnectorsManager.OnNewConnector(Socket, newId);
                }
                else
                {
                    _logger.NewConnectorOnReceiveParseFailed(_registration.Address.ToString(), _registration.Contract.FullName, InString);
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Close();
                }

                return true;
            }
            internal void OnExcp(Exception e)
            {
                _logger.NewConnectorExceptionOccurred(_registration.Address.ToString(), _registration.Contract.FullName, e);
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }
        }

        //=====================================================================================

        private Socket _listenerSock;
        private bool _stop;
        private int _port;


        ///////////////////////////////////////////////////////////////////////
        // Buffer size
        ///////////////////////////////////////////////////////////////////////
        
        private readonly int _receiveBufferSize;

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public SocketConnectorsManager(Int32 id, NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger)
            : this(id, TcpSocketsUtils.DefualtReceiveBufferSize, registration, logger)
        {
        }

        public SocketConnectorsManager(Int32 id, int receiveBufferSize, NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger)
            : this(id, AddressListenMode.External, receiveBufferSize, registration, logger)
        {
        }

        public SocketConnectorsManager(Int32 id, AddressListenMode listenMode, NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger)
            : this(id, listenMode, TcpSocketsUtils.DefualtReceiveBufferSize, registration, logger)
        {
        }

        public SocketConnectorsManager(Int32 id, AddressListenMode listenMode, int receiveBufferSize, NetworkApiEndpointRegistration registration, INetworkEndpointLogger logger)
            : base(id, registration, logger)
        {
            ListenMode = listenMode;
            _receiveBufferSize = receiveBufferSize;
        }

        internal protected void OnNewConnector(Socket s, Int32 id) { } //-=-=-= EliT: TODO implement

        //---------------------------------------------------
        //initial connection
        public void CreateConnector(string addr, int port)
        {
            Socket socket = TcpSocketsUtils.Connect(addr, port);
            //send unique id
            NewClientConnection newConnection = new NewClientConnection(_receiveBufferSize, Registration, Logger);
            newConnection.ConnectorsManager = this;
            newConnection.Socket = socket;

            TcpSocketsUtils.Send(socket, this.Id.ToString(), newConnection.OnSend, newConnection.OnExcp);
        }


        //---------------------------------------------------
        //Set the listen mode.
        public AddressListenMode ListenMode { set; get; }

        //---------------------------------------------------
        //use to listen to remote connections
        //if isBlocking is false, run Accept loop on thread
        public override void StartListening(int port, bool isBlockingAcceptLoop)
        {
            _stop = false;
            _port = port;

            base.StartListening(port,isBlockingAcceptLoop);

            // Make endpoint for the socket.
            IPAddress serverAdd;
            switch (ListenMode)
            {
                case AddressListenMode.Internal:
                {
                    serverAdd = IPAddress.Parse("127.0.0.1");
                    break;
                }
                case AddressListenMode.Any:
                {
                    serverAdd = IPAddress.Any;
                    break;
                }
                //case AddressListenMode.External:
                default:
                {
                    IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                    serverAdd = ipHost.AddressList[0];
                    break;
                }
            }

            IPEndPoint ep = new IPEndPoint(serverAdd, port);

            // Create a TCP/IP socket for listner.
            _listenerSock = new Socket(
                serverAdd.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Bind the socket to the endpoint and wait for listen for incoming connections.

            _listenerSock.Bind(ep);
            _listenerSock.Listen(10);

            if (isBlockingAcceptLoop)
            {
                StartListeningBlocking(port);
            }
            else
            {
                Thread t = new Thread(StartListeningThreadFunc);
                t.Start();
            }
        }

        public override void StopListening()
        {
            _stop = true;
            if (_listenerSock != null)
            {
                _listenerSock.Close();
                _listenerSock = null;
            }
            base.StopListening();
        }

        public byte[] GetRemoteIpAddress(Int32 connectorId)
        {
            AbstractNetConnector relevantConnection;
            if (Connectors.TryGetValue(connectorId, out relevantConnection))
            {
                return relevantConnection.GetRemoteIpAddress();
            }
            return null;
        }

        private void StartListeningThreadFunc()
        {
            try
            {
                StartListeningBlocking(_port);
            }
            catch (Exception e)
            {
                Logger.StartListeningFailed(Registration.Address.ToString(), Registration.Contract.FullName, e);
            }
        }

        private void StartListeningBlocking(int port)
        {
            while (!_stop)
            {
                try
                {
                    // Start an asynchronous socket to listen for connections.
                    Logger.WaitingAcceptOnPort(Registration.Address.ToString(), Registration.Contract.FullName);
                    Socket newSocket = _listenerSock.Accept();
                    NewServerConnection newConnection = new NewServerConnection(Registration, Logger);
                    newConnection.Socket = newSocket;
                    newConnection.ConnectorsManager = this;
                    TcpSocketsUtils.Recv(newSocket, newConnection.OnRecv, newConnection.OnExcp,_receiveBufferSize,false);
                }
                catch (Exception e)
                {
                    if (!_stop)
                    {
                        Logger.StartListeningFailed(Registration.Address.ToString(), Registration.Contract.FullName, e);
                    }
                }
            }
        }

        //---------------------------------------------------

    }
}
