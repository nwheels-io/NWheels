using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NWheels.Concurrency;
using NWheels.Endpoints;
using NWheels.Endpoints.Factories;
using NWheels.Extensions;
using NWheels.Processing.Commands;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests.Endpoints.Factories
{
    [TestFixture]
    public class DuplexNetworkApiProxyFactoryTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void TestTaskCompletionSource()
        {
            //-- arrange

            var producerCompletionSource = new TaskCompletionSource<string>();

            string returnValue = null;
            Func<Task> doConsume = async () => {
                returnValue = await producerCompletionSource.Task;
            };

            //-- act & assert

            var consumerTask = doConsume();
            consumerTask.Wait(500).ShouldBe(false);

            producerCompletionSource.SetResult("ABC");
            consumerTask.Wait(0).ShouldBe(true);

            returnValue.ShouldBe("ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void PingPong()
        {
            //-- arrange

            var network = new MemoryStream();
            var serverTransport = new TestTransport(network);
            var clientTransport = new TestTransport(network);

            var proxyFactory = Resolve<IDuplexNetworkApiProxyFactory>();
            var clientObject = new ClientApiImplementation(clientTransport, proxyFactory);
            var serverObject = new ServerApiImplementation();

            var proxyUsedByListenerOnServer = proxyFactory.CreateProxyInstance<IClientApi, IServerApi>(serverTransport, serverObject);

            //-- act

            clientObject.TestPing("Hello from client");
            serverTransport.TestReceiveFromNetwork();
            clientTransport.TestReceiveFromNetwork();

            //-- assert

            serverObject.Log.ShouldBe(new[] {
                "ServerApiImplementation.Ping(Hello from client)"
            });
            clientObject.Log.ShouldBe(new[] {
                "ClientApiImplementation.Pong(YOU SENT [Hello from client])"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void AsyncInvocationWithTask()
        {
            //-- arrange

            var network = new MemoryStream();
            var serverTransport = new TestTransport(network);
            var clientTransport = new TestTransport(network);

            var proxyFactory = Resolve<IDuplexNetworkApiProxyFactory>();
            var clientObject = new ClientApiImplementation(clientTransport, proxyFactory);
            var serverObject = new ServerApiImplementation();

            var proxyUsedByListenerOnServer = proxyFactory.CreateProxyInstance<IClientApi, IServerApi>(serverTransport, serverObject);
            //var proxyUsedOnClient = proxyFactory.CreateProxyInstance<IServerApi, IClientApi>(clientTransport, clientObject);

            string returnValue = null;
            Func<Task> doEcho = async () => {
                returnValue = await clientObject.Server.ReverseEcho("Hello world");
            };

            //-- act

            var doEchoTask = doEcho(); // returns after it begins await for completion of the call to server
            
            serverTransport.TestReceiveFromNetwork();
            
            var finishedTooEarly = doEchoTask.Wait(500); // should return false as the task is awaiting for server reply
            var earlyClientLog = clientObject.Log.ToArray();
            var earlyServerLog = serverObject.Log.ToArray();

            clientTransport.TestReceiveFromNetwork();

            var finishedAsExpected = doEchoTask.Wait(10000);
            var lateClientLog = clientObject.Log.ToArray();
            var lateServerLog = serverObject.Log.ToArray();

            //-- assert

            finishedTooEarly.ShouldBe(false);
            finishedAsExpected.ShouldBe(true);
            returnValue.ShouldBe("dlrow olleH");

            earlyServerLog.ShouldBe(new[] {
                "ServerApiImplementation.ReverseEcho(Hello world)"
            });
            earlyClientLog.ShouldBeEmpty();

            lateServerLog.ShouldBe(new[] {
                "ServerApiImplementation.ReverseEcho(Hello world)"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IServerApi
        {
            void Ping(string message);
            Task<string> ReverseEcho(string message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IClientApi
        {
            void Pong(string message);
            Promise<string> IdentifyUserMachine();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ServerApiImplementation : IServerApi
        {
            public ServerApiImplementation()
            {
                this.Log = new List<string>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IServerApi.Ping(string message)
            {
                Log.Add("ServerApiImplementation.Ping(" + message + ")");
                
                var client = DuplexNetworkApi.CurrentCall.GetRemotePartyAs<IClientApi>();
                client.Pong("YOU SENT [" + message + "]");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Task<string> IServerApi.ReverseEcho(string message)
            {
                Log.Add("ServerApiImplementation.ReverseEcho(" + message + ")");

                var result = new string(message.Reverse().ToArray(), 0, message.Length);
                return Task.FromResult(result);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<string> Log { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ClientApiImplementation : IClientApi
        {
            private readonly IServerApi _server;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ClientApiImplementation(IDuplexNetworkEndpointTransport transport, IDuplexNetworkApiProxyFactory proxyFactory)
            {
                _server = proxyFactory.CreateProxyInstance<IServerApi, IClientApi>(transport, this);
                this.Log = new List<string>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IClientApi.Pong(string message)
            {
                Log.Add("ClientApiImplementation.Pong(" + message + ")");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Promise<string> IClientApi.IdentifyUserMachine()
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void TestPing(string message)
            {
                _server.Ping(message);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public async Task<string> TestReverseEcho(string message)
            {
                return await _server.ReverseEcho(message);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IServerApi Server
            {
                get { return _server; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<string> Log { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestTransport : IDuplexNetworkEndpointTransport
        {
            private readonly MemoryStream _network;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestTransport(MemoryStream network)
            {
                _network = network;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IDuplexNetworkEndpointTransport

            public void SendBytes(byte[] bytes)
            {
                _network.Write(bytes, 0, bytes.Length);
                _network.Seek(-bytes.Length, SeekOrigin.End);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<byte[]> BytesReceived;
            public event Action<Exception> SendFailed;
            public event Action<Exception> ReceiveFailed;

            #endregion
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void TestReceiveFromNetwork()
            {
                var receivedBuffer = new MemoryStream();
                _network.CopyTo(receivedBuffer);

                if (receivedBuffer.Length > 0)
                {
                    if (BytesReceived != null)
                    {
                        BytesReceived(receivedBuffer.ToArray());
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void TestSendFailed(Exception error)
            {
                if (SendFailed != null)
                {
                    SendFailed(error);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void TestReceiveFailure(Exception error)
            {
                if (ReceiveFailed != null)
                {
                    ReceiveFailed(error);
                }
            }
        }
    }
}
