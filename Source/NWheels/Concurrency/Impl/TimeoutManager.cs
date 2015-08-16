using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using NWheels.Hosting;
using Timer = System.Timers.Timer;

namespace NWheels.Concurrency.Impl
{
    internal abstract class RealTimeoutHandle : ITimeoutHandle
    {
        protected RealTimeoutHandle(string timerName, string timerInstanceId, TimeSpan initialDueTime, RealTimeoutManager relatedManager)
        {
            _relatedManager = relatedManager;
            TimerName = timerName;
            TimerInstanceId = timerInstanceId;
            InitialDueTime = initialDueTime;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            CancelTimer();
        }

        #endregion

        #region Implementation of ITimerHandle

        public void ResetDueTime(TimeSpan newInitialDueTime)
        {
            _relatedManager.CancelTimeOutEvent(this);
            InitialDueTime = newInitialDueTime;
            _relatedManager.AddTimeoutEvent(this);
        }

        public string TimerName { get; private set; }
        public string TimerInstanceId { get; private set; }
        public TimeSpan InitialDueTime { get; private set; }
        public DateTime DueTimeUtc { get; internal set; }

        public void CancelTimer()
        {
            _relatedManager.CancelTimeOutEvent(this);
        }

        #endregion

        internal int Id { get; set; }
        private RealTimeoutManager _relatedManager;

        internal abstract void Invoke();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    internal class RealTimeoutHandleNoParam : RealTimeoutHandle
    {
        public Action Callback { get; private set; }
        public RealTimeoutHandleNoParam(string timerName, string timerInstanceId, TimeSpan initialDueTime, Action callback, RealTimeoutManager relatedManager)
            : base(timerName, timerInstanceId, initialDueTime, relatedManager)
        {
            Callback = callback;
        }

        internal override void Invoke()
        {
            Callback();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    internal class RealTimeoutHandle<TParam> : RealTimeoutHandle
    {
        public Action<TParam> Callback { get; private set; }
        public TParam Parameter { get; private set; }

        public RealTimeoutHandle(string timerName, string timerInstanceId, TimeSpan initialDueTime, Action<TParam> callback, TParam parameter, RealTimeoutManager relatedManager)
            : base(timerName, timerInstanceId, initialDueTime, relatedManager)
        {
            Callback = callback;
            Parameter = parameter;
        }

        internal override void Invoke()
        {
            Callback(Parameter);
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    internal class RealTimeoutManager : LifecycleEventListenerBase
    {
        private int _handlersId;
        private Timer _timer;
        private readonly Dictionary<DateTime, Dictionary<int, RealTimeoutHandle>> _timeOutEvents;
        private DateTime _nextTimeToCheck;
        private const int NumOfMsBetweenTicks = 20; // NOTE: The value should be a divisor of 1000. Otherwise the rounding calculation should be changed

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RealTimeoutManager()
        {
            _timeOutEvents = new Dictionary<DateTime, Dictionary<int, RealTimeoutHandle>>();
            _timer = new Timer();
            _timer.Elapsed += TimerFunction;

            _nextTimeToCheck = GetDateInTimeOutResolution(DateTime.UtcNow);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void CancelTimeOutEvent(RealTimeoutHandle handle)
        {
            lock (_timeOutEvents)
            {
                Dictionary<int, RealTimeoutHandle> relevantTimeDictionary;
                if (_timeOutEvents.TryGetValue(handle.DueTimeUtc, out relevantTimeDictionary))
                {
                    relevantTimeDictionary.Remove(handle.Id);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddTimeoutEvent(RealTimeoutHandle timeoutHandle)
        {
            DateTime startTime = DateTime.UtcNow;

            lock (_timeOutEvents)
            {
                // NOTE: Lock is around the time calculation so we will not be interrupted in the middle and might miss this event time!!!
                DateTime fullTimeOutTime = startTime;
                double timeOutInMilliSeconds = timeoutHandle.InitialDueTime.TotalMilliseconds;
                fullTimeOutTime = fullTimeOutTime.AddMilliseconds(timeOutInMilliSeconds);
                int milliSecToRound = NumOfMsBetweenTicks - 1;
                fullTimeOutTime = fullTimeOutTime.AddMilliseconds(milliSecToRound);       // round the calculation up
                DateTime finalTimeOut = GetDateInTimeOutResolution(fullTimeOutTime);
                if (finalTimeOut < _nextTimeToCheck)
                {
                    finalTimeOut = _nextTimeToCheck;
                }
                AddFinalTimeOutEvent(finalTimeOut, timeoutHandle);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private DateTime GetDateInTimeOutResolution(DateTime fullTime)
        {
            int roundedMilliSec = fullTime.Millisecond - (fullTime.Millisecond % NumOfMsBetweenTicks);
            DateTime timeByResolution = new DateTime(
                fullTime.Year,
                fullTime.Month,
                fullTime.Day,
                fullTime.Hour,
                fullTime.Minute,
                fullTime.Second,
                roundedMilliSec);

            return timeByResolution;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        // Should be called under lock(_timeOutEvents)
        private void AddFinalTimeOutEvent(
            DateTime finalTimeOut,
            RealTimeoutHandle timeoutHandle)
        {
            timeoutHandle.DueTimeUtc = finalTimeOut;

            Dictionary<int, RealTimeoutHandle> relevantTimeDictionary;
            if (_timeOutEvents.TryGetValue(finalTimeOut, out relevantTimeDictionary) == false)
            {
                relevantTimeDictionary = new Dictionary<int, RealTimeoutHandle>(1);
                _timeOutEvents.Add(finalTimeOut, relevantTimeDictionary);
            }

            timeoutHandle.Id = Interlocked.Increment(ref _handlersId);
            relevantTimeDictionary.Add(timeoutHandle.Id, timeoutHandle);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void TimerFunction(object sender, ElapsedEventArgs e)
        {
            try
            {
                //DateTime nowRoundedByResolution = GetDateInTimeOutResolution(DateTime.UtcNow);
                DateTime now = DateTime.UtcNow;
                List<RealTimeoutHandle> allTimeOutEventsToInvoke = new List<RealTimeoutHandle>();

                lock (_timeOutEvents)
                {
                    while (_nextTimeToCheck <= now)
                    {
                        Dictionary<int, RealTimeoutHandle> relevantTimeDictionary;
                        if ( _timeOutEvents.TryGetValue(_nextTimeToCheck, out relevantTimeDictionary) )
                        {
                            _timeOutEvents.Remove(_nextTimeToCheck);
                            allTimeOutEventsToInvoke.AddRange(relevantTimeDictionary.Values);
                        }

                        _nextTimeToCheck = _nextTimeToCheck.AddMilliseconds(NumOfMsBetweenTicks);
                    }
                }

                foreach (RealTimeoutHandle handle in allTimeOutEventsToInvoke)
                {
                    ThreadPool.QueueUserWorkItem(Invoke, handle);
                }
            }
            catch (Exception)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Invoke(object realTimeoutHandleObject)
        {
            var timeoutHandle = (RealTimeoutHandle)realTimeoutHandleObject;

            try
            {
                timeoutHandle.Invoke();
            }
            catch ( Exception )
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of LifecycleEventListenerBase

        public override void NodeActivated()
        {
            base.NodeActivated();

            _timer.Interval = NumOfMsBetweenTicks;
            _timer.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeDeactivating()
        {
            base.NodeDeactivating();

            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }

        #endregion
    }
}
