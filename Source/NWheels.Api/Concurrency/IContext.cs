using System;

namespace NWheels.Api.Concurrency
{
    public interface IContext
    {
        PromiseState Promise { get; }
        Exception Error { get; }
    }
}
