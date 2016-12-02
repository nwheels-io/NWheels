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
    public class DuplexTcpClientFactory
    {
        private readonly IComponentContext _components;
        private readonly IMethodCallObjectFactory _callFactory;
        private readonly IDuplexNetworkApiProxyFactory _proxyFactory;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DuplexTcpClientFactory(IComponentContext components, IMethodCallObjectFactory callFactory, IDuplexNetworkApiProxyFactory proxyFactory)
        {
            _components = components;
            _callFactory = callFactory;
            _proxyFactory = proxyFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TServerApi CreateServerProxy<TServerApi, TClientApi>(
            TClientApi clientObject,
            string serverHostname,
            int serverPort,
            int maxPendingMessages = Int32.MaxValue,
            int workerThreadCount = 1,
            TimeSpan? clientHeartbeatInterval = null,
            TimeSpan? serverPingInterval = null)
            where TServerApi : class
            where TClientApi : class
        {
            var session = new Session<TServerApi, TClientApi>(
                _proxyFactory,
                clientObject,
                serverHostname,
                serverPort,
                maxPendingMessages,
                workerThreadCount,
                clientHeartbeatInterval,
                serverPingInterval);

            return session.ServerProxy;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class Session<TServerApi, TClientApi> : IDuplexNetworkEndpointTransport
            where TServerApi : class
            where TClientApi : class
        {
            private readonly int _maxPendingMessages;
            private readonly TimeSpan? _clientHeartbeatInterval;
            private readonly TimeSpan? _serverPingInterval;
            private readonly TcpClient _tcpClient;
            private readonly BlockingCollection<byte[]> _workerQueue;
            private readonly NetworkStream _stream;
            private readonly CancellationTokenSource _cancellation;
            private readonly TServerApi _serverProxy;
            private readonly Task[] _workerThreads;
            private readonly Task _receiveMessagesTask;
            private volatile SessionCloseReason _closeReason = SessionCloseReason.Unknown;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Session(
                IDuplexNetworkApiProxyFactory proxyFactory,
                TClientApi clientObject,
                string serverHostname,
                int serverPort,
                int maxPendingMessages,
                int workerThreadCount,
                TimeSpan? clientHeartbeatInterval,
                TimeSpan? serverPingInterval)
            {
                _maxPendingMessages = maxPendingMessages;
                _clientHeartbeatInterval = clientHeartbeatInterval;
                _serverPingInterval = serverPingInterval;
                _tcpClient = new TcpClient();
                _tcpClient.Connect(serverHostname, serverPort);
                _workerQueue = new BlockingCollection<byte[]>();
                _stream = _tcpClient.GetStream();
                _cancellation = new CancellationTokenSource();

                _serverProxy = proxyFactory.CreateProxyInstance<TServerApi, TClientApi>(this, clientObject);
                ((IDuplexNetworkEndpointApiProxy)_serverProxy).Disposing += OnProxyDisposing;

                _workerThreads = new Task[workerThreadCount];
                for (int i = 0 ; i < _workerThreads.Length ; i++)
                {
                    _workerThreads[i] = Task.Factory.StartNew(RunWorkerThread);
                }
                
                _receiveMessagesTask = ReceiveMessages();
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

            public TServerApi ServerProxy
            {
                get { return _serverProxy; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public async Task ReceiveMessages()
            {
                var cancellation = _cancellation.Token;
                byte[] headerBuffer = new byte[sizeof(Int32)];

                while (!cancellation.IsCancellationRequested)
                {
                    try
                    {
                        int bytesRead;
                        bytesRead = await _stream.ReadAsync(headerBuffer, 0, headerBuffer.Length, cancellation);
                        if (bytesRead != headerBuffer.Length)
                        {
                            //TODO: log error + include circuit breaker
                            if (ReceiveFailed != null)
                            {
                                ReceiveFailed(new ProtocolViolationException("Unexpected end of data (body-length header)"));
                            }
                            _closeReason = SessionCloseReason.NetworkError;
                            break;
                        }

                        var bodyLength = BitConverter.ToInt32(headerBuffer, startIndex: 0);
                        var bodyBuffer = new byte[bodyLength];

                        bytesRead = await _stream.ReadAsync(bodyBuffer, 0, bodyBuffer.Length, cancellation);
                        if (bytesRead != bodyBuffer.Length)
                        {
                            //TODO: log error + include circuit breaker
                            if (ReceiveFailed != null)
                            {
                                ReceiveFailed(new ProtocolViolationException("Unexpected end of data (message-body)"));
                            }
                            _closeReason = SessionCloseReason.NetworkError;
                            break;
                        }

                        _workerQueue.Add(bodyBuffer, cancellation);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                }

                FinalizeAndCleanup();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void OnProxyDisposing()
            {
                _closeReason = SessionCloseReason.LocalPartyShutDown;
                _cancellation.Cancel();

                if (!_receiveMessagesTask.Wait(10000))
                {
                    //TODO: log warning
                }

                if (!Task.WaitAll(_workerThreads, 10000))
                {
                    //TODO: log warning
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FinalizeAndCleanup()
            {
                try
                {
                    ((IDuplexNetworkEndpointApiProxy)_serverProxy).NotifySessionClosed(_closeReason);
                }
                finally
                {
                    _stream.Dispose();
                    _tcpClient.Close();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RunWorkerThread()
            {
                while (!_cancellation.IsCancellationRequested)
                {
                    byte[] message;

                    try
                    {
                        message = _workerQueue.Take(_cancellation.Token);
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
                        RaiseBytesReceived(message);
                    }
                    catch //(Exception e)
                    {
                        //TODO: log error + include circuit breaker
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

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
        }
    }
}
