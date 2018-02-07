using System;
using System.Collections.Generic;

namespace NWheels.Configuration.Api
{
    public interface IConfigElementList<T> : IList<T>
    {
        T NewItem();
    }
}
