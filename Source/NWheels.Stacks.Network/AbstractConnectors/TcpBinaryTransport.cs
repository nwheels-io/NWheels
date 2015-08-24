using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Autofac;
using NWheels.Endpoints;
using NWheels.Stacks.Network;
using NWheels.Processing.Messages;

namespace NWheels.Stacks.Network
{
    public class TcpBinaryTransport : NetConnectorsBinaryTransport
    {
        private readonly IFramework _framework;
        private readonly IComponentContext _components;
        private readonly IServiceBus _serviceBus;
        private Socket _listenerSocket;
        private bool _stop;

        private ITcpBinaryTransportConfiguration TcpConfiguration { get { return (ITcpBinaryTransportConfiguration)MyConfiguration; } }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TcpBinaryTransport(
            IAbstractNetwrokTransportConfig configuration,
            NetworkApiEndpointRegistration registration, 
            IComponentContext components,
            INetworkEndpointLogger logger,
            IFramework framework,
            IServiceBus serviceBus)
            : base(configuration, registration, logger)
        {
            _serviceBus = serviceBus;
            _components = components;
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //initiate a connection
        public void CreateConnector(string remoteAddr, int port)
        {
            Socket socket = TcpSocketsUtils.Connect(remoteAddr, port);
            OnNewConnector(socket);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //use to listen to remote connections
        //if isBlocking is false, run Accept loop on thread
        public override void StartListening()
        {
            _stop = false;
            base.StartListening();

            if (TcpConfiguration.ListenMode == AddressListenMode.None)
            {
                return;
            }

            // Make endpoint for the socket.
            IPAddress serverAdd;
            switch (TcpConfiguration.ListenMode)
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

            IPEndPoint ep = new IPEndPoint(serverAdd, TcpConfiguration.IncomingPort);

            // Create a TCP/IP socket for listner.
            _listenerSocket = new Socket(
                serverAdd.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Bind the socket to the endpoint and wait for listen for incoming connections.

            _listenerSocket.Bind(ep);
            _listenerSocket.Listen(10);

            Thread t = new Thread(ListenerThreadFunc);
            t.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void StopListening()
        {
            _stop = true;
            if (_listenerSocket != null)
            {
                _listenerSocket.Close();
                _listenerSocket = null;
            }
            base.StopListening();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public byte[] GetRemoteIpAddress(Int32 connectorId)
        {
            AbstractNetBinaryConnector connector;
            if (Connectors.TryGetValue(connectorId, out connector))
            {
                return connector.GetRemoteIpAddress();
            }
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnNewConnector(Socket socket)
        {
            TcpConnector newConnector = new TcpConnector(this, socket, TcpConfiguration.ReceiveBufferSize, null, this.Registration, _framework, Logger, _serviceBus);
            newConnector.StartRecv();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ListenerThreadFunc()
        {
            while (!_stop)
            {
                try
                {
                    // Start an asynchronous socket to listen for connections.
                    Logger.WaitingAcceptOnPort(Registration.Address.ToString(), Registration.Contract.FullName);
                    Socket newSocket = _listenerSocket.Accept();
                    OnNewConnector(newSocket);
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
    }
}
