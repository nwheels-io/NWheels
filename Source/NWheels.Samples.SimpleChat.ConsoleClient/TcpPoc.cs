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
using NWheels.Endpoints;
using NWheels.Extensions;

namespace NWheels.Samples.SimpleChat.ConsoleClient
{
    public static class TcpPoc
    {
        private static DateTime _s_logStartTime;
        private static Stopwatch _s_logClock;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static TcpPoc()
        {
            _s_logStartTime = DateTime.Now;
            _s_logClock = Stopwatch.StartNew();
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

        private static void PrintLog(string party, string format, params object[] args)
        {
            var timestamp = _s_logStartTime.Add(_s_logClock.Elapsed);
            Console.WriteLine(
                "{0:HH:mm:ss.ffff} : {1}@{2:###0} > {3}", 
                timestamp, 
                party, 
                Thread.CurrentThread.ManagedThreadId, 
                format.FormatIf(args));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Server
        {
            private readonly string _ip;
            private readonly int _port;
            private readonly TcpListener _tcpListener;
            private readonly CancellationTokenSource _cancellation;
            private readonly Hashtable _sessionTaskBySession;
            private readonly BlockingCollection<object> _sessionManagerQueue;
            private readonly Task _sessionManagerTask;
            private readonly Task _acceptConnectionsTask;
            private readonly Action<Session, SessionCloseReason> _onSessionClosedDelegate;

            public Server(
                string ip, 
                int port, 
                Action<Session> onClientConnected = null, 
                Action<Session, SessionCloseReason> onClientDisconnected = null)
            {
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

                PrintLog("SERVER", "Server.ctor(): initializing");

                _tcpListener = new TcpListener(IPAddress.Parse(_ip ?? "127.0.0.1"), _port);
                _sessionManagerTask = Task.Factory.StartNew(ManageSessions, _cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                _acceptConnectionsTask = AcceptConnections();
                _tcpListener.Start(backlog: 1000);

                PrintLog("SERVER", "Server.ctor(): server is ready!");
            }

            public void Dispose()
            {
                _cancellation.Cancel();
                _tcpListener.Stop();
                
                _acceptConnectionsTask.Wait(TimeSpan.FromSeconds(10));
                _sessionManagerTask.Wait(TimeSpan.FromSeconds(5));

                var allSessionTasks = _sessionTaskBySession.Values.Cast<Task>().ToArray();
                Task.WaitAll(allSessionTasks, TimeSpan.FromSeconds(15));
            }

            public event Action<Session> ClientConnected;
            public event Action<Session, SessionCloseReason> ClientDisconnected;

            private async Task AcceptConnections()
            {
                await Task.Delay(10); // ensure migration from constructor's thread to thread pool

                PrintLog("SERVER", "Server.AcceptConnections(): ready to accept TCP connections");

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

                    PrintLog("SERVER", "Server.AcceptConnections(): accepted TCP connection");
                    _sessionManagerQueue.Add(socket);
                }
            }

            private void ManageSessions()
            {
                var cancellationToken = _cancellation.Token;

                PrintLog("SERVER", "Server.ManageSessions(): ready to manage sessions");

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

                    var openSessionRequest = request as Socket;
                    var closeSessionRequest = request as Session;

                    if (openSessionRequest != null)
                    {
                        PrintLog("SERVER", "Server.ManageSessions(): got OPEN request");

                        var session = new Session(socket: openSessionRequest, upstreamCancellation: _cancellation.Token);
                        session.Closed += _onSessionClosedDelegate;

                        if (ClientConnected != null)
                        {
                            PrintLog("SERVER", "Server.ManageSessions(): firing SESSIONOPENED event");
                            ClientConnected(session);
                        }

                        var sessionTask = session.BeginReceiveMessages();
                        _sessionTaskBySession.Add(session, sessionTask);
                    }
                    else if (closeSessionRequest != null)
                    {
                        PrintLog("SERVER", "Server.ManageSessions(): got CLOSE request");
                        _sessionTaskBySession.Remove(closeSessionRequest);
                    }
                    else
                    {
                        //TODO: log warning + circuit breaker
                        PrintLog("SERVER", "Server.ManageSessions(): got UNKNOWN request...");
                    }
                }
            }

            private void OnSessionClosed(Session session, SessionCloseReason reason)
            {
                PrintLog("SERVER", "Server.OnSessionClosed(): got notification with reason '{0}'", reason);
                
                if (ClientDisconnected != null)
                {
                    PrintLog("SERVER", "Server.OnSessionClosed(): firing SESSIONCLOSED event");
                    ClientDisconnected(session, reason);
                }

                _sessionManagerQueue.Add(session);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Client
        {
            private readonly TcpClient _tcpClient;
            private readonly NetworkStream _stream;
            //private readonly Session _session;
            private readonly CancellationTokenSource _cancellation;

            public Client(
                string serverHost, 
                int serverPort,
                Action<byte[]> onMessageReceived = null,
                Action<SessionCloseReason> onDisconnected = null)
            {
                _cancellation = new CancellationTokenSource();
                _tcpClient = new TcpClient();

                var effectiveServerHost = (serverHost ?? "localhost");

                PrintLog("CLIENT", "ctor(): connecting to {0}:{1}", effectiveServerHost, serverPort);

                _tcpClient.Connect(effectiveServerHost, serverPort);
                _stream = _tcpClient.GetStream();
                //_session = new Session(_tcpClient.Client, _tcpClient.GetStream(), _cancellation.Token, isClientSession: true);
                //_session.

                PrintLog("CLIENT", "ctor(): successfully conected!");
            }

            public void Dispose()
            {
                _cancellation.Cancel();
                _stream.Dispose();
                _tcpClient.Close();
            }

            //public event Action<byte[]> MessageReceived;
            //public event Action<SessionCloseReason> Disconnected;
            //public event Action<Exception> SendFailed;
            //public event Action<Exception> ReceiveFailed;

            public void RunScenario1()
            {
                PrintLog("CLIENT", "RunScenario1(): sleeping 10 sec");

                Thread.Sleep(10000);

                var message1 = Encoding.UTF8.GetBytes("Hello server");

                PrintLog("CLIENT", "RunScenario1(): sending message of {0} bytes length", message1.Length);

                _stream.Write(BitConverter.GetBytes(message1.Length), 0, 4);
                _stream.Write(message1, 0, message1.Length);

                PrintLog("CLIENT", "RunScenario1(): 1st message sent; sleeping 10 sec");

                Thread.Sleep(10000);

                var message2 = Encoding.UTF8.GetBytes("Goodbye server");

                PrintLog("CLIENT", "RunScenario1(): sending message of {0} bytes length", message2.Length);
                
                _stream.Write(BitConverter.GetBytes(message2.Length), 0, 4);
                _stream.Write(message2, 0, message2.Length);

                PrintLog("CLIENT", "RunScenario1(): all messages sent; DONE");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Session
        {
            private readonly Socket _socket;
            private readonly NetworkStream _stream;
            private readonly CancellationToken _upstreamCancellation;
            private readonly CancellationTokenSource _sessionCancellation;
            private readonly CancellationToken _anyReasonCancellation;
            private readonly SocketAddress _remoteAddress;
            private readonly string _logPartyName;
            private Task _receiveTask;
            private SessionCloseReason _closeReason;

            public Session(Socket socket, CancellationToken upstreamCancellation)
                : this(socket, new NetworkStream(socket, ownsSocket: true), upstreamCancellation)
            {
            }

            public Session(Socket socket, NetworkStream stream, CancellationToken upstreamCancellation, bool isClientSession = false)
            {
                _socket = socket;
                _stream = stream;
                _upstreamCancellation = upstreamCancellation;
                _sessionCancellation = new CancellationTokenSource();
                _anyReasonCancellation = CancellationTokenSource.CreateLinkedTokenSource(_upstreamCancellation, _sessionCancellation.Token).Token;
                _remoteAddress = socket.RemoteEndPoint.Serialize();
                _closeReason = SessionCloseReason.Unknown;
                _logPartyName = (isClientSession ? "CLIENT" : "SERVER");

                PrintLog(_logPartyName, "Session.ctor(): initialized session with remote address '{0}'.", _remoteAddress);
            }

            public Task BeginReceiveMessages()
            {
                if (_receiveTask != null)
                {
                    PrintLog(_logPartyName, "Session.BeginReceiveMessages(): already receiving messages! throwing!");
                    throw new InvalidOperationException("Already receiving messages.");
                }

                _receiveTask = ReceiveMessages();

                PrintLog(_logPartyName, "Session.BeginReceiveMessages(): started ReceiveMessages task");

                return _receiveTask;
            }

            public void SendMessage(byte[] message)
            {
                try
                {
                    Int32 length = (message != null ? message.Length : 0);

                    PrintLog(_logPartyName, "Session.SendMessage(): sending message of {0} bytes length", length);
                    
                    _stream.Write(BitConverter.GetBytes(length), 0, sizeof(Int32));

                    PrintLog(_logPartyName, "Session.SendMessage(): header written");

                    if (message != null)
                    {
                        _stream.Write(message, 0, message.Length);
                        PrintLog(_logPartyName, "Session.SendMessage(): body written");
                    }

                    _stream.Flush();

                    PrintLog(_logPartyName, "Session.SendMessage(): stream flushed -- message sent");
                }
                catch (Exception e)
                {
                    PrintLog(_logPartyName, "Session.SendMessage(): exception! {0}", e.ToString());

                    if (SendFailed != null)
                    {
                        PrintLog(_logPartyName, "Session.SendMessage(): firing SENDFAILED event");
                        SendFailed(this, e);
                    }
                }
            }

            public SocketAddress RemoteAddress
            {
                get { return _remoteAddress; }
            }

            public SessionCloseReason CloseReason
            {
                get { return _closeReason; }
            }

            public event Action<Session, byte[]> MessageReceived;
            public event Action<Session, Exception> SendFailed;
            public event Action<Session, Exception> ReceiveFailed;
            public event Action<Session, SessionCloseReason> Closed;

            private async Task ReceiveMessages()
            {
                PrintLog(_logPartyName, "Session.ReceiveMessages(): starting to receive messages", _remoteAddress);

                try
                {
                    byte[] headerBuffer = new byte[sizeof(Int32)];

                    while (!_anyReasonCancellation.IsCancellationRequested)
                    {
                        try
                        {
                            PrintLog(_logPartyName, "Session.ReceiveMessages(): waiting for header...");
                            await ReceiveOneMessage(headerBuffer);
                        }
                        catch (TaskCanceledException)
                        {
                            PrintLog(_logPartyName, "Session.ReceiveMessages(): TaskCanceledException");
                            _closeReason = SessionCloseReason.LocalPartyShutDown;
                            break;
                        }
                        catch (ObjectDisposedException)
                        {
                            PrintLog(_logPartyName, "Session.ReceiveMessages(): ObjectDisposedException");
                            _closeReason = SessionCloseReason.LocalPartyShutDown;
                            break;
                        }
                        catch (Exception e)
                        {
                            PrintLog(_logPartyName, "Session.ReceiveMessages(): exception {0}", e.ToString());
                            //TODO: log error + include circuit breaker
                            if (ReceiveFailed != null)
                            {
                                PrintLog(_logPartyName, "Session.ReceiveMessages(): firing RECEIVEFAILED event");
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

            private async Task ReceiveOneMessage(byte[] headerBuffer)
            {
                PrintLog(_logPartyName, "Session.ReceiveOneMessage(): waiting for header...");

                if (!await ReceiveBytes(headerBuffer, 0, headerBuffer.Length)) //TODO: include timeout
                {
                    PrintLog(_logPartyName, "Session.ReceiveOneMessage(): length header was not received");
                    //TODO: log warning + include circuit breaker
                    return;
                }

                var bodyLength = BitConverter.ToInt32(headerBuffer, startIndex: 0);
                var bodyBuffer = new byte[bodyLength];

                if (!await ReceiveBytes(bodyBuffer, 0, bodyBuffer.Length)) //TODO: include timeout
                {
                    PrintLog(_logPartyName, "Session.ReceiveOneMessage(): body was not received");
                    //TODO: log warning + include circuit breaker
                    return;
                }

                PrintLog(_logPartyName, "Session.ReceiveOneMessage(): successfully received message of {0} bytes length", bodyBuffer.Length);

                if (MessageReceived != null)
                {
                    PrintLog(_logPartyName, "Session.ReceiveOneMessage(): firing MESSAGERECEIVED event");
                    MessageReceived(this, bodyBuffer);
                }
            }

            private async Task<bool> ReceiveBytes(byte[] buffer, int offset, int count) //TODO: include timeout
            {
                try
                {
                    var totalBytesReceived = 0;

                    while (totalBytesReceived < count)
                    {
                        PrintLog(_logPartyName, "Session.ReceiveBytes(): waiting to receive {0} more bytes...", count - totalBytesReceived);

                        var chunkBytesReceived = await _stream.ReadAsync(
                            buffer, 
                            offset + totalBytesReceived, 
                            count - totalBytesReceived, 
                            _anyReasonCancellation);

                        PrintLog(_logPartyName, "Session.ReceiveBytes(): received {0} bytes", chunkBytesReceived);

                        if (chunkBytesReceived <= 0)
                        {
                            PrintLog(_logPartyName, "Session.ReceiveBytes(): closing session as RemotePartyNotReachable");

                            _closeReason = SessionCloseReason.RemotePartyNotReachable;
                            _sessionCancellation.Cancel();
                            return false;
                        }

                        totalBytesReceived += chunkBytesReceived;
                    }

                    PrintLog(_logPartyName, "Session.ReceiveBytes(): successfully received {0} bytes", totalBytesReceived);

                    return true;
                }
                catch (Exception e)
                {
                    PrintLog(_logPartyName, "Session.ReceiveBytes(): exception! {0}", e.ToString());

                    if (ReceiveFailed != null)
                    {
                        PrintLog(_logPartyName, "Session.ReceiveBytes(): firing RECEIVEFAILED event");
                        ReceiveFailed(this, e);
                    }
                }

                return false;
            }

            private void FinalizeAndCleanup()
            {
                PrintLog(_logPartyName, "Session.FinalizeAndCleanup(): finalizing session with close reason '{0}'.", _closeReason);

                try
                {
                    if (_upstreamCancellation.IsCancellationRequested)
                    {
                        PrintLog(_logPartyName, "Session.FinalizeAndCleanup(): setting close reason to 'LocalPartyShutDown'.");
                        _closeReason = SessionCloseReason.LocalPartyShutDown;
                    }

                    if (Closed != null)
                    {
                        PrintLog(_logPartyName, "Session.FinalizeAndCleanup(): firing CLOSED event");
                        Closed(this, _closeReason);
                    }
                }
                finally
                {
                    try
                    {
                        PrintLog(_logPartyName, "Session.FinalizeAndCleanup(): closing network stream and socket");
                        _stream.Close();
                        //necessary? SafeCloseSocket(_socket);
                    }
                    catch (Exception e)
                    {
                        //TODO: log warning + include circuit breaker
                        PrintLog(_logPartyName, "Session.FinalizeAndCleanup(): exception! {0}.", e.ToString());
                    }
                }
            }
        }
    }
}
