using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Mechanism
{
    public interface IComponentPipe<out T> : IEnumerable<T>
    {
        T GetPropagator(PropagatorOption option);
        T this[int index] { get; }
        int Count { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum PropagatorOption
    {
        StopOnError,
        ContinueOnError
    }
}
