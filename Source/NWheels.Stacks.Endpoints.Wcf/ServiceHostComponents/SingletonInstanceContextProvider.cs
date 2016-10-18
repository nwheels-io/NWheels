using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;

namespace NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents
{
    public class SingletonInstanceContextProvider : IInstanceContextProvider
    {
        private readonly ServiceHostBase _serviceHost;
        private readonly object _instanceContextSyncRoot = new object();
        private InstanceContext _instanceContext;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public SingletonInstanceContextProvider(ServiceHostBase serviceHost)
        {
            _serviceHost = serviceHost;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            if ( _instanceContext == null )
            {
                if ( !Monitor.TryEnter(_instanceContextSyncRoot, timeout: TimeSpan.FromSeconds(10)) )
                {
                    throw new TimeoutException("Could not acquire lock on instance context withing allotted timeout.");
                }

                if ( _instanceContext != null )
                {
                    Monitor.Exit(_instanceContextSyncRoot);
                }
            }

            return _instanceContext;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
            _instanceContext = instanceContext;
            Monitor.Exit(_instanceContextSyncRoot);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsIdle(InstanceContext instanceContext)
        {
            return false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
        }
    }
}