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
        private readonly DuplexTcpServerFactory _tcpServerFactory;
        private DuplexTcpServer<IChatServiceApi, IChatClientApi> _tcpServer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ServerLifecycle(DuplexTcpServerFactory tcpServerFactory)
        {
            _tcpServerFactory = tcpServerFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LifecycleEventListenerBase

        public override void Activate()
        {
            _tcpServer = _tcpServerFactory.CreateServer<IChatServiceApi, IChatClientApi>(
                listenPortNumber: 9797,
                workerThreadCount: 1,
                serverPingInterval: TimeSpan.FromSeconds(1),
                serverObjectFactory: (tcp, client) => new ChatService(/*tcp, client*/));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            _tcpServer.Dispose();
        }

        #endregion
    }
}
