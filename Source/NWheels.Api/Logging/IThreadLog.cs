using System;

namespace NWheels.Api.Logging
{
    public interface IThreadLog
    {
        Guid Id { get; }

        Guid CorrelationId { get; }
        
        ThreadTaskType TaskType { get; }

        IActivityLogEvent Activity { get; } 
    }
}
