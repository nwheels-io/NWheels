using NWheels.Frameworks.Ddd.RestApi;
using NWheels.Injection;
using System;

namespace NWheels.Samples.FirstHappyPath.HelloService
{
    public class TxResourceHandlerList_of_HelloWorldTx : ITxResourceHandlerList
    {
        public Type[] GetHandlerTypes()
        {
            return new Type[] {
                typeof(TxResourceHandler_of_HelloWorldTx_Hello)
            };
        }
    }
}

