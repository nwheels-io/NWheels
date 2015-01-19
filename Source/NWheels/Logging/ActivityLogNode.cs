using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWheels.Logging
{
    public abstract class ActivityLogNode : LogNode, ILogActivity
    {
        private IThreadLog _threadLog;
        private ActivityLogNode _parent;
        private LogNode _firstChild = null;
        private LogNode _lastChild = null;
        private bool _isClosed = false;
        private long? _finalMillisecondsDuration = null;
        private Exception _exception = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ActivityLogNode()
            : base(LogContentTypes.PerformanceMeasurement, LogLevel.Info)
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
            var subNodes = new List<ThreadLogSnapshot.LogNodeSnapshot>();

            for ( var child = _firstChild ; child != null ; child = child.NextSibling )
            {
                subNodes.Add(child.TakeSnapshot());
            }

            return new ThreadLogSnapshot.ActivityNodeSnapshot {
                MillisecondsTimestamp = base.MillisecondsTimestamp,
                Level = base.Level,
                ContentTypes = base.ContentTypes,
                SingleLineText = this.SingleLineText,
                FullDetailsText = this.FullDetailsText,
                ExceptionTypeName = (this.Exception != null ? this.Exception.GetType().FullName : null),
                MillisecondsDuration = this.MillisecondsDuration,
                SubNodes = subNodes.ToArray()
            };
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
                return (_finalMillisecondsDuration ?? _threadLog.ElapsedThreadMilliseconds - base.MillisecondsTimestamp);
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
                return (_threadLog != null ? _threadLog.TaskType : base.TaskType);
            }
        }

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

        internal override void AttachToThreadLog(IThreadLog threadLog, ActivityLogNode parent)
        {
            base.AttachToThreadLog(threadLog, parent);

            _parent = parent;
            _threadLog = threadLog;
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
            _finalMillisecondsDuration = _threadLog.ElapsedThreadMilliseconds - base.MillisecondsTimestamp;
            _isClosed = true;

            if ( _parent != null )
            {
                _parent.BubbleActivityResultsFrom(this);
            }
            
            if ( _threadLog != null )
            {
                _threadLog.NotifyActivityClosed(this);
            }
        }
    }
}
