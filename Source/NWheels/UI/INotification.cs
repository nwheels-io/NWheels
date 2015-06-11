using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    /// <summary>
    /// Defines a notification, which is an event that can be broadcast to UI elements by widgets, commands, and behaviors.
    /// </summary>
    public interface INotification
    {
        /// <summary>
        /// Gets qualified string identifier of the notification. This is a unique identifier of this notification type.
        /// </summary>
        string QualifiedName { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Defines a notification, which is an event that can be broadcast to UI elements by widgets, commands, and behaviors.
    /// </summary>
    /// <typeparam name="TPayload">
    /// Type of payload value carried with this notification
    /// </typeparam>
    public interface INotification<out TPayload> : INotification
    {
        /// <summary>
        /// Gets payload value carried with this notification instance.
        /// </summary>
        TPayload Payload { get; }
    }
}
