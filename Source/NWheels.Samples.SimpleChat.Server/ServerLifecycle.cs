using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints;
using NWheels.Hosting;
using NWheels.Samples.SimpleChat.Contracts;

namespace NWheels.Samples.SimpleChat.Server
{
    public class ServerLifecycle : LifecycleEventListenerBase
    {
        private readonly DuplexTcpTransport.ApiFactory _tcpFactory;
        private IDuplexNetworkApiEndpoint<IChatServiceApi, IChatClientApi> _endpoint;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ServerLifecycle(DuplexTcpTransport.ApiFactory tcpFactory)
        {
            _tcpFactory = tcpFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LifecycleEventListenerBase

        public override void Activate()
        {
            _endpoint = _tcpFactory.CreateApiServer<IChatServiceApi, IChatClientApi>(
                listenIpAddress: null,
                listenPortNumber: 9797);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            _endpoint.Dispose();
        }

        #endregion
    }
}
