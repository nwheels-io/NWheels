using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NWheels.Endpoints.Factories;
using NWheels.Logging;

namespace NWheels.Endpoints
{
    public class DuplexTcpTransport
    {
        public class ApiFactory
        {
            private readonly IComponentContext _components;
            private readonly IDuplexNetworkApiProxyFactory _proxyFactory;
            private readonly Logger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ApiFactory(IComponentContext components, IDuplexNetworkApiProxyFactory proxyFactory, Logger logger)
            {
                _components = components;
                _proxyFactory = proxyFactory;
                _logger = logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IDuplexNetworkApiEndpoint<TServerApi, TClientApi> CreateApiServer<TServerApi, TClientApi>(
                string listenIpAddress,
                int listenPortNumber,
                int listenBacklog = 1000,
                int maxPendingConnections = 1000,
                int maxConcurrentConnections = 10000,
                TimeSpan? clientToServerHeartbeatInterval = null,
                TimeSpan? serverToClientHeartbeatInterval = null) where TServerApi : class where TClientApi : class
            {
                if (listenPortNumber == 0)
                {
                    listenPortNumber = new Random().Next(5000, 55000);
                }

                var server = new Server(
                    _logger,
                    listenIpAddress,
                    listenPortNumber,
                    listenBacklog,
                    maxPendingConnections,
                    maxConcurrentConnections,
                    clientToServerHeartbeatInterval,
                    serverToClientHeartbeatInterval);

                server.Start();

                return new ServerApiEndpoint<TServerApi, TClientApi>(server, this);
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public TServerApi CreateApiClient<TServerApi, TClientApi>(
                TClientApi localObject,
                string serverHost,
                int serverPort,
                TimeSpan? clientToServerHeartbeatInterval = null,
                TimeSpan? serverToClientHeartbeatInterval = null) 
                where TServerApi : class 
                where TClientApi : class
            {
                var client = new Client(_logger, serverHost, serverPort);
                var apiConnection = ApiConnection<TClientApi, TServerApi>.CreateOnClient(this, client.Connection, localObject);
                
                client.Start();
                
                return apiConnection.RemoteProxy;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal IComponentContext Components
            {
                get { return _components; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal IDuplexNetworkApiProxyFactory ProxyFactory
            {
                get { return _proxyFactory; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void SafeCloseSocket(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                socket.Close();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ServerApiEndpoint<TServerApi, TClientApi> : IDuplexNetworkApiEndpoint<TServerApi, TClientApi>
            where TServerApi : class
            where TClientApi : class
        {
            private readonly Server _server;
            private readonly ApiFactory _factory;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ServerApiEndpoint(Server server, ApiFactory factory)
            {
                _server = server;
                _factory = factory;

                AttachServerEventHandlers();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _server.Dispose();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Broadcast(Action<TClientApi> actionPerClient)
            {
                _server.Broadcast(connection => {
                    var apiConnection = ApiConnection<TServerApi, TClientApi>.From(connection);
                    actionPerClient(apiConnection.RemoteProxy);
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int ActiveClientCount 
            {
                get { return _server.ActiveConnectionCount; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void AttachServerEventHandlers()
            {
                _server.ClientConnected += (connection) => {
                    ApiConnection<TServerApi, TClientApi>.CreateOnServer(_factory, this, connection);
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ApiConnection<TLocalApi, TRemoteApi> : IDuplexNetworkEndpointTransport
            where TLocalApi : class
            where TRemoteApi : class
        {
            private readonly ServerApiEndpoint<TLocalApi, TRemoteApi> _server;
            private readonly Connection _connection;
            private readonly object _localObject;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ApiConnection(ServerApiEndpoint<TLocalApi, TRemoteApi> server, Connection connection, object localObject)
            {
                _server = server;
                _connection = connection;
                _localObject = localObject;

                AttachConnectionEventHandlers();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SendBytes(byte[] bytes)
            {
                _connection.SendMessage(bytes);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TRemoteApi RemoteProxy { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<byte[]> BytesReceived;
            public event Action<Exception> SendFailed;
            public event Action<Exception> ReceiveFailed;

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void AttachConnectionEventHandlers()
            {
                _connection.MessageReceived += (connection, messageBytes) => {
                    if (BytesReceived != null)
                    {
                        BytesReceived(messageBytes);
                    }
                };
                
                _connection.SendFailed += (connection, error) => {
                    if (SendFailed != null)
                    {
                        SendFailed(error);
                    }
                };

                _connection.ReceiveFailed += (connection, error) => {
                    if (ReceiveFailed != null)
                    {
                        ReceiveFailed(error);
                    }
                };

                _connection.Closed += (connection, reason) => {
                    var apiService = _localObject as IDuplexNetworkApiTarget<TLocalApi, TRemoteApi>;
                    if (apiService != null)
                    {
                        apiService.OnDisconnected(_server, RemoteProxy, reason);
                    }
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static ApiConnection<TLocalApi, TRemoteApi> CreateOnServer(
                ApiFactory factory,
                ServerApiEndpoint<TLocalApi, TRemoteApi> endpoint, 
                Connection connection)
            {
                return Create(factory, connection, localObject: null, endpoint: endpoint);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static ApiConnection<TLocalApi, TRemoteApi> CreateOnClient(
                ApiFactory factory,
                Connection connection, 
                TLocalApi localObject)
            {
                return Create(factory, connection, localObject, endpoint: null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static ApiConnection<TLocalApi, TRemoteApi> From(Connection connection)
            {
                return (ApiConnection<TLocalApi, TRemoteApi>)connection.UpperLayerTag;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static ApiConnection<TLocalApi, TRemoteApi> Create(
                ApiFactory factory,
                Connection connection,
                TLocalApi localObject,
                ServerApiEndpoint<TLocalApi, TRemoteApi> endpoint)
            {
                var effectiveLocalObject = (localObject ?? factory.Components.Resolve<TLocalApi>());
                
                var apiConnection = new ApiConnection<TLocalApi, TRemoteApi>(endpoint, connection, effectiveLocalObject);
                connection.UpperLayerTag = apiConnection;

                var remoteProxy = factory.ProxyFactory.CreateProxyInstance<TRemoteApi, TLocalApi>(apiConnection, effectiveLocalObject);
                apiConnection.RemoteProxy = remoteProxy;

                var apiService = effectiveLocalObject as IDuplexNetworkApiTarget<TLocalApi, TRemoteApi>;
                if (apiService != null)
                {
                    apiService.OnConnected(endpoint, remoteProxy);
                }

                return apiConnection;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Server
        {
            private readonly Logger _logger;
            private readonly string _ip;
            private readonly int _port;
            private readonly int _listenBacklog;
            private readonly int _maxPendingConnections;
            private readonly int _maxConcurrentConnections;
            private readonly TcpListener _tcpListener;
            private readonly CancellationTokenSource _cancellation;
            private readonly Hashtable _connectionTaskByConnection;
            private readonly BlockingCollection<object> _connectionManagerQueue;
            private readonly Task _connectionManagerTask;
            private readonly Task _acceptConnectionsTask;
            private readonly Action<Connection, ConnectionCloseReason> _onConnectionClosedDelegate;
            private int _pendingConnectionCount;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Server(
                Logger logger,
                string ip,
                int port,
                int listenBacklog = 1000,
                int maxPendingConnections = 1000,
                int maxConcurrentConnections = 10000, 
                TimeSpan? clientHeartbeatInterval = null,
                TimeSpan? serverPingInterval = null,
                Action<Connection> onClientConnected = null,
                Action<Connection, ConnectionCloseReason> onClientDisconnected = null)
            {
                _logger = logger;
                _ip = ip;
                _port = port;
                _listenBacklog = listenBacklog;
                _maxPendingConnections = maxPendingConnections;
                _maxConcurrentConnections = maxConcurrentConnections;
                _cancellation = new CancellationTokenSource();
                _connectionTaskByConnection = new Hashtable();
                _connectionManagerQueue = new BlockingCollection<object>();
                _onConnectionClosedDelegate = this.OnConnectionClosed;
                _pendingConnectionCount = 0;

                if (onClientConnected != null)
                {
                    ClientConnected += onClientConnected;
                }

                if (onClientDisconnected != null)
                {
                    ClientDisconnected += onClientDisconnected;
                }

                _logger.ServerInitializing(ip, port);

                _tcpListener = new TcpListener(IPAddress.Parse(_ip ?? "127.0.0.1"), _port);
                _connectionManagerTask = Task.Factory.StartNew(ManageConnections, _cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                _acceptConnectionsTask = AcceptConnections();

                _logger.ServerCtorCompleted(ip, port);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Start()
            {
                _tcpListener.Start(_listenBacklog);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Broadcast(Action<Connection> actionPerConnection)
            {
                var activeConnections = _connectionTaskByConnection.Keys.Cast<Connection>().ToArray();

                for (int i = 0 ; i < activeConnections.Length ; i++)
                {
                    try
                    {
                        actionPerConnection(activeConnections[i]);
                    }
                    catch (Exception e)
                    {
                        _logger.ServerBroadcastFailedToConnection(_ip, _port, error: e);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _logger.ServerShuttingDown(_ip, _port);
                _cancellation.Cancel();

                try
                {
                    _logger.ServerShutdownClosingTcpListener();
                    _tcpListener.Stop();
                }
                catch (Exception e)
                {
                    _logger.ServerShutdownCloseTcpListenerFailure(_ip, _port, e);
                }

                _logger.ServerShutdownWaitingForThreadsToStop();

                var acceptConnectionsStopped = _acceptConnectionsTask.Wait(TimeSpan.FromSeconds(10));
                _logger.ServerShutdownAcceptConnectionsStopped(success: acceptConnectionsStopped);

                var connectionManagerStopped = _connectionManagerTask.Wait(TimeSpan.FromSeconds(5));
                _logger.ServerShutdownConnectionManagerStopped(success: connectionManagerStopped);

                var allConnectionTasks = _connectionTaskByConnection.Values.Cast<Task>().ToArray();
                var allConnectionTasksStopped = Task.WaitAll(allConnectionTasks, TimeSpan.FromSeconds(15));
                _logger.ServerShutdownAllConnectionTasksStopped(success: allConnectionTasksStopped);

                if (!acceptConnectionsStopped || !connectionManagerStopped || !allConnectionTasksStopped)
                {
                    _logger.ServerShutdownSomeThreadsDidNotProperlyStop(_ip, _port, acceptConnectionsStopped, connectionManagerStopped, allConnectionTasksStopped);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int PendingConnectionCount
            {
                get { return _pendingConnectionCount; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int ActiveConnectionCount
            {
                get { return _connectionTaskByConnection.Count; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<Connection> ClientConnected;
            public event Action<Connection, ConnectionCloseReason> ClientDisconnected;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private async Task AcceptConnections()
            {
                await Task.Delay(10); // ensure migration from constructor's thread to thread pool

                _logger.ServerReadyToAcceptTcpConnections(_ip, _port);

                while (!_cancellation.IsCancellationRequested)
                {
                    Socket socket;

                    try
                    {
                        socket = await _tcpListener.AcceptSocketAsync();
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }

                    if (Interlocked.Increment(ref _pendingConnectionCount) <= _maxPendingConnections)
                    {
                        _logger.ServerTcpConnectionAccepted(socket.Handle);
                        _connectionManagerQueue.Add(socket);
                    }
                    else
                    {
                        // too busy
                        Interlocked.Decrement(ref _pendingConnectionCount);
                        SafeCloseSocket(socket);
                        _logger.ServerTcpConnectionDeclinedTooBusy(_ip, _port, _pendingConnectionCount, _connectionTaskByConnection.Count);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ManageConnections()
            {
                var cancellationToken = _cancellation.Token;

                _logger.ServerConnectionManagerStarted();

                while (!cancellationToken.IsCancellationRequested)
                {
                    object request;

                    try
                    {
                        request = _connectionManagerQueue.Take(cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }

                    HandleConnectionManagerRequest(request);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void HandleConnectionManagerRequest(object request)
            {
                var openConnectionRequest = request as Socket;
                var closeConnectionRequest = request as Connection;

                if (openConnectionRequest != null)
                {
                    Interlocked.Decrement(ref _pendingConnectionCount);

                    _logger.ServerConnectionManagerGotOpenRequest(socketHandle: openConnectionRequest.Handle);

                    var connection = new Connection(socket: openConnectionRequest, upstreamCancellation: _cancellation.Token, logger: _logger);
                    connection.Closed += _onConnectionClosedDelegate;

                    if (ClientConnected != null)
                    {
                        _logger.ServerConnectionManagerFiringClientConnectedEvent();
                        ClientConnected(connection);
                    }

                    var connectionTask = connection.BeginReceiveMessages();
                    _connectionTaskByConnection.Add(connection, connectionTask);
                }
                else if (closeConnectionRequest != null)
                {
                    _logger.ServerConnectionManagerGotCloseRequest(socketHandle: closeConnectionRequest.SocketHandle, closeReason: closeConnectionRequest.CloseReason);
                    _connectionTaskByConnection.Remove(closeConnectionRequest);
                }
                else
                {
                    _logger.ServerConnectionManagerGotBadRequest();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void OnConnectionClosed(Connection connection, ConnectionCloseReason reason)
            {
                _logger.ServerConnectionClosed(socketHandler: connection.SocketHandle, closeReason: reason);

                if (ClientDisconnected != null)
                {
                    _logger.ServerConnectionClosedFiringClientDisconnected();
                    ClientDisconnected(connection, reason);
                }

                _connectionManagerQueue.Add(connection);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Client
        {
            private readonly Logger _logger;
            private readonly TcpClient _tcpClient;
            private readonly NetworkStream _stream;
            private readonly Connection _connection;
            private readonly CancellationTokenSource _cancellation;
            private Task _receiveTask;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Client(
                Logger logger,
                string serverHost,
                int serverPort,
                Action<byte[]> onMessageReceived = null,
                Action<ConnectionCloseReason> onDisconnected = null)
            {
                _logger = logger;
                _cancellation = new CancellationTokenSource();
                _tcpClient = new TcpClient();

                var effectiveServerHost = (serverHost ?? "localhost");

                _logger.ClientConnectingToServer(host: effectiveServerHost, port: serverPort);

                _tcpClient.Connect(effectiveServerHost, serverPort);
                _stream = _tcpClient.GetStream();

                _logger.ClientSuccessfullyConnectedToServer();

                _connection = new Connection(_tcpClient.Client, _tcpClient.GetStream(), _cancellation.Token, isClientConnection: true, logger: _logger);

                AttachConnectionEventHandlers(onMessageReceived, onDisconnected);

                _logger.ClientCtorCompleted();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Start()
            {
                if (_receiveTask == null)
                {
                    _receiveTask = _connection.BeginReceiveMessages();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _logger.ClientDisposeStoppingConnection();

                _cancellation.Cancel();
                
                if (_receiveTask != null)
                {
                    _receiveTask.Wait(TimeSpan.FromSeconds(10));
                }

                _logger.ClientDisposeClosingSocket();

                _stream.Dispose();
                _tcpClient.Close();

                _logger.ClientDisposeAllResourcesReleased();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SendMessage(byte[] message)
            {
                _logger.ClientSendMessage(length: message.Length);

                try
                {
                    _connection.SendMessage(message);
                }
                catch (Exception e)
                {
                    _logger.ClientSendMessageFailed(error: e);

                    if (ReceiveFailed != null)
                    {
                        ReceiveFailed(e);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<byte[]> MessageReceived;
            public event Action<ConnectionCloseReason> Disconnected;
            public event Action<Exception> SendFailed;
            public event Action<Exception> ReceiveFailed;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal Connection Connection
            {
                get { return _connection; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AttachConnectionEventHandlers(Action<byte[]> onMessageReceived, Action<ConnectionCloseReason> onDisconnected)
            {
                _connection.MessageReceived += (connection, message) => {
                    if (MessageReceived != null)
                    {
                        MessageReceived(message);
                    }
                };

                _connection.Closed += (connection, reason) => {
                    if (Disconnected != null)
                    {
                        Disconnected(reason);
                    }
                };

                _connection.SendFailed += (connection, error) => {
                    if (SendFailed != null)
                    {
                        SendFailed(error);
                    }
                };

                _connection.ReceiveFailed += (connection, error) => {
                    if (ReceiveFailed != null)
                    {
                        ReceiveFailed(error);
                    }
                };

                if (onMessageReceived != null)
                {
                    _connection.MessageReceived += (connection, message) => {
                        onMessageReceived(message);
                    };
                }

                if (onDisconnected != null)
                {
                    _connection.Closed += (connection, reason) => {
                        onDisconnected(reason);
                    };
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Connection
        {
            private readonly Socket _socket;
            private readonly NetworkStream _stream;
            private readonly CancellationToken _upstreamCancellation;
            private readonly Logger _logger;
            private readonly CancellationTokenSource _connectionCancellation;
            private readonly CancellationToken _anyReasonCancellation;
            private readonly SocketAddress _remoteAddress;
            private readonly string _logPartyName;
            private Task _receiveTask;
            private ConnectionCloseReason _closeReason;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Connection(Socket socket, CancellationToken upstreamCancellation, Logger logger)
                : this(socket, new NetworkStream(socket, ownsSocket: true), upstreamCancellation, logger)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Connection(Socket socket, NetworkStream stream, CancellationToken upstreamCancellation, Logger logger, bool isClientConnection = false)
            {
                _socket = socket;
                _stream = stream;
                _upstreamCancellation = upstreamCancellation;
                _logger = logger;
                _connectionCancellation = new CancellationTokenSource();
                _anyReasonCancellation = CancellationTokenSource.CreateLinkedTokenSource(_upstreamCancellation, _connectionCancellation.Token).Token;
                _remoteAddress = socket.RemoteEndPoint.Serialize();
                _closeReason = ConnectionCloseReason.Unknown;
                _logPartyName = (isClientConnection ? "CLIENT" : "SERVER");

                _logger.ConnectionInitialized(_logPartyName, _remoteAddress.ToString());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task BeginReceiveMessages()
            {
                if (_receiveTask != null)
                {
                    throw _logger.ConnectionAlreadyReceivingMessages(_logPartyName);
                }

                _receiveTask = ReceiveMessages();

                _logger.ConnectionStartedReceiveMessagesTask(_logPartyName);

                return _receiveTask;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SendMessage(byte[] message)
            {
                try
                {
                    Int32 length = (message != null ? message.Length : 0);

                    _logger.ConnectionSendMessage(_logPartyName, length);

                    _stream.Write(BitConverter.GetBytes(length), 0, sizeof(Int32));

                    _logger.ConnectionSendMessageHeaderWritten(_logPartyName);

                    if (message != null)
                    {
                        _stream.Write(message, 0, message.Length);
                        _logger.ConnectionSendMessageBodyWritten(_logPartyName);
                    }

                    _stream.Flush();

                    _logger.ConnectionSendMessageStreamFlushedSent(_logPartyName);
                }
                catch (Exception e)
                {
                    _logger.ConnectionSendingMessageError(_logPartyName, e);

                    if (SendFailed != null)
                    {
                        _logger.ConnectionSendingMessageFiringSendFailedEvent(_logPartyName);
                        SendFailed(this, e);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SocketAddress RemoteAddress
            {
                get { return _remoteAddress; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ConnectionCloseReason CloseReason
            {
                get { return _closeReason; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object UpperLayerTag { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<Connection, byte[]> MessageReceived;
            public event Action<Connection, Exception> SendFailed;
            public event Action<Connection, Exception> ReceiveFailed;
            public event Action<Connection, ConnectionCloseReason> Closed;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal IntPtr SocketHandle
            {
                get { return _socket.Handle; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private async Task ReceiveMessages()
            {
                _logger.ConnectionReceiveMessagesStarted(_logPartyName, _remoteAddress.ToString());

                try
                {
                    byte[] headerBuffer = new byte[sizeof(Int32)];

                    while (!_anyReasonCancellation.IsCancellationRequested)
                    {
                        try
                        {
                            _logger.ConnectionReceiveMessagesWaitingForHeader(_logPartyName);
                            await ReceiveOneMessage(headerBuffer);
                        }
                        catch (TaskCanceledException)
                        {
                            _logger.ConnectionReceiveMessagesTaskCanceledException(_logPartyName);
                            _closeReason = ConnectionCloseReason.LocalPartyShutDown;
                            break;
                        }
                        catch (ObjectDisposedException)
                        {
                            _logger.ConnectionReceiveMessagesObjectDisposedException(_logPartyName);
                            _closeReason = ConnectionCloseReason.LocalPartyShutDown;
                            break;
                        }
                        catch (Exception e)
                        {
                            _logger.ConnectionReceiveMessagesException(_logPartyName, e);

                            if (ReceiveFailed != null)
                            {
                                _logger.ConnectionReceiveMessagesFiringReceiveFailed(_logPartyName);
                                ReceiveFailed(this, e);
                            }
                        }
                    }
                }
                finally
                {
                    FinalizeAndCleanup();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private async Task ReceiveOneMessage(byte[] headerBuffer)
            {
                _logger.ConnectionReceiveOneMessagesWaitingForHeader(_logPartyName);

                if (!await ReceiveBytes(headerBuffer, 0, headerBuffer.Length)) //TODO: include timeout
                {
                    _logger.ConnectionReceiveOneMessagesHeaderWasNotReceived(_logPartyName);
                    return;
                }

                var bodyLength = BitConverter.ToInt32(headerBuffer, startIndex: 0);
                var bodyBuffer = new byte[bodyLength];

                if (!await ReceiveBytes(bodyBuffer, 0, bodyBuffer.Length)) //TODO: include timeout
                {
                    _logger.ConnectionReceiveOneMessagesBodyWasNotReceived(_logPartyName);
                    return;
                }

                _logger.ConnectionReceiveOneMessagesSuccess(_logPartyName, bodyBuffer.Length);

                if (MessageReceived != null)
                {
                    _logger.ConnectionReceiveOneMessagesFiringMessageReceived(_logPartyName);
                    MessageReceived(this, bodyBuffer);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private async Task<bool> ReceiveBytes(byte[] buffer, int offset, int count) //TODO: include timeout
            {
                try
                {
                    var totalBytesReceived = 0;

                    while (totalBytesReceived < count)
                    {
                        _logger.ConnectionReceiveBytesWaiting(_logPartyName, bytesToReceive: count - totalBytesReceived);

                        var chunkBytesReceived =
                            await _stream.ReadAsync(buffer, offset + totalBytesReceived, count - totalBytesReceived, _anyReasonCancellation);

                        _logger.ConnectionReceiveBytesReceived(_logPartyName, numberOfBytes: chunkBytesReceived);

                        if (chunkBytesReceived <= 0)
                        {
                            _logger.ConnectionReceiveBytesRemotePartyNotReachableClosingConnection(_logPartyName);

                            _closeReason = ConnectionCloseReason.RemotePartyNotReachable;
                            _connectionCancellation.Cancel();
                            return false;
                        }

                        totalBytesReceived += chunkBytesReceived;
                    }

                    _logger.ConnectionReceiveBytesSuccess(_logPartyName, bytesReceived: totalBytesReceived);
                    return true;
                }
                catch (Exception e)
                {
                    _logger.ConnectionReceiveBytesFailed(_logPartyName, error: e);

                    if (ReceiveFailed != null)
                    {
                        _logger.ConnectionReceiveBytesFiringReceiveFailed(_logPartyName);
                        ReceiveFailed(this, e);
                    }
                }

                _closeReason = ConnectionCloseReason.RemotePartyNotReachable;
                _connectionCancellation.Cancel();
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FinalizeAndCleanup()
            {
                _logger.ConnectionFinalizing(_logPartyName, _closeReason);

                try
                {
                    if (_upstreamCancellation.IsCancellationRequested)
                    {
                        _logger.ConnectionFinalizeSettingCloseReasonToLocalPartyShotDown(_logPartyName);
                        _closeReason = ConnectionCloseReason.LocalPartyShutDown;
                    }

                    if (Closed != null)
                    {
                        _logger.ConnectionFinalizeFiringClosedEvent(_logPartyName);
                        Closed(this, _closeReason);
                    }
                }
                finally
                {
                    try
                    {
                        _logger.ConnectionFinalizeClosingSocket(_logPartyName);
                        _stream.Close();
                        //necessary? SafeCloseSocket(_socket);
                    }
                    catch (Exception e)
                    {
                        _logger.ConnectionFinalizeFailure(_logPartyName, error: e);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class Logger : IApplicationEventLogger
        {
            [LogInfo(ToPlainLog = true)]
            public abstract void ServerReadyToAcceptTcpConnections(string ip, int port);

            [LogWarning(ToPlainLog = true)] //TODO: include circuit breaker
            public abstract void ServerTcpConnectionDeclinedTooBusy(string ip, int port, int pendingConnectionCount, int activeConnectionCount);

            [LogInfo(ToPlainLog = true)]
            public abstract void ServerShuttingDown(string ip, int port);

            [LogWarning(ToPlainLog = true)]
            public abstract void ServerShutdownSomeThreadsDidNotProperlyStop(
                string ip, 
                int port, 
                bool acceptConnectionsStopped, 
                bool connectionManagerStopped, 
                bool allConnectionTasksStopped);

            [LogVerbose(ToPlainLog = true)]
            public abstract void ConnectionInitialized(string party, string remoteAddress);

            [LogError(ToPlainLog = true)]
            public abstract InvalidOperationException ConnectionAlreadyReceivingMessages(string party);

            [LogError(ToPlainLog = true)]
            public abstract void ConnectionReceiveMessagesException(string party, Exception exception);

            [LogWarning(ToPlainLog = true)]
            public abstract void ConnectionReceiveOneMessagesHeaderWasNotReceived(string party);

            [LogWarning(ToPlainLog = true)]
            public abstract void ConnectionReceiveOneMessagesBodyWasNotReceived(string party);

            [LogVerbose(ToPlainLog = true)]
            public abstract void ConnectionFinalizing(string party, ConnectionCloseReason closeReason);

            [LogError(ToPlainLog = true)]
            public abstract void ConnectionFinalizeFailure(string party, Exception error);

            [LogError(ToPlainLog = true)]
            public abstract void ServerBroadcastFailedToConnection(string ip, int port, Exception error);

            [LogVerbose(ToPlainLog = true)]
            public abstract void ClientSuccessfullyConnectedToServer();

            [LogError(ToPlainLog = true)]
            public abstract void ClientSendMessageFailed(Exception error);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionStartedReceiveMessagesTask(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionSendMessage(string party, int length);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionSendMessageHeaderWritten(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionSendMessageBodyWritten(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionSendMessageStreamFlushedSent(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionSendingMessageError(string party, Exception exception);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionSendingMessageFiringSendFailedEvent(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveMessagesStarted(string party, string toString);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveMessagesWaitingForHeader(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveMessagesTaskCanceledException(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveMessagesObjectDisposedException(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveMessagesFiringReceiveFailed(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveOneMessagesWaitingForHeader(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveOneMessagesSuccess(string party, int length);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveOneMessagesFiringMessageReceived(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveBytesWaiting(string party, int bytesToReceive);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveBytesReceived(string party, int numberOfBytes);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveBytesRemotePartyNotReachableClosingConnection(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveBytesSuccess(string party, int bytesReceived);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveBytesFailed(string party, Exception error);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionReceiveBytesFiringReceiveFailed(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionFinalizeSettingCloseReasonToLocalPartyShotDown(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionFinalizeFiringClosedEvent(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ConnectionFinalizeClosingSocket(string party);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerInitializing(string ip, int port);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerCtorCompleted(string ip, int port);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerTcpConnectionAccepted(IntPtr socketHandle);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerShutdownClosingTcpListener();

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerShutdownCloseTcpListenerFailure(string ip, int port, Exception exception);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerShutdownWaitingForThreadsToStop();

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerShutdownAcceptConnectionsStopped(bool success);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerShutdownConnectionManagerStopped(bool success);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerShutdownAllConnectionTasksStopped(bool success);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerConnectionManagerStarted();

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerConnectionManagerGotOpenRequest(IntPtr socketHandle);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerConnectionManagerFiringClientConnectedEvent();

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerConnectionManagerGotCloseRequest(IntPtr socketHandle, ConnectionCloseReason closeReason);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerConnectionManagerGotBadRequest();

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerConnectionClosedFiringClientDisconnected();

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ServerConnectionClosed(IntPtr socketHandler, ConnectionCloseReason closeReason);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ClientConnectingToServer(string host, int port);

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ClientCtorCompleted();

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ClientDisposeStoppingConnection();

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ClientDisposeClosingSocket();

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ClientDisposeAllResourcesReleased();

            [LogDebug(ToPlainLog = true), Conditional("TCPDEBUG")]
            public abstract void ClientSendMessage(int length);
        }
    }
}
