using NWheels.Platform.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Ddd.RestApi
{
    public interface ITxResourceHandlerObjectFactory
    {
        ITxResourceHandlerList CreateResourceHandlerList(Type txType);
    }
}
