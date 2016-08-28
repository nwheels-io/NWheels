using System;
using System.Collections.Generic;

namespace NWheels.Api.Logging
{
    /// <summary>
    /// Represents an event written to or read from a log stream.
    /// </summary> 
    public interface ILogEvent
    {
        /// <summary>
        /// Gets event id, which is a concatenation in the format logger_name:event_name.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets severity level of the event.
        /// </summary>
        LogLevel Level { get; }

        /// <summary>
        /// Gets UTC timestamp of the event
        /// </summary>
        DateTime Utc { get; }

        /// <summary>
        /// Values written with the event
        /// </summary>
        IDictionary<string, object> Values { get; }
    }
}
