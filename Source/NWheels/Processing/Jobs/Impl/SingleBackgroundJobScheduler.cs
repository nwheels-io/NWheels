using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Core;
using NWheels.Hosting;
using NWheels.Processing.Jobs.Core;

namespace NWheels.Processing.Jobs.Impl
{
    public class SingleBackgroundJobScheduler : LifecycleEventListenerBase
    {
        private readonly object _executionSyncRoot = new object();
        private readonly ICoreFramework _framework;
        private readonly IFrameworkApplicationJobConfig _configuration;
        private readonly IApplicationJobLogger _logger;
        private readonly ApplicationJobBase _job;
        private CancellationTokenSource _cancellationSource;
        private Timer _timer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SingleBackgroundJobScheduler(
            ICoreFramework framework, 
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

            var jobConfig = _configuration.Jobs[_job.JobId];
            _timer = new Timer(OnTimerTick, null, TimeSpan.FromSeconds(5), jobConfig.PeriodicInterval);

            _logger.BackgroundJobStarted(_job.JobId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeDeactivating()
        {
            _timer.Dispose();
            _cancellationSource.Cancel();

            lock (_executionSyncRoot) // wait for current invocation to finish
            {
            }

            _logger.BackgroundJobStopped(_job.JobId);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnTimerTick(object state)
        {
            _framework.RunThreadCode(
                () => {
                    if (_cancellationSource.IsCancellationRequested)
                    {
                        _logger.JobExecutionCanceled(_job.JobId);
                        return;
                    }

                    if (!Monitor.TryEnter(_executionSyncRoot, 10000))
                    {
                        _logger.JobRunSkippedPreviousRunIsStillInProgress(_job.JobId);
                        return;
                    }

                    try
                    {
                        if (_cancellationSource.IsCancellationRequested)
                        {
                            _logger.JobExecutionCanceled(_job.JobId);
                            return;
                        }

                        var clock = Stopwatch.StartNew();

                        try
                        {
                            _job.Execute(new ApplicationJobContext(_cancellationSource.Token, isDryRun: false));
                        }
                        catch (Exception e)
                        {
                            _logger.JobExecutionFailed(_job.JobId, (long)clock.Elapsed.TotalMilliseconds, e);
                            throw;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_executionSyncRoot);
                    }
                },
                () => _logger.ExecutingJob(_job.JobId));
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
