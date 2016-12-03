using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Endpoints
{
    public class DuplexTcpTransport
    {
        public class Server
        {
            private readonly Logger _logger;
            private readonly string _ip;
            private readonly int _port;
            private readonly TcpListener _tcpListener;
            private readonly CancellationTokenSource _cancellation;
            private readonly Hashtable _sessionTaskBySession;
            private readonly BlockingCollection<object> _sessionManagerQueue;
            private readonly Task _sessionManagerTask;
            private readonly Task _acceptConnectionsTask;
            private readonly Action<Session, SessionCloseReason> _onSessionClosedDelegate;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Server(
                Logger logger,
                string ip,
                int port,
                Action<Session> onClientConnected = null,
                Action<Session, SessionCloseReason> onClientDisconnected = null)
            {
                _logger = logger;
                _ip = ip;
                _port = port;
                _cancellation = new CancellationTokenSource();
                _sessionTaskBySession = new Hashtable();
                _sessionManagerQueue = new BlockingCollection<object>();
                _onSessionClosedDelegate = this.OnSessionClosed;

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
                _sessionManagerTask = Task.Factory.StartNew(ManageSessions, _cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                _acceptConnectionsTask = AcceptConnections();
                _tcpListener.Start(backlog: 1000);

                _logger.ServerCtorCompleted(ip, port);
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

                var sessionManagerStopped = _sessionManagerTask.Wait(TimeSpan.FromSeconds(5));
                _logger.ServerShutdownSessionManagerStopped(success: sessionManagerStopped);

                var allSessionTasks = _sessionTaskBySession.Values.Cast<Task>().ToArray();
                var allSessionTasksStopped = Task.WaitAll(allSessionTasks, TimeSpan.FromSeconds(15));
                _logger.ServerShutdownAllSessionTasksStopped(success: allSessionTasksStopped);

                if (!acceptConnectionsStopped || !sessionManagerStopped || !allSessionTasksStopped)
                {
                    _logger.ServerShutdownSomeThreadsDidNotProperlyStop(_ip, _port, acceptConnectionsStopped, sessionManagerStopped, allSessionTasksStopped);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<Session> ClientConnected;
            public event Action<Session, SessionCloseReason> ClientDisconnected;

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

                    _logger.ServerTcpConnectionAccepted(socketHandle: socket.Handle);
                    _sessionManagerQueue.Add(socket);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ManageSessions()
            {
                var cancellationToken = _cancellation.Token;

                _logger.ServerSessionManagerStarted();

                while (!cancellationToken.IsCancellationRequested)
                {
                    object request;

                    try
                    {
                        request = _sessionManagerQueue.Take(cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }

                    HandleSessionManagerRequest(request);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void HandleSessionManagerRequest(object request)
            {
                var openSessionRequest = request as Socket;
                var closeSessionRequest = request as Session;

                if (openSessionRequest != null)
                {
                    _logger.ServerSessionManagerGotOpenRequest(socketHandle: openSessionRequest.Handle);

                    var session = new Session(socket: openSessionRequest, upstreamCancellation: _cancellation.Token, logger: _logger);
                    session.Closed += _onSessionClosedDelegate;

                    if (ClientConnected != null)
                    {
                        _logger.ServerSessionManagerFiringClientConnectedEvent();
                        ClientConnected(session);
                    }

                    var sessionTask = session.BeginReceiveMessages();
                    _sessionTaskBySession.Add(session, sessionTask);
                }
                else if (closeSessionRequest != null)
                {
                    _logger.ServerSessionManagerGotCloseRequest(socketHandle: closeSessionRequest.SocketHandle, closeReason: closeSessionRequest.CloseReason);
                    _sessionTaskBySession.Remove(closeSessionRequest);
                }
                else
                {
                    _logger.ServerSessionManagerGotBadRequest();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void OnSessionClosed(Session session, SessionCloseReason reason)
            {
                _logger.ServerSessionClosed(socketHandler: session.SocketHandle, closeReason: reason);

                if (ClientDisconnected != null)
                {
                    _logger.ServerSessionClosedFiringClientDisconnected();
                    ClientDisconnected(session, reason);
                }

                _sessionManagerQueue.Add(session);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Client
        {
            private readonly Logger _logger;
            private readonly TcpClient _tcpClient;
            private readonly NetworkStream _stream;
            private readonly Session _session;
            private readonly Task _receiveTask;
            private readonly CancellationTokenSource _cancellation;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Client(
                Logger logger,
                string serverHost,
                int serverPort,
                Action<byte[]> onMessageReceived = null,
                Action<SessionCloseReason> onDisconnected = null)
            {
                _logger = logger;
                _cancellation = new CancellationTokenSource();
                _tcpClient = new TcpClient();

                var effectiveServerHost = (serverHost ?? "localhost");

                _logger.ClientConnectingToServer(host: effectiveServerHost, port: serverPort);

                _tcpClient.Connect(effectiveServerHost, serverPort);
                _stream = _tcpClient.GetStream();

                _logger.ClientSuccessfullyConnectedToServer();

                _session = new Session(_tcpClient.Client, _tcpClient.GetStream(), _cancellation.Token, isClientSession: true, logger: _logger);

                AttachSessionEventHandlers();

                _receiveTask = _session.BeginReceiveMessages();

                _logger.ClientCtorCompleted();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _logger.ClientDisposeStoppingSession();

                _cancellation.Cancel();
                _receiveTask.Wait(TimeSpan.FromSeconds(10));

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
                    _session.SendMessage(message);
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
            public event Action<SessionCloseReason> Disconnected;
            public event Action<Exception> SendFailed;
            public event Action<Exception> ReceiveFailed;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AttachSessionEventHandlers()
            {
                _session.MessageReceived += (session, message) => {
                    if (MessageReceived != null)
                    {
                        MessageReceived(message);
                    }
                };

                _session.Closed += (session, reason) => {
                    if (Disconnected != null)
                    {
                        Disconnected(reason);
                    }
                };

                _session.SendFailed += (session, error) => {
                    if (SendFailed != null)
                    {
                        SendFailed(error);
                    }
                };

                _session.ReceiveFailed += (session, error) => {
                    if (ReceiveFailed != null)
                    {
                        ReceiveFailed(error);
                    }
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Session
        {
            private readonly Socket _socket;
            private readonly NetworkStream _stream;
            private readonly CancellationToken _upstreamCancellation;
            private readonly Logger _logger;
            private readonly CancellationTokenSource _sessionCancellation;
            private readonly CancellationToken _anyReasonCancellation;
            private readonly SocketAddress _remoteAddress;
            private readonly string _logPartyName;
            private Task _receiveTask;
            private SessionCloseReason _closeReason;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Session(Socket socket, CancellationToken upstreamCancellation, Logger logger)
                : this(socket, new NetworkStream(socket, ownsSocket: true), upstreamCancellation, logger)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Session(Socket socket, NetworkStream stream, CancellationToken upstreamCancellation, Logger logger, bool isClientSession = false)
            {
                _socket = socket;
                _stream = stream;
                _upstreamCancellation = upstreamCancellation;
                _logger = logger;
                _sessionCancellation = new CancellationTokenSource();
                _anyReasonCancellation = CancellationTokenSource.CreateLinkedTokenSource(_upstreamCancellation, _sessionCancellation.Token).Token;
                _remoteAddress = socket.RemoteEndPoint.Serialize();
                _closeReason = SessionCloseReason.Unknown;
                _logPartyName = (isClientSession ? "CLIENT" : "SERVER");

                _logger.SessionInitialized(_logPartyName, _remoteAddress.ToString());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task BeginReceiveMessages()
            {
                if (_receiveTask != null)
                {
                    throw _logger.SessionAlreadyReceivingMessages(_logPartyName);
                }

                _receiveTask = ReceiveMessages();

                _logger.SessionStartedReceiveMessagesTask(_logPartyName);

                return _receiveTask;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SendMessage(byte[] message)
            {
                try
                {
                    Int32 length = (message != null ? message.Length : 0);

                    _logger.SessionSendMessage(_logPartyName, length);

                    _stream.Write(BitConverter.GetBytes(length), 0, sizeof(Int32));

                    _logger.SessionSendMessageHeaderWritten(_logPartyName);

                    if (message != null)
                    {
                        _stream.Write(message, 0, message.Length);
                        _logger.SessionSendMessageBodyWritten(_logPartyName);
                    }

                    _stream.Flush();

                    _logger.SessionSendMessageStreamFlushedSent(_logPartyName);
                }
                catch (Exception e)
                {
                    _logger.SessionSendingMessageError(_logPartyName, e);

                    if (SendFailed != null)
                    {
                        _logger.SessionSendingMessageFiringSendFailedEvent(_logPartyName);
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

            public SessionCloseReason CloseReason
            {
                get { return _closeReason; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<Session, byte[]> MessageReceived;
            public event Action<Session, Exception> SendFailed;
            public event Action<Session, Exception> ReceiveFailed;
            public event Action<Session, SessionCloseReason> Closed;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal IntPtr SocketHandle
            {
                get { return _socket.Handle; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private async Task ReceiveMessages()
            {
                _logger.SessionReceiveMessagesStarted(_logPartyName, _remoteAddress.ToString());

                try
                {
                    byte[] headerBuffer = new byte[sizeof(Int32)];

                    while (!_anyReasonCancellation.IsCancellationRequested)
                    {
                        try
                        {
                            _logger.SessionReceiveMessagesWaitingForHeader(_logPartyName);
                            await ReceiveOneMessage(headerBuffer);
                        }
                        catch (TaskCanceledException)
                        {
                            _logger.SessionReceiveMessagesTaskCanceledException(_logPartyName);
                            _closeReason = SessionCloseReason.LocalPartyShutDown;
                            break;
                        }
                        catch (ObjectDisposedException)
                        {
                            _logger.SessionReceiveMessagesObjectDisposedException(_logPartyName);
                            _closeReason = SessionCloseReason.LocalPartyShutDown;
                            break;
                        }
                        catch (Exception e)
                        {
                            _logger.SessionReceiveMessagesException(_logPartyName, e);

                            if (ReceiveFailed != null)
                            {
                                _logger.SessionReceiveMessagesFiringReceiveFailed(_logPartyName);
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
                _logger.SessionReceiveOneMessagesWaitingForHeader(_logPartyName);

                if (!await ReceiveBytes(headerBuffer, 0, headerBuffer.Length)) //TODO: include timeout
                {
                    _logger.SessionReceiveOneMessagesHeaderWasNotReceived(_logPartyName);
                    return;
                }

                var bodyLength = BitConverter.ToInt32(headerBuffer, startIndex: 0);
                var bodyBuffer = new byte[bodyLength];

                if (!await ReceiveBytes(bodyBuffer, 0, bodyBuffer.Length)) //TODO: include timeout
                {
                    _logger.SessionReceiveOneMessagesBodyWasNotReceived(_logPartyName);
                    return;
                }

                _logger.SessionReceiveOneMessagesSuccess(_logPartyName, bodyBuffer.Length);

                if (MessageReceived != null)
                {
                    _logger.SessionReceiveOneMessagesFiringMessageReceived(_logPartyName);
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
                        _logger.SessionReceiveBytesWaiting(_logPartyName, bytesToReceive: count - totalBytesReceived);

                        var chunkBytesReceived = await _stream.ReadAsync(
                            buffer,
                            offset + totalBytesReceived,
                            count - totalBytesReceived,
                            _anyReasonCancellation);

                        _logger.SessionReceiveBytesReceived(_logPartyName, numberOfBytes: chunkBytesReceived);

                        if (chunkBytesReceived <= 0)
                        {
                            _logger.SessionReceiveBytesRemotePartyNotReachableClosingSession(_logPartyName);

                            _closeReason = SessionCloseReason.RemotePartyNotReachable;
                            _sessionCancellation.Cancel();
                            return false;
                        }

                        totalBytesReceived += chunkBytesReceived;
                    }

                    _logger.SessionReceiveBytesSuccess(_logPartyName, bytesReceived: totalBytesReceived);

                    return true;
                }
                catch (Exception e)
                {
                    _logger.SessionReceiveBytesFailed(_logPartyName, error: e);

                    if (ReceiveFailed != null)
                    {
                        _logger.SessionReceiveBytesFiringReceiveFailed(_logPartyName);
                        ReceiveFailed(this, e);
                    }
                }

                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FinalizeAndCleanup()
            {
                _logger.SessionFinalizing(_logPartyName, _closeReason);

                try
                {
                    if (_upstreamCancellation.IsCancellationRequested)
                    {
                        _logger.SessionFinalizeSettingCloseReasonToLocalPartyShotDown(_logPartyName);
                        _closeReason = SessionCloseReason.LocalPartyShutDown;
                    }

                    if (Closed != null)
                    {
                        _logger.SessionFinalizeFiringClosedEvent(_logPartyName);
                        Closed(this, _closeReason);
                    }
                }
                finally
                {
                    try
                    {
                        _logger.SessionFinalizeClosingSocket(_logPartyName);
                        _stream.Close();
                        //necessary? SafeCloseSocket(_socket);
                    }
                    catch (Exception e)
                    {
                        _logger.SessionFinalizeFailure(_logPartyName, error: e);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class Logger : IApplicationEventLogger
        {
            [LogInfo]
            public abstract void ServerReadyToAcceptTcpConnections(string ip, int port);
            
            [LogInfo]
            public abstract void ServerShuttingDown(string ip, int port);

            [LogWarning]
            public abstract void ServerShutdownSomeThreadsDidNotProperlyStop(
                string ip, 
                int port, 
                bool acceptConnectionsStopped, 
                bool sessionManagerStopped, 
                bool allSessionTasksStopped);

            [LogVerbose]
            public abstract void SessionInitialized(string party, string remoteAddress);

            [LogError]
            public abstract InvalidOperationException SessionAlreadyReceivingMessages(string party);

            [LogError]
            public abstract void SessionReceiveMessagesException(string party, Exception exception);

            [LogWarning]
            public abstract void SessionReceiveOneMessagesHeaderWasNotReceived(string party);

            [LogWarning]
            public abstract void SessionReceiveOneMessagesBodyWasNotReceived(string party);

            [LogVerbose]
            public abstract void SessionFinalizing(string party, SessionCloseReason closeReason);

            [LogError]
            public abstract void SessionFinalizeFailure(string party, Exception error);

            [LogVerbose]
            public abstract void ClientSuccessfullyConnectedToServer();
            
            [LogError]
            public abstract void ClientSendMessageFailed(Exception error);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionStartedReceiveMessagesTask(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionSendMessage(string party, int length);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionSendMessageHeaderWritten(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionSendMessageBodyWritten(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionSendMessageStreamFlushedSent(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionSendingMessageError(string party, Exception exception);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionSendingMessageFiringSendFailedEvent(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveMessagesStarted(string party, string toString);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveMessagesWaitingForHeader(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveMessagesTaskCanceledException(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveMessagesObjectDisposedException(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveMessagesFiringReceiveFailed(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveOneMessagesWaitingForHeader(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveOneMessagesSuccess(string party, int length);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveOneMessagesFiringMessageReceived(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveBytesWaiting(string party, int bytesToReceive);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveBytesReceived(string party, int numberOfBytes);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveBytesRemotePartyNotReachableClosingSession(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveBytesSuccess(string party, int bytesReceived);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveBytesFailed(string party, Exception error);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionReceiveBytesFiringReceiveFailed(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionFinalizeSettingCloseReasonToLocalPartyShotDown(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionFinalizeFiringClosedEvent(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void SessionFinalizeClosingSocket(string party);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerInitializing(string ip, int port);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerCtorCompleted(string ip, int port);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerTcpConnectionAccepted(IntPtr socketHandle);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerShutdownClosingTcpListener();

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerShutdownCloseTcpListenerFailure(string ip, int port, Exception exception);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerShutdownWaitingForThreadsToStop();

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerShutdownAcceptConnectionsStopped(bool success);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerShutdownSessionManagerStopped(bool success);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerShutdownAllSessionTasksStopped(bool success);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerSessionManagerStarted();

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerSessionManagerGotOpenRequest(IntPtr socketHandle);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerSessionManagerFiringClientConnectedEvent();

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerSessionManagerGotCloseRequest(IntPtr socketHandle, SessionCloseReason closeReason);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerSessionManagerGotBadRequest();

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerSessionClosedFiringClientDisconnected();

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ServerSessionClosed(IntPtr socketHandler, SessionCloseReason closeReason);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ClientConnectingToServer(string host, int port);

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ClientCtorCompleted();

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ClientDisposeStoppingSession();

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ClientDisposeClosingSocket();

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ClientDisposeAllResourcesReleased();

            [LogDebug, Conditional("TCPDEBUG")]
            public abstract void ClientSendMessage(int length);
        }
    }
}
