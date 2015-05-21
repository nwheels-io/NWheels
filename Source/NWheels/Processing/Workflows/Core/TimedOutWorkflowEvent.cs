using System;

namespace NWheels.Processing.Workflows.Core
{
    /// <summary>
    /// Represents a timeout waiting for an event.
    /// </summary>
    /// <remarks>
    /// The timeout event is dispatched to the subscribers instead of the awaited event which timed out.
    /// Since TimedOutWorkflowEvent returns the same event type and event key as the original awaited event,
    /// it is dispatched on the same path as the original event would.
    /// Subscribers must be prepared to receive TimedOutWorkflowEvent instead of the original event they await.
    /// </remarks>
    public class TimedOutWorkflowEvent : WorkflowEventBase<object>
    {
        private readonly Type _timedOutEventType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TimedOutWorkflowEvent(Type timedOutEventType, object timeOutEventKey)
            : base(timeOutEventKey)
        {
            _timedOutEventType = timedOutEventType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WorkflowEventBase<object>

        /// <summary>
        /// Returns the type of the original awaited event which timed out.
        /// </summary>
        public override Type GetEventType()
        {
            return _timedOutEventType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Always returnes TimedOut status.
        /// </summary>
        public override WorkflowEventStatus GetEventStatus()
        {
            return WorkflowEventStatus.TimedOut;
        }

        #endregion
    }
}
