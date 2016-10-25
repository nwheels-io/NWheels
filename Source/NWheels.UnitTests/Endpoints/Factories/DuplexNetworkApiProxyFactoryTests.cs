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

namespace NWheels.UnitTests.Endpoints.Factories
{
    [TestFixture]
    public class DuplexNetworkApiProxyFactoryTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void PingPong()
        {
            //-- arrange

            var proxyFactory = Resolve<IDuplexNetworkApiProxyFactory>();
            var transport = new TestTransport();
            
            var clientObject = new ClientApiImplementation();
            var proxyUsedOnClient = proxyFactory.CreateProxyInstance<IServerApi, IClientApi>(transport, clientObject);

            var serverObject = new ServerApiImplementation();
            var proxyUsedOnServer = proxyFactory.CreateProxyInstance<IClientApi, IServerApi>(transport, serverObject);


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
            #region Implementation of IServerApi

            public void Ping(string message)
            {
                DuplexNetworkEndpointApiContext.GetRemotePartyAs<IClientApi>().Pong("YOU SENT: " + message);
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ClientApiImplementation : IClientApi
        {
            #region Implementation of IClientApi

            public void Pong(string message)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestTransport : IDuplexNetworkEndpointTransport
        {
            #region Implementation of IDuplexNetworkEndpointTransport

            public void SendBytes(byte[] bytes)
            {
                //
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<byte[]> BytesReceived;
            public event Action<Exception> SendFailed;
            public event Action<Exception> ReceiveFailed;

            #endregion
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void TestReceiveBytes(byte[] bytes)
            {
                if (BytesReceived != null)
                {
                    BytesReceived(bytes);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void TestSendFailure(Exception error)
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
