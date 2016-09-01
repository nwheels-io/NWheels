using System;
using NWheels.Api.Concurrency;

namespace NWheels.Api
{
    public interface IFramework
    {
        DateTime UtcNow { get; }
        IScheduler Scheduler { get; }
    }
}
