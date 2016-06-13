using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [Flags]
    public enum LogOptions
    {
        None = 0,
        ThreadLog = 0x01,
        PlainLog = 0x02,
        AuditLog = 0x04,
        AuditLogIfFailure = 0x08,
        CollectCount = 0x10,
        CollectStats = 0x20,
        RetainDetails = 0x40,
        RetainThreadLog = 0x80,
        PublishStats = 0x100,
        ThrowOnFailure = 0x200,
        Aggregate = 0x400,
        AggregateAsDbAccess = LogOptionsExtensions.AggregateAsFlag | 0x0,
        AggregateAsCommunication = LogOptionsExtensions.AggregateAsFlag | 0x1000,
        AggregateAsLockWait = LogOptionsExtensions.AggregateAsFlag | 0x2000,
        AggregateAsLockHold = LogOptionsExtensions.AggregateAsFlag | 0x1000 | 0x2000,
        CompactMode = 0x10000
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public static class LogOptionsExtensions
    {
        public const int AggregateAsIndexDbAccess = 0;
        public const int AggregateAsIndexCommunication = 1;
        public const int AggregateAsIndexLockWait = 2;
        public const int AggregateAsIndexLockHold = 3;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public const int AggregateAsFlag = 0x800;
        public const int AggregateAsMask = AggregateAsFlag | 0x1000 | 0x2000 | 0x4000 | 0x8000;
        public const int AggregateAnyMask = (int)LogOptions.Aggregate | AggregateAsMask;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool HasAggregation(this LogOptions value)
        {
            return (((int)value & AggregateAnyMask) != 0);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool HasAggregateAs(this LogOptions value)
        {
            return (((int)value & AggregateAsFlag) != 0);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static int GetAggregateAsIndex(this LogOptions value)
        {
            return (((int)value & AggregateAsMask) >> 12);
        }
    }
}
