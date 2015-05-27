using System;

namespace NWheels.UnitTests.Processing.Workflows.Impl
{
    internal class AwaitEventRequest
    {
        public AwaitEventRequest(Type eventType, object eventKey, TimeSpan timeout)
        {
            this.EventType = eventType;
            this.EventKey = eventKey;
            this.Timeout = timeout;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            var other = obj as AwaitEventRequest;

            if ( other != null )
            {
                return this.ToString().Equals(other.ToString());
            }
            else
            {
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            return EventType.GetHashCode() ^ (EventKey != null ? EventKey.GetHashCode() : 0) ^ Timeout.GetHashCode();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return String.Format(
                "{0}{1},{2}", 
                EventType.Name, EventKey != null ? "[" + EventKey + "]" : "", Timeout);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type EventType { get; private set; }
        public object EventKey { get; private set; }
        public TimeSpan Timeout { get; private set; }
    }
}