﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NWheels.Logging.Impl;

namespace NWheels.Logging
{
    public abstract class ActivityLogNode : LogNode, ILogActivity
    {
        private ActivityLogNode _parent;
        private LogNode _firstChild = null;
        private LogNode _lastChild = null;
        private bool _isClosed = false;
        private long? _finalMillisecondsDuration = null;
        private ulong? _finalCpuCycles = null;
        private Exception _exception = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ActivityLogNode(string messageId, LogLevel level, LogOptions options)
            : base(messageId, LogContentTypes.PerformanceStats, level, options)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ILogActivity.Warn(Exception error)
        {
            if ( _exception == null )
            {
                _exception = error;
            }
            else
            {
                _exception = new AggregateException(_exception, error).Flatten();
            }

            base.BubbleLogLevelFrom(LogLevel.Warning);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ILogActivity.Fail(Exception error)
        {
            if ( _exception == null )
            {
                _exception = error;
            }
            else
            {
                _exception = new AggregateException(_exception, error).Flatten();
            }

            base.BubbleLogLevelFrom(LogLevel.Error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IDisposable.Dispose()
        {
            Close();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ThreadLogSnapshot.LogNodeSnapshot TakeSnapshot()
        {
            var snapshot = base.TakeSnapshot();
            var subNodes = new List<ThreadLogSnapshot.LogNodeSnapshot>(capacity: 16);

            for ( var child = _firstChild ; child != null ; child = child.NextSibling )
            {
                subNodes.Add(child.TakeSnapshot());
            }

            snapshot.SubNodes = subNodes;
            snapshot.IsActivity = true;
            snapshot.Duration = this.MillisecondsDuration;
            snapshot.CpuTime = (long)this.MillisecondsCpuTime;

            return snapshot;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode TryFindLogNode(Func<LogNode, bool> predicate, Func<ActivityLogNode, bool> recursionPredicate = null)
        {
            if (predicate(this))
            {
                return this;
            }

            for (var node = _firstChild ; node != null ; node = node.NextSibling)
            {
                if (predicate(node))
                {
                    return node;
                }

                var activity = (node as ActivityLogNode);

                if (activity != null)
                {
                    if (recursionPredicate == null || recursionPredicate(activity))
                    {
                        return activity.TryFindLogNode(predicate, recursionPredicate);
                    }
                }
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<LogNode> FindLogNodes(Func<LogNode, bool> predicate, Func<ActivityLogNode, bool> recursionPredicate = null)
        {
            if (predicate(this))
            {
                yield return this;
            }

            for (var node = _firstChild; node != null; node = node.NextSibling)
            {
                if (predicate(node))
                {
                    yield return node;
                }

                var activity = (node as ActivityLogNode);

                if (activity != null)
                {
                    if (recursionPredicate == null || recursionPredicate(activity))
                    {
                        foreach (var found in activity.FindLogNodes(predicate, recursionPredicate))
                        {
                            yield return found;
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string GetStatsGroupKey()
        {
            return MessageId;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ActivityLogNode Parent
        {
            get
            {
                return _parent;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode FirstChild
        {
            get
            {
                return _firstChild;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsClosed
        {
            get
            {
                return _isClosed;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsFailure
        {
            get
            {
                return (_isClosed && this.Level >= LogLevel.Error);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsSuccess
        {
            get
            {
                return (_isClosed && this.Level < LogLevel.Error);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long MillisecondsDuration
        {
            get
            {
                return (_finalMillisecondsDuration ?? ThreadLog.ElapsedThreadMilliseconds - base.MillisecondsTimestamp);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ulong MillisecondsCpuTime
        {
            get
            {
                if ( ThreadCpuTimeUtility.IsThreadCpuTimeSupported )
                {
                    var usedCpuCycles = (_finalCpuCycles ?? ThreadLog.UsedThreadCpuCycles);
                    return ThreadCpuTimeUtility.GetThreadCpuMilliseconds(usedCpuCycles);
                }
                else
                {
                    return 0;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ThreadTaskType TaskType
        {
            get
            {
                return (ThreadLog != null ? ThreadLog.TaskType : base.TaskType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Action<ActivityLogNode> Closed { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void BubbleActivityResultsFrom(ActivityLogNode subActivity)
        {
            base.BubbleLogLevelFrom(subActivity.Level);
            base.BubbleContentTypesFrom(subActivity.ContentTypes);
            this.BubbleExceptionFrom(subActivity.Exception);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void BubbleExceptionFrom(Exception subNodeException)
        {
            if ( this._exception == null && subNodeException != null )
            {
                this._exception = subNodeException;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal override void AttachToThreadLog(IThreadLog threadLog, ActivityLogNode parent, int indexInLog)
        {
            base.AttachToThreadLog(threadLog, parent, indexInLog);
            _parent = parent;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal virtual void AppendChildNode(LogNode child, bool clearFailure = false)
        {
            if ( _isClosed )
            {
                throw new InvalidOperationException("Cannot append log node to a closed activity.");
            }

            base.BubbleLogLevelFrom(child.Level.NoFailureIf(clearFailure));
            base.BubbleContentTypesFrom(child.ContentTypes);

            if ( !clearFailure )
            {
                BubbleExceptionFrom(child.Exception);
            }

            if ( _lastChild != null )
            {
                _lastChild.AttachNextSibling(child);
            }

            _lastChild = child;

            if ( _firstChild == null )
            {
                _firstChild = child;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void Close()
        {
            _finalMillisecondsDuration = ThreadLog.ElapsedThreadMilliseconds - base.MillisecondsTimestamp;
            _finalCpuCycles = ThreadLog.UsedThreadCpuCycles - base.CpuCyclesTimestamp;
            _isClosed = true;

            if ( _parent != null )
            {
                _parent.BubbleActivityResultsFrom(this);
            }

            if ( Closed != null )
            {
                Closed(this);
            }

            if ( ThreadLog != null )
            {
                ThreadLog.NotifyActivityClosed(this);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                new LogNameValuePair<long> { Name = "$duration", Value = this.MillisecondsDuration },
                new LogNameValuePair<ulong> { Name = "$cputime", Value = this.MillisecondsCpuTime },
            });
        }
    }
}
