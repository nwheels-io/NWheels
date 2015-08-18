using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Autofac;
using NWheels.Endpoints;

namespace NWheels.Stacks.Network
{
    public class TcpConnectorsManager : AbstractNetConnectorsManager
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

        private readonly IFramework _framework;
        private readonly IComponentContext _components;
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

        public TcpConnectorsManager(
            Int32 id, 
            AddressListenMode listenMode, 
            int receiveBufferSize, 
            NetworkApiEndpointRegistration registration, 
            IComponentContext components,
            INetworkEndpointLogger logger, IFramework framework)
            : base(id, registration, logger)
        {
            _components = components;
            _framework = framework;
            ListenMode = listenMode;
            _receiveBufferSize = receiveBufferSize;
        }

        internal protected void OnNewConnector(Socket socket)
        {
            TcpConnector newConnector = new TcpConnector(this, socket, 0, _receiveBufferSize, null, this.Registration, _framework, Logger);
            newConnector.StartRecv();
        } 

        //---------------------------------------------------
        //initial connection
        public void CreateConnector(string addr, int port)
        {
            Socket socket = TcpSocketsUtils.Connect(addr, port);
            OnNewConnector(socket);

            //send unique id
            //NewClientConnection newConnection = new NewClientConnection(_receiveBufferSize, Registration, Logger);
            //newConnection.ConnectorsManager = this;
            //newConnection.Socket = socket;

            //TcpSocketsUtils.Send(socket, this.Id.ToString(), newConnection.OnSend, newConnection.OnExcp);
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
                    //NewServerConnection newConnection = new NewServerConnection(Registration, Logger);
                    //newConnection.Socket = newSocket;
                    //newConnection.ConnectorsManager = this;
                    OnNewConnector(newSocket);
                    //TcpSocketsUtils.Recv(newSocket, newConnection.OnRecv, newConnection.OnExcp,_receiveBufferSize,false);
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
