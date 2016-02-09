using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;
using NWheels.Logging.Core;

namespace NWheels.Testing
{
    public class TestThreadLogAppender : IThreadLogAppender
    {
        private readonly List<LogNode> _logNodes = new List<LogNode>();
        private readonly TestFramework _framework;
        private readonly TestThreadLog _threadLog;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestThreadLogAppender(TestFramework framework)
        {
            _framework = framework;
            _threadLog = new TestThreadLog(framework);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendLogNode(LogNode node)
        {
            _logNodes.Add(node);
            node.AttachToThreadLog(_threadLog, parent: null, indexInLog: 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendActivityNode(ActivityLogNode activity)
        {
            _logNodes.Add(activity);
            activity.AttachToThreadLog(_threadLog, parent: null, indexInLog: 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StartThreadLog(ThreadTaskType taskType, ActivityLogNode rootActivity)
        {
            this.StartedThreadLogIndex = _logNodes.Count;
            this.StartedThreadTaskType = taskType;

            _logNodes.Add(rootActivity);
            rootActivity.AttachToThreadLog(_threadLog, parent: null, indexInLog: 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode[] GetLog()
        {
            return _logNodes.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode[] TakeLog()
        {
            var result = _logNodes.ToArray();

            _logNodes.Clear();
            this.StartedThreadLogIndex = null;
            this.StartedThreadTaskType = null;
            
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] GetLogStrings()
        {
            return GetLog().Select(node => node.SingleLineText).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] TakeLogStrings()
        {
            return TakeLog().Select(node => node.SingleLineText).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int? StartedThreadLogIndex { get; private set; }
        public ThreadTaskType? StartedThreadTaskType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal IThreadLog ThreadLog
        {
            get { return _threadLog; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestThreadLog : IThreadLog
        {
            private readonly TestFramework _framework;
            private readonly Guid _logId;
            private readonly DateTime _startedAtUtc;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestThreadLog(TestFramework framework)
            {
                _framework = framework;
                _logId = framework.NewGuid();
                _startedAtUtc = framework.UtcNow;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IThreadLog Members

            public void NotifyActivityClosed(ActivityLogNode activity)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ThreadLogSnapshot TakeSnapshot()
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Hosting.INodeConfiguration Node
            {
                get
                {
                    return _framework.NodeConfiguration;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ThreadTaskType TaskType
            {
                get
                {
                    return ThreadTaskType.Unspecified;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Guid LogId
            {
                get
                {
                    return _logId;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Guid CorrelationId
            {
                get
                {
                    return _framework.CurrentCorrelationId;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DateTime ThreadStartedAtUtc
            {
                get
                {
                    return _startedAtUtc;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ulong ThreadStartCpuCycles
            {
                get
                {
                    return 0;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public long ElapsedThreadMilliseconds
            {
                get
                {
                    return 0;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ulong UsedThreadCpuCycles
            {
                get
                {
                    return 0;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ActivityLogNode RootActivity
            {
                get { throw new NotImplementedException(); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ActivityLogNode CurrentActivity
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }
    }
}
