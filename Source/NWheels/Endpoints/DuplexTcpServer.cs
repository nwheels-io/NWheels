using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NWheels.Endpoints.Core;
using NWheels.Endpoints.Factories;
using NWheels.Processing.Commands.Factories;
using NWheels.Serialization;

namespace NWheels.Endpoints
{
    public class DuplexTcpServer<TServerApi, TClientApi> : IDisposable
        where TServerApi : class 
        where TClientApi : class
    {
        private readonly IComponentContext _components;
        private readonly IDuplexNetworkApiProxyFactory _proxyFactory;
        private readonly int _maxPendingConnections;
        private readonly TimeSpan? _clientHeartbeatInterval;
        private readonly TimeSpan? _serverPingInterval;
        private readonly Func<TClientApi, TServerApi> _serverObjectFactory;
        private readonly TcpListener _tcpListener;
        private readonly BlockingCollection<Tuple<Socket, Session>> _sessionManagerQueue; // session==null -> OPEN ; session!=null -> CLOSE
        private readonly Hashtable _sessionTaskBySessionObject; // single writer = sessionManagerThread; multiple readers
        private readonly Action<Session> _onSessionClosedDelegate;
        private readonly BlockingCollection<Tuple<Session, byte[]>> _workerQueue; // messages received by sessions
        private readonly CancellationTokenSource _cancellation;
        private readonly Task _incomingConnectionReceiver;
        private readonly Task _sessionManagerThread;
        private readonly Task[] _workerThreads;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DuplexTcpServer(
            IComponentContext components,
            IDuplexNetworkApiProxyFactory proxyFactory,
            int listenPortNumber,
            int listenBacklog = Int32.MaxValue,
            int maxPendingConnections = Int32.MaxValue,
            int workerThreadCount = 1,
            TimeSpan? clientHeartbeatInterval = null,
            TimeSpan? serverPingInterval = null,
            Func<TClientApi, TServerApi> serverObjectFactory = null)
        {
            _components = components;
            _proxyFactory = proxyFactory;
            _maxPendingConnections = maxPendingConnections;
            _clientHeartbeatInterval = clientHeartbeatInterval;
            _serverPingInterval = serverPingInterval;
            _serverObjectFactory = (serverObjectFactory ?? ResolveServerObjectByDefault);
            _sessionManagerQueue = new BlockingCollection<Tuple<Socket, Session>>();
            _sessionTaskBySessionObject = new Hashtable();
            _onSessionClosedDelegate = this.OnSessionClosed;
            _workerQueue = new BlockingCollection<Tuple<Session, byte[]>>();
            _cancellation = new CancellationTokenSource();
            
            _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), listenPortNumber);
            _tcpListener.Start(backlog: listenBacklog);
            
            _sessionManagerThread = Task.Factory.StartNew(RunSessionManagerThread);
            _incomingConnectionReceiver = ReceiveIncomingConnections();

            _workerThreads = new Task[workerThreadCount];
            
            for (int i = 0 ; i < _workerThreads.Length ; i++)
            {
                _workerThreads[i] = Task.Factory.StartNew(RunWorkerThread);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _cancellation.Cancel();
            _tcpListener.Stop();

            if (!Task.WaitAll(new[] { _incomingConnectionReceiver, _sessionManagerThread }, 10000))
            {
                //TODO: log warning
            }

            if (!Task.WaitAll(_workerThreads, 10000))
            {
                //TODO: log warning
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Broadcast(Action<TClientApi> sender)
        {
            var currentSessions = _sessionTaskBySessionObject.Keys.Cast<Session>().ToArray();

            for (int i = 0 ; i < currentSessions.Length ; i++)
            {
                if (!currentSessions[i].IsClosed)
                {
                    try
                    {
                        sender(currentSessions[i].ClientProxy);
                    }
                    catch //(Exception e)
                    {
                        //TODO: log error
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task ReceiveIncomingConnections()
        {
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

                if (_sessionManagerQueue.Count < _maxPendingConnections)
                {
                    _sessionManagerQueue.Add(new Tuple<Socket, Session>(socket, null));
                }
                else
                {
                    // too busy
                    SafeCloseSocket(socket);
                    //TODO: log error + include circuit breaker
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunSessionManagerThread()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                Socket socket;
                Session session;

                try
                {
                    var workItem = _sessionManagerQueue.Take(_cancellation.Token);
                    socket = workItem.Item1;
                    session = workItem.Item2;
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch //(Exception e)
                {
                    //TODO:log error + include circuit breaker
                    continue;
                }

                try
                {
                    if (session == null && socket != null) // OPEN SESSION request
                    {
                        OpenSession(socket);
                    }
                    else if (session != null && socket == null) // CLOSE SESSION request
                    {
                        CloseSession(session);
                    }
                    else
                    {
                        //TODO: log invalid request
                    }
                }
                catch //(Exception e)
                {
                    //TODO: log error + include circuit breaker
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunWorkerThread()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                Session session;
                byte[] message;

                try
                {
                    var workItem = _workerQueue.Take(_cancellation.Token);
                    session = workItem.Item1;
                    message = workItem.Item2;
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch //(Exception e)
                {
                    //TODO:log error + include circuit breaker
                    continue;
                }

                try
                {
                    session.RaiseBytesReceived(message);
                }
                catch //(Exception e)
                {
                    //TODO: log error + include circuit breaker
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OpenSession(Socket socket)
        {
            Session session;
            Task sessionTask;

            if (TryOpenSession(socket, out session, out sessionTask))
            {
                _sessionTaskBySessionObject[session] = sessionTask;
            }
            else
            {
                SafeCloseSocket(socket);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CloseSession(Session session)
        {
            _sessionTaskBySessionObject.Remove(session);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryOpenSession(Socket socket, out Session session, out Task sessionTask)
        {
            try
            {
                session = new Session(socket, _components.Resolve<TServerApi>(), _proxyFactory, _workerQueue, _cancellation.Token);
            }
            catch //(Exception e)
            {
                //TODO: log error + include circuit breaker
                session = null;
                sessionTask = null;
                return false;
            }

            session.Closed += _onSessionClosedDelegate;
            sessionTask = session.ReceiveMessages();
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TServerApi ResolveServerObjectByDefault(TClientApi client)
        {
            return _components.Resolve<TServerApi>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnSessionClosed(Session session)
        {
            _sessionManagerQueue.Add(new Tuple<Socket, Session>(null, session));
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

        private class Session : IDisposable, IDuplexNetworkEndpointTransport
        {
            private readonly Socket _socket;
            private readonly BlockingCollection<Tuple<Session, byte[]>> _workerQueue;
            private readonly NetworkStream _stream;
            private readonly TClientApi _clientProxy;
            private readonly CancellationToken _serverCancellation;
            private readonly CancellationTokenSource _sessionCancellation;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Session(
                Socket socket, 
                TServerApi serverObject, 
                IDuplexNetworkApiProxyFactory proxyFactory, 
                BlockingCollection<Tuple<Session, byte[]>> workerQueue,
                CancellationToken cancellation)
            {
                _socket = socket;
                _workerQueue = workerQueue;
                _stream = new NetworkStream(socket);
                _serverCancellation = cancellation;
                _sessionCancellation = new CancellationTokenSource();
                _clientProxy = proxyFactory.CreateProxyInstance<TClientApi, TServerApi>(this, serverObject);

                //TODO: match session initiator method if one is defined and throw if not matched or failed 
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                try
                {
                    _stream.Dispose();
                }
                finally
                {
                    SafeCloseSocket(_socket);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IDuplexNetworkEndpointTransport

            public void SendBytes(byte[] bytes)
            {
                try
                {
                    _stream.Write(bytes, 0, bytes.Length);
                    _stream.Flush();
                }
                catch (Exception e)
                {
                    //TODO: log error + include circuit breaker

                    if (SendFailed != null)
                    {
                        SendFailed(e);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<byte[]> BytesReceived;
            public event Action<Exception> SendFailed;
            public event Action<Exception> ReceiveFailed;

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RaiseBytesReceived(byte[] bytes)
            {
                if (BytesReceived != null)
                {
                    try
                    {
                        BytesReceived(bytes);
                    }
                    catch (Exception e)
                    {
                        //TODO: log error + include circuit breaker

                        if (ReceiveFailed != null)
                        {
                            ReceiveFailed(e);
                        }
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<Session> Closed;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public async Task ReceiveMessages()
            {
                var sessionOrServerCancellation = CancellationTokenSource.CreateLinkedTokenSource(_serverCancellation, _sessionCancellation.Token).Token;
                byte[] headerBuffer = new byte[sizeof(Int32)];

                while (!sessionOrServerCancellation.IsCancellationRequested)
                {
                    try
                    {
                        if (await _stream.ReadAsync(headerBuffer, 0, headerBuffer.Length, sessionOrServerCancellation) != headerBuffer.Length)
                        {
                            //TODO: log warning + include circuit breaker
                            if (ReceiveFailed != null)
                            {
                                ReceiveFailed(new ProtocolViolationException("Unexpected end of data (body-length header)"));
                            }
                            break;
                        }

                        var bodyLength = BitConverter.ToInt32(headerBuffer, startIndex: 0);
                        var bodyBuffer = new byte[bodyLength];

                        if (await _stream.ReadAsync(bodyBuffer, 0, bodyBuffer.Length, sessionOrServerCancellation) != bodyBuffer.Length)
                        {
                            //TODO: log warning + include circuit breaker
                            if (ReceiveFailed != null)
                            {
                                ReceiveFailed(new ProtocolViolationException("Unexpected end of data (message-body)"));
                            }
                            break;
                        }

                        _workerQueue.Add(new Tuple<Session, byte[]>(this, bodyBuffer), sessionOrServerCancellation);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        //TODO: log error + include circuit breaker

                        if (ReceiveFailed != null)
                        {
                            ReceiveFailed(e);
                        }
                    }
                }

                FinalizeAndCleanup();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TClientApi ClientProxy
            {
                get { return _clientProxy; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsClosed { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FinalizeAndCleanup()
            {
                IsClosed = true;

                try
                {
                    if (Closed != null)
                    {
                        Closed(this);
                    }
                }
                finally
                {
                    Dispose();
                }
            }
        }
    }
}
