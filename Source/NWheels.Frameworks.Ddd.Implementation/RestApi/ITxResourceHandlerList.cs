using NWheels.Platform.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Ddd.RestApi
{
    public interface ITxResourceHandlerList
    {
        Type[] GetHandlerTypes();
    }
}
