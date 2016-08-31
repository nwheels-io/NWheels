using System;
using NWheels.Api.Logging;

namespace NWheels.Api.Concurrency
{
    public class SelectResult
    {
        public T As<T>()
        {
            return default(T);
        }
    }
}