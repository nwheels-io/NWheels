using System;
using System.Diagnostics;
using System.Threading;
using NWheels.Concurrency;
using NWheels.Hosting;
using NWheels.Processing.Jobs.Core;

namespace NWheels.Processing.Jobs.Impl
{
    public class SingleBackgroundJobScheduler : LifecycleEventListenerBase
    {
        private readonly object _executionSyncRoot = new object();
        private readonly IFramework _framework;
        private readonly IFrameworkApplicationJobConfig _configuration;
        private readonly IApplicationJobLogger _logger;
        private readonly ApplicationJobBase _job;
        private CancellationTokenSource _cancellationSource;
        private ITimeoutHandle _timeoutHandle;
        private IFrameworkSingleJobConfig _jobConfig;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SingleBackgroundJobScheduler(
            IFramework framework, 
            IFrameworkApplicationJobConfig configuration, 
            IApplicationJobLogger logger, 
            ApplicationJobBase job)
        {
            _job = job;
            _framework = framework;
            _configuration = configuration;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LifecycleEventListenerBase

        public override void NodeActivated()
        {
            _cancellationSource = new CancellationTokenSource();

            _jobConfig = _configuration.Jobs[_job.JobId];
            _timeoutHandle = _framework.NewTimer(_job.JobId, string.Empty, TimeSpan.FromSeconds(5), ScheduledJobAction);

            _logger.BackgroundJobStarted(_job.JobId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeDeactivating()
        {
            _cancellationSource.Cancel();
            _timeoutHandle.CancelTimer();

            lock (_executionSyncRoot) // wait for current invocation to finish
            {
            }

            _logger.BackgroundJobStopped(_job.JobId);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ScheduledJobAction()
        {

            if (_cancellationSource.IsCancellationRequested)
            {
                _logger.JobExecutionCanceled(_job.JobId);
                return;
            }

            var clock = Stopwatch.StartNew();

            try
            {
                Monitor.Enter(_executionSyncRoot);

                if (!_cancellationSource.IsCancellationRequested)
                {
                    _logger.ExecutingJob(_job.JobId);
                    _job.Execute(new ApplicationJobContext(_cancellationSource.Token, isDryRun: false));
                }

            }
            catch (Exception e)
            {
                _logger.JobExecutionFailed(_job.JobId, (long)clock.Elapsed.TotalMilliseconds, e);
                throw;
            }
            finally
            {
                if (!_cancellationSource.IsCancellationRequested)
                {
                    _timeoutHandle.ResetDueTime(_jobConfig.PeriodicInterval);
                }

                Monitor.Exit(_executionSyncRoot);
            }
        }
       
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ApplicationJobContext : IApplicationJobContext
        {
            public ApplicationJobContext(CancellationToken cancellation, bool isDryRun)
            {
                Cancellation = cancellation;
                IsDryRun = isDryRun;
            }

            #region Implementation of IApplicationJobContext

            public void Report(string statusText, decimal? percentCompleted)
            {
            }

            public bool IsDryRun { get; private set; }
            public CancellationToken Cancellation { get; private set; }

            #endregion
        }
    }
}
