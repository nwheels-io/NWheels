using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;

namespace NWheels.Endpoints
{
    public static class DuplexNetworkEndpointApiContext
    {
        public static TRemoteApi GetRemotePartyAs<TRemoteApi>()
        {
            return (TRemoteApi)CurrentProxy;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IDuplexNetworkEndpointApiProxy CurrentProxy
        {
            get { return ThreadStaticResourceConsumerScope<IDuplexNetworkEndpointApiProxy>.CurrentResource; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static IDisposable UseProxy(IDuplexNetworkEndpointApiProxy proxy)
        {
            return new ThreadStaticResourceConsumerScope<IDuplexNetworkEndpointApiProxy>(proxy, externallyOwned: true);
        }
    }
}
