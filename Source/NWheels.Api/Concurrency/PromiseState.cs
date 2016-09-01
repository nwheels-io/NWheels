using System;
using NWheels.Api.Logging;

namespace NWheels.Api.Concurrency
{
    public enum PromiseState
    {
        Pending,
        InProgress,
        Fulfilled,
        Failed,
        InProgressCancelled,
        PendingAborted,
        TimedOut
    }
}