using System;
using System.Threading;
using NWheels.Hosting;
using NWheels.Logging.Impl;

namespace NWheels.Logging.Core
{
    internal class ThreadLog : IThreadLog
    {
        private readonly IThreadRegistry _registry;
        private readonly Guid _logId;
        private readonly Guid _correlationId;
        private readonly INodeConfiguration _node;
        private readonly ThreadTaskType _taskType;
        private readonly DateTime _startedAtUtc;
        private readonly IClock _clock;
        private readonly IThreadLogAnchor _anchor;
        private readonly ulong _startCpuCycles;
        private ActivityLogNode _rootActivity;
        private ActivityLogNode _currentActivity;
        private ulong _cpuTimeMicroseconds;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLog(
            IFramework framework, IClock clock, IThreadRegistry registry, IThreadLogAnchor anchor, ThreadTaskType taskType, ActivityLogNode rootActivity)
        {
            _registry = registry;
            _anchor = anchor;
            _taskType = taskType;
            _rootActivity = rootActivity;
            _currentActivity = rootActivity;
            _startedAtUtc = framework.UtcNow;
            _logId = framework.NewGuid();
            _correlationId = _logId;
            _node = framework.CurrentNode;
            _clock = clock;

            _rootActivity.AttachToThreadLog(this, parent: null);
            _registry.ThreadStarted(this);

            if ( NativeMethods.CpuCyclesPerMillisecond > 0 )
            {
                Thread.BeginThreadAffinity();
                _startCpuCycles = NativeMethods.GetThreadCycles();
            }
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

        public ThreadLogSnapshot TakeSnapshot()
        {
            return new ThreadLogSnapshot {
                EnvironmentName = _node.EnvironmentName,
                NodeName = _node.NodeName,
                NodeInstance = _node.InstanceId,
                MachineName = _s_machineName,
                ProcessId = _s_processId,
                LogId = _logId,
                CorrelationId = _correlationId,
                StartedAtUtc = _startedAtUtc,
                TaskType = _taskType,
                RootActivity = _rootActivity.TakeSnapshot(),
                CpuMicroseconds = CpuTimeMicroseconds
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NWheels.Hosting.INodeConfiguration Node
        {
            get
            {
                return _node; 
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid LogId
        {
            get
            {
                return _logId;
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
                return _clock.ElapsedMilliseconds;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ulong CpuTimeMicroseconds
        {
            get
            {
                return _cpuTimeMicroseconds;
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
            if ( NativeMethods.CpuCyclesPerMillisecond > 0 )
            {
                ulong endCpuCycles = NativeMethods.GetThreadCycles();
                _cpuTimeMicroseconds = 1000 * (endCpuCycles - _startCpuCycles) / NativeMethods.CpuCyclesPerMillisecond;
                
                Thread.EndThreadAffinity();
            }

            _anchor.CurrentThreadLog = null;
            _registry.ThreadFinished(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly static string _s_machineName = System.Environment.MachineName;
        private readonly static int _s_processId = System.Diagnostics.Process.GetCurrentProcess().Id;
    }
}

