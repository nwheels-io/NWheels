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
        public void PopulateFrom(ThreadLogSnapshot.LogNodeSnapshot source, DateTime threadStartedAt, ref int nodeId)
        {
            this.Id = nodeId++;
            this.Type = (source.IsActivity ? LogNodeType.Activity : LogNodeType.Log);
            this.Icon = LogNodeIcon.None;//TODO
            this.Level = source.Level;
            this.MessageId = source.MessageId;
            this.Text = GetTextFromMessageId(source.MessageId);
            this.ExceptionDetails = source.ExceptionDetails;

            if ( source.NameValuePairs != null && source.NameValuePairs.Any(p => !p.IsDetail) )
            {
                this.Text += ": " + string.Join(", ", source.NameValuePairs.Where(p => !p.IsDetail).Select(p => p.Name + "=" + p.Value));
            }

            this.TimeText = "+ " + ((decimal)source.MicrosecondsDuration / 1000m).ToString("#,##0.00");
            this.Timestamp = threadStartedAt.AddMilliseconds(source.MicrosecondsTimestamp / 1000).ToString("yyyy-MM-dd HH:mm:ss.fff");
            this.NameValuePairs = source.NameValuePairs;

            if ( source.IsActivity )
            {
                this.Duration = (int)(source.MicrosecondsDuration / 1000);
            }

            if ( source.SubNodes != null )
            {
                var subNodeId = nodeId;

                this.SubNodes = source.SubNodes.Select(subSource => {
                    var subModel = new LogNodeViewModel();
                    subModel.PopulateFrom(subSource, threadStartedAt, ref subNodeId);
                    return subModel;
                }).ToArray();

                nodeId = subNodeId;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int Id { get; set; }
        public LogNodeType Type { get; set; }
        public LogNodeIcon Icon { get; set; }
        public LogLevel Level { get; set; }
        public string MessageId { get; set; }
        public string Text { get; set; }
        public string ExceptionDetails { get; set; }
        public string Timestamp { get; set; }
        public string TimeText { get; set; }
        public int? Duration { get; set; }
        public int? CpuTime { get; set; }
        public int? DbCount { get; set; }
        public int? DbDuration { get; set; }
        public int? LockWaitCount { get; set; }
        public int? LockWaitDuration { get; set; }
        public int? LockHoldCount { get; set; }
        public int? LockHoldDuration { get; set; }
        public IList<ThreadLogSnapshot.NameValuePairSnapshot> NameValuePairs { get; set; }
        public IList<LogNodeViewModel> SubNodes { get; set; }

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
            this.ThreadType = source.TaskType;
            this.CorrelationId = source.CorrelationId.ToString("N");

            int nodeId = 1;
            PopulateFrom(source.RootActivity, source.StartedAtUtc, ref nodeId);

            this.Type = LogNodeType.Thread;
            this.TimeText = source.StartedAtUtc.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long CaptureId { get; set; }
        public long TimestampValue { get; set; }
        public string Environment { get; set; }
        public string Node { get; set; }
        public string LogId { get; set; }
        public ThreadTaskType ThreadType { get; set; }
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
