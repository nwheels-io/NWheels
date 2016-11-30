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
    public class DuplexTcpServerFactory
    {
        private readonly IComponentContext _components;
        private readonly IDuplexNetworkApiProxyFactory _proxyFactory;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DuplexTcpServerFactory(IComponentContext components, IDuplexNetworkApiProxyFactory proxyFactory)
        {
            _components = components;
            _proxyFactory = proxyFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DuplexTcpServer<TServerApi, TClientApi> CreateServer<TServerApi, TClientApi>(
            int listenPortNumber,
            int listenBacklog = Int32.MaxValue,
            int maxPendingConnections = Int32.MaxValue,
            int workerThreadCount = 1,
            TimeSpan? clientHeartbeatInterval = null,
            TimeSpan? serverPingInterval = null)
            where TServerApi : class
            where TClientApi : class
        {
            return new DuplexTcpServer<TServerApi, TClientApi>(
                _components,
                _proxyFactory,
                listenPortNumber,
                listenBacklog,
                maxPendingConnections,
                workerThreadCount,
                clientHeartbeatInterval,
                serverPingInterval);
        }
    }
}
