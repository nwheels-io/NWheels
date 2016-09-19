using NWheels.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    public enum SummaryLogLevel
    {
        Positive,
        Negative
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class SummaryLogLevelExtensions
    {
        public static SummaryLogLevel ToSummaryLogLevel(this LogLevel level)
        {
            return (level >= LogLevel.Warning ? SummaryLogLevel.Negative : SummaryLogLevel.Positive);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static int SignFactor(this LogLevel level)
        {
            return (level.ToSummaryLogLevel() == SummaryLogLevel.Positive ? 1 : -1);
        }
    }
}
