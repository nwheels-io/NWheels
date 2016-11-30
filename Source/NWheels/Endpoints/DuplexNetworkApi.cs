using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;

namespace NWheels.Endpoints
{
    public static class DuplexNetworkApi
    {
        public class InitiateSessionAttribute : Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TerminateSessionAttribute : Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class CurrentCall
        {
            private static readonly Disposable _s_disposable = new Disposable();

            [ThreadStatic]
            private static IDuplexNetworkEndpointApiProxy _s_currentClientProxy;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public static TRemoteApi GetRemotePartyAs<TRemoteApi>()
            {
                return (TRemoteApi)ClientProxy;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            internal static IDuplexNetworkEndpointApiProxy ClientProxy
            {
                get
                {
                    return _s_currentClientProxy;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            internal static IDisposable UseClientProxy(IDuplexNetworkEndpointApiProxy proxy)
            {
                _s_currentClientProxy = proxy;
                return _s_disposable;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private class Disposable : IDisposable
            {
                public void Dispose()
                {
                    _s_currentClientProxy = null;
                }
            }
        }
    }
}
