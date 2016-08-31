using System;
using System.Collections.Generic;

namespace NWheels.Api.Components
{
    public interface IPipeline<T> : IEnumerable<T>
    {
        T GetInvocationProxy();
        int Count { get; }

        T this[int index] { get; }
    }
}