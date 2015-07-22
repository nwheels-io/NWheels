using System;
using System.Collections.Generic;
using System.Timers;

namespace NWheels.Stacks.Network
{
    public delegate void OnTimeOutDlgt(object dlgtParam);
    public delegate void OnTimeOutInvokerDlgt(OnTimeOutDlgt dlgtToInvoke, object dlgtParam);

    public class TimeOutHandle
    {
        internal DateTime TimeOutTime;
        internal OnTimeOutDlgt OnTimeOutDlgt;
        internal object TimeOutDlgtParam;
        internal int Id;
    }

    ///////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////
    
    public class TimeOutUtils : ITimeOutUtils
    {
        private bool _isRunning;
        private Timer _timer;
        private readonly Dictionary<DateTime, Dictionary<int, TimeOutHandle>> _timeOutEvents;
        private DateTime _lastTimeChecked;
        private OnTimeOutInvokerDlgt _managerTimeOutInvoker;
        private readonly Random _rndObj;
        private TimeOutResolution _timeOutResolution;

        private DateTime GetDateInTimeOutResolutionResolution(DateTime fullTime)
        {
            DateTime timeByResolution;

            switch (_timeOutResolution)
            {
                case TimeOutResolution.Seconds:
                {
                    timeByResolution = new DateTime(
                        fullTime.Year,
                        fullTime.Month,
                        fullTime.Day,
                        fullTime.Hour,
                        fullTime.Minute,
                        fullTime.Second);
                    break;
                }
                /*case TimeOutResolution.MilliSeconds:
                {
                    timeByResolution = new DateTime(
                        fullTime.Year,
                        fullTime.Month,
                        fullTime.Day,
                        fullTime.Hour,
                        fullTime.Minute,
                        fullTime.Second,
                        fullTime.Millisecond);
                }*/
                case TimeOutResolution.TensMilliSeconds:
                {
                    int roundedMilliSec = fullTime.Millisecond - (fullTime.Millisecond % 10);
                    timeByResolution = new DateTime(
                        fullTime.Year,
                        fullTime.Month,
                        fullTime.Day,
                        fullTime.Hour,
                        fullTime.Minute,
                        fullTime.Second,
                        roundedMilliSec);
                    break;
                }
                default:
                {
                    throw new Exception(string.Format("Unhandled TimeOut resolution {0}", _timeOutResolution));
                }
            }

            return timeByResolution;
        }

        ///////////////////////////////////////////////////////////////////////

        // The time passes between the TimerFunction calles
        private int GetTimerInterval()
        {
            switch (_timeOutResolution)
            {
                case TimeOutResolution.Seconds:
                {
                    return 1000;    // every second
                }
                /*case TimeOutResolution.MilliSeconds:
                {
                    return 1;
                }*/
                case TimeOutResolution.TensMilliSeconds:
                {
                    return 10;
                }
                default:
                {
                    throw new Exception(string.Format("Unhandled TimeOut resolution {0}", _timeOutResolution));
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        private void TimerFunction(object sender, ElapsedEventArgs e)
        {
            try
            {
                DateTime nowRoundedByResolution = GetDateInTimeOutResolutionResolution(DateTime.UtcNow);
                List<TimeOutHandle> allTimeOutEventsToInvoke = new List<TimeOutHandle>();

                lock (_timeOutEvents)
                {
                    bool moreToSearch = true;
                    while (moreToSearch)
                    {
                        Dictionary<int, TimeOutHandle> relevantTimeDictionary;
                        if (_timeOutEvents.TryGetValue(_lastTimeChecked, out relevantTimeDictionary))
                        {
                            _timeOutEvents.Remove(_lastTimeChecked);
                            allTimeOutEventsToInvoke.AddRange(relevantTimeDictionary.Values);
                        }

                        if (_lastTimeChecked < nowRoundedByResolution)
                        {
                            _lastTimeChecked = _lastTimeChecked.AddMilliseconds(GetTimerInterval());
                        }
                        else
                        {
                            moreToSearch = false;
                        }
                    }
                }

                foreach (TimeOutHandle h in allTimeOutEventsToInvoke)
                {
                    _managerTimeOutInvoker(h.OnTimeOutDlgt, h.TimeOutDlgtParam);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //-=-= LogUtils.LogError(LogUtils.DefaultLogNamespace, "TimerFunction(): {0}", exp);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        public TimeOutUtils()
        {
            _rndObj = new Random();
            _timeOutEvents = new Dictionary<DateTime, Dictionary<int, TimeOutHandle>>();
            _isRunning = false;
            _timeOutResolution = TimeOutResolution.Seconds;
            _timer = new Timer();

            _timer.Elapsed += TimerFunction;
        }

        ///////////////////////////////////////////////////////////////////////

        public void Run(OnTimeOutInvokerDlgt managerTimeOutInvoker)
        {
            Run(managerTimeOutInvoker, TimeOutResolution.Seconds);
        }

        ///////////////////////////////////////////////////////////////////////

        public void Run(OnTimeOutInvokerDlgt managerTimeOutInvoker, TimeOutResolution timeResolution)
        {
            _managerTimeOutInvoker = managerTimeOutInvoker;
            _timeOutResolution = timeResolution;
            _isRunning = true;
            _lastTimeChecked = GetDateInTimeOutResolutionResolution(DateTime.UtcNow);

            _timer.Interval = GetTimerInterval();
            _timer.Start();
        }

        ///////////////////////////////////////////////////////////////////////

        public TimeOutHandle AddTimeOutEvent(
            UInt32 timeOutInSeconds,
            OnTimeOutDlgt timeOutdlgt,
            object timeOutdlgtParam)
        {
            if (_isRunning == false)
            {
                return null;
            }

            lock (_timeOutEvents)
            {
                // NOTE: Lock is around the time calculation so we will not be interrupted in the middle and might miss this event time!!!
                DateTime fullTimeOutTime = DateTime.UtcNow;
                fullTimeOutTime = fullTimeOutTime.AddSeconds(timeOutInSeconds);
                fullTimeOutTime = fullTimeOutTime.AddMilliseconds(999);       // round the calculation up
                DateTime finalTimeOut = GetDateInTimeOutResolutionResolution(fullTimeOutTime);

                return AddFinalTimeOutEvent(finalTimeOut, timeOutdlgt, timeOutdlgtParam);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        public TimeOutHandle AddMilliSecTimeOutEvent(
            UInt32 timeOutInMilliSeconds,
            OnTimeOutDlgt timeOutdlgt,
            object timeOutdlgtParam)
        {
            if (_isRunning == false)
            {
                return null;
            }

            lock (_timeOutEvents)
            {
                // NOTE: Lock is around the time calculation so we will not be interrupted in the middle and might miss this event time!!!
                DateTime fullTimeOutTime = DateTime.UtcNow;
                fullTimeOutTime = fullTimeOutTime.AddMilliseconds(timeOutInMilliSeconds);
                int milliSecToRound = GetTimerInterval() - 1;
                fullTimeOutTime = fullTimeOutTime.AddMilliseconds(milliSecToRound);       // round the calculation up
                DateTime finalTimeOut = GetDateInTimeOutResolutionResolution(fullTimeOutTime);

                return AddFinalTimeOutEvent(finalTimeOut, timeOutdlgt, timeOutdlgtParam);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        // Should be called under lock(m_TimeOutEvents)
        private TimeOutHandle AddFinalTimeOutEvent(
            DateTime finalTimeOut,
            OnTimeOutDlgt timeOutdlgt,
            object timeOutdlgtParam)
        {
            TimeOutHandle newHandle = new TimeOutHandle();
            newHandle.OnTimeOutDlgt = timeOutdlgt;
            newHandle.TimeOutDlgtParam = timeOutdlgtParam;
            newHandle.TimeOutTime = finalTimeOut;

            Dictionary<int, TimeOutHandle> relevantTimeDictionary;
                if ( _timeOutEvents.TryGetValue(finalTimeOut, out relevantTimeDictionary) == false )
                {
                    relevantTimeDictionary = new Dictionary<int, TimeOutHandle>(1);
                    _timeOutEvents.Add(finalTimeOut, relevantTimeDictionary);
                }

                bool found;
                do
                {
                    newHandle.Id = _rndObj.Next();
                    found = !relevantTimeDictionary.ContainsKey(newHandle.Id);
                } while ( !found );
                relevantTimeDictionary.Add(newHandle.Id, newHandle);

            return newHandle;
        }

        ///////////////////////////////////////////////////////////////////////

        public void CancelTimeOutEvent(TimeOutHandle handle)
        {
            if (handle == null)
            {
                return;
            }
            lock (_timeOutEvents)
            {
                Dictionary<int, TimeOutHandle> relevantTimeDictionary;
                if (_timeOutEvents.TryGetValue(handle.TimeOutTime, out relevantTimeDictionary))
                {
                    relevantTimeDictionary.Remove(handle.Id);
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if ( _timer == null )
            {
                return;
            }
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }

        #endregion
    }

    ///////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This class wraps the TimeOutUtils class by getting a Dispatcher.
    /// This saves some work to the user of this class who wishes to invoke
    /// the timeout delegates via a message dispatcher.
    /// If the dispatcher is null, then the delegates are called in the timer thread.
    /// </summary>
    public class DispatchedTimeOutUtils : TimeOutUtils
    {
        private MessagesDispatcher MyMessagesDispatcher = null;

        private class TimeOutEventInfo
        {
            internal OnTimeOutDlgt DlgtToInvoke;
            internal object DlgtToInvokeParam;
        }

        // This delegate is called in the message dispatcher thread -
        // synchronized with anything else there.
        private void SynchronizedTimeOutInvoker(object o)
        {
            TimeOutEventInfo info = (TimeOutEventInfo)o;
            info.DlgtToInvoke(info.DlgtToInvokeParam);
        }

        // This delegate is called in the timer thread
        private void OnTimeOutInvoker(OnTimeOutDlgt dlgtToInvoke, object dlgtToInvokeParam)
        {
            TimeOutEventInfo info = new TimeOutEventInfo();
            info.DlgtToInvoke = dlgtToInvoke;
            info.DlgtToInvokeParam = dlgtToInvokeParam;

            if (MyMessagesDispatcher != null)
            {
                MyMessagesDispatcher.EnqueueTask(SynchronizedTimeOutInvoker, info);
            }
            else
            {
                dlgtToInvoke(dlgtToInvokeParam);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// All time out delegates will be called using the dispatcher services.
        /// If the dispatcher is null, then the delegates are called in the timer thread.
        /// </summary>
        /// <param name="messagesDispatcher"></param>
        public DispatchedTimeOutUtils(MessagesDispatcher messagesDispatcher) :
            base()
        {
            MyMessagesDispatcher = messagesDispatcher;
        }

        public void Run()
        {
            Run(OnTimeOutInvoker);
        }

        public void Run(TimeOutResolution timeResolution)
        {
            Run(OnTimeOutInvoker, timeResolution);
        }
    }
}
