using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;
using NWheels.Extensions;

namespace NWheels.Tools.LogViewerWeb.App
{
    public class LogNodeViewModel
    {
        public void PopulateFrom(ThreadLogSnapshot.LogNodeSnapshot source)
        {
            this.Type = (source.IsActivity ? LogNodeType.Activity : LogNodeType.Log);
            this.Icon = LogNodeIcon.None;//TODO
            this.Level = source.Level;
            this.Message = GetTextFromMessageId(source.MessageId);

            if ( source.NameValuePairs != null && source.NameValuePairs.Any(p => !p.IsDetail) )
            {
                this.Message += ": " + string.Join(", ", source.NameValuePairs.Where(p => !p.IsDetail).Select(p => p.Name + "=" + p.Value));
            }

            this.Timestamp = "+ " + source.MillisecondsTimestamp.ToString();

            if ( source.IsActivity )
            {
                this.Duration = (int)source.Duration;
            }

            if ( source.SubNodes != null )
            {
                this.SubNodes = source.SubNodes.Select(subSource => {
                    var subModel = new LogNodeViewModel();
                    subModel.PopulateFrom(subSource);
                    return subModel;
                }).ToArray();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNodeType Type { get; set; }
        public LogNodeIcon Icon { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
        public int? Duration { get; set; }
        public int? CpuTime { get; set; }
        public int? DbCount { get; set; }
        public int? DbDuration { get; set; }
        public int? LockWaitCount { get; set; }
        public int? LockWaitDuration { get; set; }
        public int? LockHoldCount { get; set; }
        public int? LockHoldDuration { get; set; }
        public LogNodeViewModel[] SubNodes { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetTextFromMessageId(string messageId)
        {
            var lastDotPosition = messageId.LastIndexOf('.');

            if ( lastDotPosition >= 0 && lastDotPosition < messageId.Length - 1 )
            {
                return messageId.Substring(lastDotPosition + 1).SplitPascalCase();
            }
            else
            {
                return messageId.SplitPascalCase();
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ThreadNodeViewModel : LogNodeViewModel
    {
        public void PopulateFrom(ThreadLogSnapshot source)
        {
            this.TimestampValue = source.StartedAtUtc.Ticks;
            this.Environment = source.EnvironmentName;
            this.Node = source.NodeName;
            this.LogId = source.LogId.ToString("N");
            this.ThreadType = source.TaskType.ToString().SplitPascalCase();
            this.CorrelationId = source.CorrelationId.ToString("N");

            PopulateFrom(source.RootActivity);

            this.Type = LogNodeType.Thread;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long TimestampValue { get; set; }
        public string Environment { get; set; }
        public string Node { get; set; }
        public string LogId { get; set; }
        public string ThreadType { get; set; }
        public string CorrelationId { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum LogNodeType
    {
        Thread,
        Activity,
        Log
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum LogNodeIcon
    {
        None,
        Info,
        Warning,
        Error,
        MessageEnvelope,
        DataTable
    }
}
