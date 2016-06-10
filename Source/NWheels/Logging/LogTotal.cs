using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    public struct LogTotal
    {
        public LogTotal(string messageId, int count, int durationMs) 
            : this()
        {
            this.MessageId = messageId;
            this.Count = count;
            this.DurationMs = durationMs;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogTotal Increment(int count, int durationMs)
        {
            return new LogTotal(
                this.MessageId,
                this.Count + count,
                this.DurationMs + durationMs);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public readonly string MessageId;
        public readonly int Count;
        public readonly int DurationMs;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Increment(ref LogTotal destination, ref LogTotal source)
        {
            if (source.Count > 0)
            {
                destination = destination.Increment(source.Count, source.DurationMs);
            }
        }
    }
}
