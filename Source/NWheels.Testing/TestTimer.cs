using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;

namespace NWheels.Testing
{
    public class TestTimer<TParam> : ITimerHandle
    {
        private readonly Action<TParam> _callback;
        private readonly TParam _parameter;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestTimer(
            TestFramework framework,
            string timerName,
            string timerInstanceId,
            TimeSpan initialDueTime,
            TimeSpan? recurringPeriod,
            Action<TParam> callback,
            TParam parameter)
        {
            _callback = callback;
            _parameter = parameter;

            this.TimerName = timerName;
            this.TimerInstanceId = timerInstanceId;
            this.DueTimeUtc = framework.UtcNow.Add(initialDueTime);
            this.InitialDueTime = initialDueTime;
            this.RecurringPeriod = recurringPeriod;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IDisposable

        public void Dispose()
        {
            IsDisposed = true;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ITimerHandle

        public void Expand(TimeSpan delta)
        {
            DueTimeUtc = DueTimeUtc.Add(delta);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Expand(int deltaMilliseconds)
        {
            DueTimeUtc = DueTimeUtc.AddMilliseconds(deltaMilliseconds);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string TimerName { get; private set; }
        public string TimerInstanceId { get; private set; }
        public TimeSpan InitialDueTime { get; private set; }
        public TimeSpan? RecurringPeriod { get; private set; }
        public DateTime DueTimeUtc { get; set; }
        public int TickCount { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsDisposed { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void PerformTick()
        {
            TickCount++;
            _callback(_parameter);
        }
    }
}
