using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Endpoints;
using NWheels.Endpoints.Factories;
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

        public interface IServerApi
        {
            void Ping(string message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IClientApi
        {
            void Pong(string message);
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
