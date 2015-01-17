using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Core.Logging
{
    internal class ThreadLog : IThreadLog
    {
        private readonly ThreadLogRegistry _registry;
        private readonly ThreadTaskType _taskType;
        private readonly DateTime _startedAtUtc;
        private readonly Guid _correlationId;
        private readonly Stopwatch _watch;
        private ActivityLogNode _rootActivity;
        private ActivityLogNode _currentActivity;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLog(IFramework framework, ThreadLogRegistry registry, ThreadTaskType taskType, ActivityLogNode rootActivity)
        {
            _registry = registry;
            _taskType = taskType;
            _rootActivity = rootActivity;
            _currentActivity = rootActivity;
            _startedAtUtc = framework.UtcNow;
            _correlationId = framework.NewGuid();

            _watch = Stopwatch.StartNew();

            _rootActivity.AttachToThreadLog(this, parent: null);
            _registry.ThreadLogStarted(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendNode(LogNode node, bool clearFailure = false)
        {
            node.AttachToThreadLog(this, _currentActivity);
            _currentActivity.AppendChildNode(node, clearFailure);

            var nodeAsActivity = (node as ActivityLogNode);

            if ( nodeAsActivity != null )
            {
                _currentActivity = nodeAsActivity;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NotifyActivityClosed(ActivityLogNode activity)
        {
            if ( activity != _currentActivity )
            {
                throw new InvalidOperationException("Cannot close actvity because it is not the current activity at the moment.");
            }

            if ( activity.Parent != null )
            {
                _currentActivity = activity.Parent;
            }
            else
            {
                Close();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid CorrelationId
        {
            get
            {
                return _correlationId;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadTaskType TaskType
        {
            get
            {
                return _taskType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime ThreadStartedAtUtc
        {
            get
            {
                return _startedAtUtc;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long ElapsedThreadMilliseconds
        {
            get
            {
                return _watch.ElapsedMilliseconds;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ActivityLogNode RootActivity
        {
            get
            {
                return _rootActivity;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ActivityLogNode CurrentActivity
        {
            get
            {
                return _currentActivity;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Close()
        {
            _registry.ThreadLogFinished(this);
        }
    }
}

