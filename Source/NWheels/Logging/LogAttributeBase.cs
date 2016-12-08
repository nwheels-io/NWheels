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
        protected LogAttributeBase(LogLevel level, bool isActivity, bool isThread, bool isMethodCall)
        {
            this.Level = level;
            this.IsActivity = isActivity;
            this.IsThread = isThread;
            this.IsMethodCall = isMethodCall;
            this.Options = GetDefaultLogOptions(level, isActivity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadTaskType TaskType { get; set; }
        public string SystemAlertId { get; set; }
        public LogLevel Level { get; private set; }
        public LogOptions Options { get; private set; }
        public bool IsActivity { get; private set; }
        public bool IsThread { get; private set; }
        public bool IsMethodCall { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ToThreadLog
        {
            get { return GetLogOption(LogOptions.ThreadLog); }
            set { SetLogOption(LogOptions.ThreadLog, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ToPlainLog
        {
            get { return GetLogOption(LogOptions.PlainLog); }
            set { SetLogOption(LogOptions.PlainLog, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ToAuditLog
        {
            get { return GetLogOption(LogOptions.AuditLog); }
            set { SetLogOption(LogOptions.AuditLog, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ToAuditLogIfFailure
        {
            get { return GetLogOption(LogOptions.AuditLogIfFailure); }
            set { SetLogOption(LogOptions.AuditLogIfFailure, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CollectCount
        {
            get { return GetLogOption(LogOptions.CollectCount); }
            set { SetLogOption(LogOptions.CollectCount, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CollectStats
        {
            get { return GetLogOption(LogOptions.CollectStats); }
            set { SetLogOption(LogOptions.CollectStats, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool RetainDetails
        {
            get { return GetLogOption(LogOptions.RetainDetails); }
            set { SetLogOption(LogOptions.RetainDetails, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool RetainThreadLog
        {
            get { return GetLogOption(LogOptions.RetainThreadLog); }
            set { SetLogOption(LogOptions.RetainThreadLog, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool PublishStats
        {
            get { return GetLogOption(LogOptions.PublishStats); }
            set { SetLogOption(LogOptions.PublishStats, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ThrowOnFailure
        {
            get { return GetLogOption(LogOptions.ThrowOnFailure); }
            set { SetLogOption(LogOptions.ThrowOnFailure, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Aggregate
        {
            get { return GetLogOption(LogOptions.Aggregate); }
            set { SetLogOption(LogOptions.Aggregate, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool AggregateAsDbAccess
        {
            get { return GetLogOption(LogOptions.AggregateAsDbAccess); }
            set { SetLogOption(LogOptions.AggregateAsDbAccess, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool AggregateAsCommunication
        {
            get { return GetLogOption(LogOptions.AggregateAsCommunication); }
            set { SetLogOption(LogOptions.AggregateAsCommunication, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool AggregateAsLockWait
        {
            get { return GetLogOption(LogOptions.AggregateAsLockWait); }
            set { SetLogOption(LogOptions.AggregateAsLockWait, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool AggregateAsLockHold
        {
            get { return GetLogOption(LogOptions.AggregateAsLockHold); }
            set { SetLogOption(LogOptions.AggregateAsLockHold, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool CompactMode
        {
            get { return GetLogOption(LogOptions.CompactMode); }
            set { SetLogOption(LogOptions.CompactMode, value); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool GetLogOption(LogOptions flag)
        {
            return ((Options & flag) != 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetLogOption(LogOptions flag, bool isOn)
        {
            if ( isOn )
            {
                Options |= flag;
            }
            else
            {
                Options &= ~flag;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static LogOptions GetDefaultLogOptions(LogLevel level, bool isActivity)
        {
            var basicOptions = (isActivity ? LogOptions.CollectStats : LogOptions.None);

            switch ( level )
            {
                case LogLevel.Info:
                case LogLevel.Warning:
                case LogLevel.Error:
                    return basicOptions | LogOptions.ThreadLog | LogOptions.PlainLog | LogOptions.CollectCount | LogOptions.RetainDetails;
                case LogLevel.Critical:
                    return basicOptions | LogOptions.ThreadLog | LogOptions.PlainLog | LogOptions.CollectCount | LogOptions.RetainDetails | LogOptions.AuditLog;
                default:
                    return basicOptions | LogOptions.ThreadLog;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogDebugAttribute : LogAttributeBase
    {
        public LogDebugAttribute()
            : base(LogLevel.Debug, isActivity: false, isThread: false, isMethodCall: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogVerboseAttribute : LogAttributeBase
    {
        public LogVerboseAttribute()
            : base(LogLevel.Verbose, isActivity: false, isThread: false, isMethodCall: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogInfoAttribute : LogAttributeBase
    {
        public LogInfoAttribute()
            : base(LogLevel.Info, isActivity: false, isThread: false, isMethodCall: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogWarningAttribute : LogAttributeBase
    {
        public LogWarningAttribute()
            : base(LogLevel.Warning, isActivity: false, isThread: false, isMethodCall: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogErrorAttribute : LogAttributeBase
    {
        public LogErrorAttribute()
            : base(LogLevel.Error, isActivity: false, isThread: false, isMethodCall: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogCriticalAttribute : LogAttributeBase
    {
        public LogCriticalAttribute()
            : base(LogLevel.Critical, isActivity: false, isThread: false, isMethodCall: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogAuditAttribute : LogAttributeBase
    {
        public LogAuditAttribute()
            : base(LogLevel.Audit, isActivity: false, isThread: false, isMethodCall: false)
        {
            base.ToAuditLog = true;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogActivityAttribute : LogAttributeBase
    {
        public LogActivityAttribute(LogLevel level = LogLevel.Verbose)
            : base(level, isActivity: true, isThread: false, isMethodCall: false)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogMethodAttribute : LogAttributeBase
    {
        public LogMethodAttribute(LogLevel level = LogLevel.Verbose, bool throwOnFailure = true)
            : base(level, isActivity: true, isThread: false, isMethodCall: true)
        {
            this.ThrowOnFailure = throwOnFailure;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogThreadAttribute : LogAttributeBase
    {
        public LogThreadAttribute(ThreadTaskType taskType)
            : base(LogLevel.Verbose, isActivity: true, isThread: true, isMethodCall: false)
        {
            base.TaskType = taskType;
        }
    }
}
