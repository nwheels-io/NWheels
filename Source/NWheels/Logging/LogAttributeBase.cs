using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public abstract class LogAttributeBase : Attribute
    {
        protected LogAttributeBase(LogLevel level, bool isActivity)
        {
            this.Level = level;
            this.IsActivity = isActivity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool MustFormatEarly { get; set; }
        public LogLevel Level { get; private set; }
        public bool IsActivity { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogDebugAttribute : LogAttributeBase
    {
        public LogDebugAttribute()
            : base(LogLevel.Debug, isActivity: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogVerboseAttribute : LogAttributeBase
    {
        public LogVerboseAttribute()
            : base(LogLevel.Verbose, isActivity: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogInfoAttribute : LogAttributeBase
    {
        public LogInfoAttribute()
            : base(LogLevel.Info, isActivity: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogWarningAttribute : LogAttributeBase
    {
        public LogWarningAttribute()
            : base(LogLevel.Warning, isActivity: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogErrorAttribute : LogAttributeBase
    {
        public LogErrorAttribute()
            : base(LogLevel.Error, isActivity: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogCriticalAttribute : LogAttributeBase
    {
        public LogCriticalAttribute()
            : base(LogLevel.Critical, isActivity: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogActivityAttribute : LogAttributeBase
    {
        public LogActivityAttribute()
            : base(LogLevel.Info, isActivity: true)
        {
        }
    }
}
