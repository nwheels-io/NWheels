using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Mechanism.IoC
{
    public interface IPipeline : IEnumerable
    {
        int Count { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPipeline<T> : IPipeline, IEnumerable<T>
    {
        T this[int index] { get; }
        T Propagator { get; }
    }
}
