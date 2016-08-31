using System;
using System.Collections.Generic;

namespace NWheels.Api.Logging
{
    /// <summary>
    /// Represents an activity log event. Activity has duration and contained events.
    /// </summary> 
    public interface IActivityLogEvent : ILogEvent, IEnumerable<ILogEvent>
    {
        bool Finished { get; }

        TimeSpan Duration { get; }
    }
}
