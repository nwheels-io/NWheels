using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Logging;

namespace NWheels.Tools.LogViewer
{
    public class ThreadLogViewModel
    {
        public ThreadLogViewModel(ThreadLogSnapshot threadLog)
        {
            this.RootActivity = new NodeItem(threadLog.RootActivity, isRootActivity: true, taskType: threadLog.TaskType);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public NodeItem RootActivity { get; private set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public class NodeItem : IEnumerable<NodeItem>
        {
            private NodeItem[] _subNodeItems = null;

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

            public NodeItem(ThreadLogSnapshot.LogNodeSnapshot logNode, bool isRootActivity = false, ThreadTaskType? taskType = null)
            {
                this.LogNode = logNode;
                this.IsRootActivity = isRootActivity;
                this.TaskType = taskType;

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
                        _subNodeItems = activityNode.SubNodes.Select(node => new NodeItem(node)).ToArray();
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

            public ThreadLogSnapshot.LogNodeSnapshot LogNode { get; private set; }
            public bool IsRootActivity { get; private set; }
            public ThreadTaskType? TaskType { get; private set; }
            public LogNodeKind NodeKind { get; private set; }

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
