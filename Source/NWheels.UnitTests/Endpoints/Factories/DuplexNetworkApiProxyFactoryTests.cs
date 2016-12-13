using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Concurrency;
using NWheels.Endpoints;
using NWheels.Endpoints.Factories;
using NWheels.Extensions;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests.Endpoints.Factories
{
    [TestFixture]
    public class DuplexNetworkApiProxyFactoryTests : DynamicTypeUnitTestBase
    {
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

        [Test, Ignore("work in progress")]
        public void AsyncReturnFromServer()
        {
            //-- arrange

            var network = new MemoryStream();
            var serverTransport = new TestTransport(network);
            var clientTransport = new TestTransport(network);

            var proxyFactory = Resolve<IDuplexNetworkApiProxyFactory>();
            var clientObject = new ClientApiImplementation(clientTransport, proxyFactory);
            var serverObject = new ServerApiImplementation();

            var proxyUsedByListenerOnServer = proxyFactory.CreateProxyInstance<IClientApi, IServerApi>(serverTransport, serverObject);
            var proxyUsedOnClient = proxyFactory.CreateProxyInstance<IServerApi, IClientApi>(clientTransport, clientObject);

            string returnValue = null;
            Func<Task> doEcho = async () => {
                await Task.Delay(10);
                returnValue = await proxyUsedOnClient.ReverseEcho("Hello world");
            };

            //-- act

            var doEchoTask = doEcho();
            
            serverTransport.TestReceiveFromNetwork();
            
            var finishedTooEarly = doEchoTask.Wait(250);
            var earlyClientLog = clientObject.Log.ToArray();
            var earlyServerLog = serverObject.Log.ToArray();

            clientTransport.TestReceiveFromNetwork();

            var finishedAsExpected = doEchoTask.Wait(0);
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
            lateClientLog.ShouldBe(new[] {
                "???"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IServerApi
        {
            void Ping(string message);
            Promise<string> ReverseEcho(string message);
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

            Promise<string> IServerApi.ReverseEcho(string message)
            {
                return new string(message.Reverse().ToArray(), 0, message.Length);
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
