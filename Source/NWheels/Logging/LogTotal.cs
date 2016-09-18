using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    public struct LogTotal
    {
        public LogTotal(string messageId, int count, long microsecondsDuration) 
            : this()
        {
            this.MessageId = messageId;
            this.Count = count;
            this.MicrosecondsDuration = microsecondsDuration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogTotal Increment(int count, long microsecondsDuration)
        {
            return new LogTotal(
                this.MessageId,
                this.Count + count,
                this.MicrosecondsDuration + microsecondsDuration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public readonly string MessageId;
        public readonly int Count;
        public readonly long MicrosecondsDuration;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void Increment(ref LogTotal destination, ref LogTotal source)
        {
            if (source.Count > 0)
            {
                destination = destination.Increment(source.Count, source.MicrosecondsDuration);
            }
        }
    }
}
