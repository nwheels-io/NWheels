using System;
using NWheels.Api.Logging;

namespace NWheels.Api.Concurrency
{
    public interface ISelectResult
    {
        T As<T>();
    }
}