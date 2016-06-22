using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Hapil;
using NWheels.Logging.Impl;

namespace NWheels.Logging
{
    public abstract class ActivityLogNode : LogNode, ILogActivity
    {
        public const int MaxTotalValues = 16;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly string DbTotalMessageId = "$total.db";
        public static readonly string CommunicationTotalMessageId = "$total.comm";
        public static readonly string LockWaitTotalMessageId = "$total.wait";
        public static readonly string LockHoldTotalMessageId = "$total.hold";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ActivityLogNode _parent;
        private bool _isCompactMode;
        private LogNode _firstChild = null;
        private LogNode _lastChild = null;
        private bool _isClosed = false;
        private long? _finalMicrosecondsDuration = null;
        private ulong? _finalCpuCycles = null;
        private Exception _exception = null;
        private LogTotal _dbTotal;
        private LogTotal _communicationTotal;
        private LogTotal _lockWaitTotal;
        private LogTotal _lockHoldTotal;
        private Dictionary<string, LogTotal> _totalByMessageId = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ActivityLogNode(string messageId, LogLevel level, LogOptions options)
            : base(messageId, LogContentTypes.PerformanceStats, level, options)
        {
            _isCompactMode = ((options & LogOptions.CompactMode) != 0);
            _dbTotal = new LogTotal(DbTotalMessageId, 0, 0);
            _communicationTotal = new LogTotal(CommunicationTotalMessageId, 0, 0);
            _lockWaitTotal = new LogTotal(LockWaitTotalMessageId, 0, 0);
            _lockHoldTotal = new LogTotal(LockHoldTotalMessageId, 0, 0);
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
            snapshot.MicrosecondsDuration = this.MicrosecondsDuration;
            snapshot.MicrosecondsCpuTime = (long)this.MicrosecondsCpuTime;

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

        public LogTotal[] GetTotals(bool includeBuiltIn = true)
        {
            if (!includeBuiltIn)
            {
                return (_totalByMessageId != null ? _totalByMessageId.Values.ToArray() : null);
            }

            var builtInTotals = new[] { _dbTotal, _communicationTotal, _lockWaitTotal, _lockHoldTotal };

            if (_totalByMessageId == null)
            {
                return builtInTotals;
            }
            else
            {
                return builtInTotals.Concat(_totalByMessageId.Values).ToArray();
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

        public long MicrosecondsDuration
        {
            get
            {
                return (_finalMicrosecondsDuration ?? ThreadLog.ElapsedThreadMicroseconds - base.MicrosecondsTimestamp);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long MillisecondsDuration
        {
            get
            {
                return (this.MicrosecondsDuration / 1000);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ulong MillisecondsCpuTime
        {
            get
            {
                if (ThreadCpuTimeUtility.IsThreadCpuTimeSupported)
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

        public ulong MicrosecondsCpuTime
        {
            get
            {
                if ( ThreadCpuTimeUtility.IsThreadCpuTimeSupported )
                {
                    var usedCpuCycles = (_finalCpuCycles ?? ThreadLog.UsedThreadCpuCycles);
                    return ThreadCpuTimeUtility.GetThreadCpuMicroseconds(usedCpuCycles);
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

        public bool IsCompactMode
        {
            get { return _isCompactMode; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogTotal DbTotal
        {
            get { return _dbTotal; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogTotal CommunicationTotal
        {
            get { return _communicationTotal; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogTotal LockWaitTotal
        {
            get { return _lockWaitTotal; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogTotal LockHoldTotal
        {
            get { return _lockHoldTotal; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Action<ActivityLogNode> Closed { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void BubbleActivityResultsFrom(ActivityLogNode subActivity)
        {
            base.BubbleLogLevelFrom(subActivity.Level);
            base.BubbleContentTypesFrom(subActivity.ContentTypes);
            this.BubbleExceptionFrom(subActivity.Exception);

            if (subActivity.Options.HasAggregation())
            {
                this.IncrementTotal(subActivity.MessageId, 1, (int)subActivity.MillisecondsDuration, subActivity.Options);
            }

            this.BubbleTotalsFrom(subActivity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void BubbleTotalsFrom(ActivityLogNode subActivity)
        {
            LogTotal.Increment(ref _dbTotal, ref subActivity._dbTotal);
            LogTotal.Increment(ref _communicationTotal, ref subActivity._communicationTotal);
            LogTotal.Increment(ref _lockWaitTotal, ref subActivity._lockWaitTotal);
            LogTotal.Increment(ref _lockHoldTotal, ref subActivity._lockHoldTotal);

            if (subActivity._totalByMessageId != null)
            {
                foreach (var subTotal in subActivity._totalByMessageId.Values)
                {
                    IncrementTotal(subTotal.MessageId, subTotal.Count, subTotal.DurationMs);
                }
            }
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

            if (parent != null)
            {
                _isCompactMode |= parent.IsCompactMode;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal virtual bool AppendChildNode(LogNode child, bool clearFailure = false)
        {
            if (_isClosed)
            {
                throw new InvalidOperationException("Cannot append log node to a closed activity.");
            }

            base.BubbleLogOptionsFrom(child.Options);
            base.BubbleLogLevelFrom(child.Level.NoFailureIf(clearFailure));
            base.BubbleContentTypesFrom(child.ContentTypes);

            var isActivity = (child is ActivityLogNode);

            if (!isActivity && child.Options.HasAggregation())
            {
                IncrementTotal(child.MessageId, count: 1, durationMs: 0, messageFlags: child.Options);
            }

            if (!clearFailure)
            {
                BubbleExceptionFrom(child.Exception);
            }

            var shouldInsert = (isActivity || child.IsImportant || !_isCompactMode);

            if (shouldInsert)
            {
                if (_lastChild != null)
                {
                    _lastChild.AttachNextSibling(child);
                }

                _lastChild = child;

                if (_firstChild == null)
                {
                    _firstChild = child;
                }
            }

            return shouldInsert;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void Close()
        {
            _finalMicrosecondsDuration = ThreadLog.ElapsedThreadMicroseconds - base.MicrosecondsTimestamp;
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

        internal void IncrementBuiltinTotal(int asIndex, int count, int durationMs)
        {
            switch (asIndex)
            {
                case LogOptionsExtensions.AggregateAsIndexDbAccess:
                    _dbTotal = _dbTotal.Increment(count, durationMs);
                    break;
                case LogOptionsExtensions.AggregateAsIndexCommunication:
                    _communicationTotal = _communicationTotal.Increment(count, durationMs);
                    break;
                case LogOptionsExtensions.AggregateAsIndexLockWait:
                    _lockWaitTotal = _lockWaitTotal.Increment(count, durationMs);
                    break;
                case LogOptionsExtensions.AggregateAsIndexLockHold:
                    _lockHoldTotal = _lockHoldTotal.Increment(count, durationMs);
                    break;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void IncrementTotal(string messageId, int count, int durationMs, LogOptions messageFlags = LogOptions.None)
        {
            if (messageFlags.HasAggregateAs())
            {
                IncrementBuiltinTotal(messageFlags.GetAggregateAsIndex(), count, durationMs);
            }
            else
            {
                if (_totalByMessageId == null)
                {
                    _totalByMessageId = new Dictionary<string, LogTotal>(capacity: MaxTotalValues);
                }

                LogTotal existingTotal;

                if (_totalByMessageId.TryGetValue(messageId, out existingTotal))
                {
                    _totalByMessageId[messageId] = existingTotal.Increment(count, durationMs);
                }
                else
                {
                    _totalByMessageId[messageId] = new LogTotal(messageId, count, durationMs);
                }
            }
        }
    }
}
