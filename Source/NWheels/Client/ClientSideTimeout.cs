using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;

namespace NWheels.Client
{
    public class ClientSideTimeout<TParam> : ITimeoutHandle
    {
        private readonly Action<TParam> _callback;
        private readonly TParam _parameter;
        private ClientSideFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ClientSideTimeout(
            ClientSideFramework framework,
            string timerName,
            string timerInstanceId,
            TimeSpan initialDueTime,
            Action<TParam> callback,
            TParam parameter)
        {
            _callback = callback;
            _parameter = parameter;
            _framework = framework;

            this.TimerName = timerName;
            this.TimerInstanceId = timerInstanceId;
            this.DueTimeUtc = framework.UtcNow.Add(initialDueTime);
            this.InitialDueTime = initialDueTime;
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

        public void ResetDueTime(TimeSpan newInitialDueTime)
        {
            DueTimeUtc = _framework.UtcNow.Add(newInitialDueTime);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string TimerName { get; private set; }
        public string TimerInstanceId { get; private set; }
        public TimeSpan InitialDueTime { get; private set; }
        public DateTime DueTimeUtc { get; set; }

        public void CancelTimer()
        {
            IsCancelled = true;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsDisposed { get; private set; }
        public bool IsCancelled { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void PerformTick()
        {
            _callback(_parameter);
        }
    }
}
