using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NWheels.Logging;

namespace NWheels.Tools.LogViewer
{
    public class ThreadLogViewModel
    {
        public ThreadLogViewModel(ThreadLogSnapshot threadLog)
        {
            this.RootActivity = new NodeItem(threadLog);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeItem RootActivity { get; private set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public class NodeItem : IEnumerable<NodeItem>
        {
            private NodeItem[] _subNodeItems = null;

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

            public NodeItem(ThreadLogSnapshot threadLog)
            {
                this.ThreadLog = threadLog;
                this.ParentNode = null;
                this.LogNode = threadLog.RootActivity;
                this.IsRootActivity = true;
                this.TaskType = threadLog.TaskType;
                this.TimestampText = threadLog.StartedAtUtc.ToString("yyyy-MM-dd HH:mm:ss.fff");

                SetNodeKind();
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

            public NodeItem(ThreadLogSnapshot threadLog, ThreadLogSnapshot.LogNodeSnapshot parentNode, ThreadLogSnapshot.LogNodeSnapshot logNode)
            {
                this.ThreadLog = threadLog;
                this.ParentNode = parentNode;
                this.LogNode = logNode;
                this.IsRootActivity = false;
                this.TaskType = null;
                this.TimestampText = "+ " + logNode.MillisecondsTimestamp;

                SetNodeKind();
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerator<NodeItem> GetEnumerator()
            {
                if ( _subNodeItems == null )
                {
                    var activityNode = (this.LogNode as ThreadLogSnapshot.ActivityNodeSnapshot);

                    if ( activityNode != null && activityNode.SubNodes != null )
                    {
                        _subNodeItems = activityNode.SubNodes.Select(node => new NodeItem(this.ThreadLog, this.LogNode, node)).ToArray();
                    }
                    else
                    {
                        _subNodeItems = new NodeItem[0];
                    }
                }

                return _subNodeItems.AsEnumerable<NodeItem>().GetEnumerator();
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string GetFullDetailsText()
            {
                var log = this.LogNode;
                var activity = (this.LogNode as ThreadLogSnapshot.ActivityNodeSnapshot);
                var text = new StringBuilder();

                text.AppendLine("------ Thread ------");
                text.AppendFormat("ID              = {0}\r\n", this.ThreadLog.LogId);
                text.AppendFormat("Correlation ID  = {0}\r\n", this.ThreadLog.CorrelationId);
                text.AppendFormat("Task Type       = {0}\r\n", this.ThreadLog.TaskType);
                text.AppendFormat("Started at UTC  = {0:yyyy-MM-dd HH:mm:ss.fff}\r\n", this.ThreadLog.StartedAtUtc);
                text.AppendFormat("------ {0} {1} ------\r\n", activity != null ? "Activity" : "Log", log.Level);
                text.AppendFormat("Recorded at UTC = {0:yyyy-MM-dd HH:mm:ss.fff} ({1} ms after thread start)\r\n", 
                    this.ThreadLog.StartedAtUtc.AddMilliseconds(log.MillisecondsTimestamp), log.MillisecondsTimestamp);

                if ( !string.IsNullOrEmpty(log.ExceptionTypeName) )
                {
                    text.AppendFormat("Exception       = {0}\r\n", log.ExceptionTypeName);
                }

                text.AppendLine("------ Text ------");
                text.AppendLine(log.SingleLineText);

                if ( !string.IsNullOrEmpty(log.FullDetailsText) )
                {
                    text.AppendLine("------ Details ------");
                    text.AppendLine(log.FullDetailsText);
                }

                return text.ToString();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ThreadLogSnapshot ThreadLog { get; private set; }
            public ThreadLogSnapshot.LogNodeSnapshot ParentNode { get; private set; }
            public ThreadLogSnapshot.LogNodeSnapshot LogNode { get; private set; }
            public bool IsRootActivity { get; private set; }
            public ThreadTaskType? TaskType { get; private set; }
            public LogNodeKind NodeKind { get; private set; }
            public string TimestampText { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsErrorNode
            {
                get
                {
                    return (LogNode.Level >= LogLevel.Error);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsWarningNode
            {
                get
                {
                    return (LogNode.Level == LogLevel.Warning);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void SetNodeKind()
            {
                var activityNode = (this.LogNode as ThreadLogSnapshot.ActivityNodeSnapshot);

                if ( activityNode != null )
                {
                    if ( IsRootActivity )
                    {
                        this.NodeKind = (
                            activityNode.Level < LogLevel.Warning ? LogNodeKind.ThreadSuccess : 
                            activityNode.Level > LogLevel.Warning ? LogNodeKind.ThreadFailure : 
                            LogNodeKind.ThreadWarning);
                    }
                    else
                    {
                        this.NodeKind = (
                            activityNode.Level < LogLevel.Warning ? LogNodeKind.ActivitySuccess : 
                            activityNode.Level > LogLevel.Warning ? LogNodeKind.ActivityFailure : 
                            LogNodeKind.ActivityWarning);
                    }
                }
                else
                {
                    this.NodeKind = LogLevelToLogNodeKind(this.LogNode.Level);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private LogNodeKind LogLevelToLogNodeKind(LogLevel level)
            {
                switch ( level )
                {
                    case LogLevel.Verbose: 
                        return LogNodeKind.LogVerbose;
                    case LogLevel.Info: 
                        return LogNodeKind.LogInfo;
                    case LogLevel.Warning: 
                        return LogNodeKind.LogWarning;
                    case LogLevel.Error: 
                        return LogNodeKind.LogError;
                    case LogLevel.Critical: 
                        return LogNodeKind.LogCritical;
                    default:
                        return LogNodeKind.LogDebug;
                }
            }
        }
    }
}
