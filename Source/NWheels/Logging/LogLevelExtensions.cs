using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    internal static class LogLevelExtensions
    {
        public static LogLevel OrHigher(this LogLevel thisLevel, LogLevel otherLevel)
        {
            return (otherLevel > thisLevel ? otherLevel : thisLevel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static LogLevel NoFailureIf(this LogLevel level, bool condition)
        {
            return (level == LogLevel.Error && condition ? LogLevel.Warning : level);
        }
    }
}
