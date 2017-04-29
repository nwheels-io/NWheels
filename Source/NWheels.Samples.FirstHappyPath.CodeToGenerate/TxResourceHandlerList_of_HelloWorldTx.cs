using NWheels.Frameworks.Ddd.RestApi;
using System;

namespace NWheels.Samples.FirstHappyPath.CodeToGenerate
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
