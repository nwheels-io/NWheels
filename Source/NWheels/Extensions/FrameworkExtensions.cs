using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Core;
using NWheels.Processing.Messages;
using System.Linq.Expressions;

namespace NWheels.Extensions
{
    public static class FrameworkExtensions
    {
        public static ServiceBusRequestApi<TService> ToService<TService>(this IFramework framework)
            where TService : class
        {
            return new ServiceBusRequestApi<TService>(framework);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ServiceBusRequestApi<TService>
            where TService : class
        {
            private readonly IFramework _framework;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal ServiceBusRequestApi(IFramework framework)
            {
                _framework = framework;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ServiceBusPromiseApi EnqueueRequest(Action<TService> request)
            {
                throw new NotImplementedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ServiceBusPromiseApi
        {
            private readonly IFramework _framework;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal ServiceBusPromiseApi(IFramework framework)
            {
                _framework = framework;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ServiceBusPromiseApi OnSuccess(Action<ServiceBusContinuationApi> continuation)
            {
                //TBD
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ServiceBusPromiseApi OnFailure(Action<ServiceBusContinuationApi> continuation)
            {
                //TBD
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ServiceBusPromiseApi OnFinish(Action<ServiceBusContinuationApi> continuation)
            {
                //TBD
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ServiceBusPromiseApi OnTimeout(TimeSpan timeout, bool abort, Action<ServiceBusContinuationApi> continuation)
            {
                //TBD
                return this;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ServiceBusContinuationApi
        {
            private readonly IFramework _framework;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal ServiceBusContinuationApi(IFramework framework)
            {
                _framework = framework;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ServiceBusRequestApi<TService> ContinueToService<TService>()
                where TService : class
            {
                return new ServiceBusRequestApi<TService>(_framework);
            }
        }
    }
}
