using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Hosting;
using NWheels.Processing;
using Quartz;
using Quartz.Impl;

namespace NWheels.Stacks.QuartzNet
{
    internal class SchedulerLifecycleManager : LifecycleEventListenerBase
    {
        private readonly IFramework _framework;
        private readonly IJobLogger _logger;
        private readonly IJobDetail[] _jobDetails;
        private readonly AutofacJobFactory _jobFactory;
        private IScheduler _scheduler;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SchedulerLifecycleManager(IFramework framework, AutofacJobFactory jobFactory, IEnumerable<IJobDetail> jobDetails, Auto<IJobLogger> logger)
        {
            _framework = framework;
            _logger = logger.Instance;
            _jobDetails = jobDetails.ToArray();
            _jobFactory = jobFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Load()
        {
            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
            _scheduler.JobFactory = _jobFactory;

            foreach ( var job in _jobDetails )
            {
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(job.Key.Name + ":Timer")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(15).RepeatForever())
                    .Build();

                _logger.RegisteringJob(job.Key.Name);
                _scheduler.ScheduleJob(job, trigger);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeActivated()
        {
            _scheduler.StartDelayed(TimeSpan.FromSeconds(3));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeDeactivating()
        {
            _scheduler.Standby();

            foreach ( var job in _scheduler.GetCurrentlyExecutingJobs() )
            {
                var interruptable = (job.JobInstance as IInterruptableJob);

                if ( interruptable != null )
                {
                    interruptable.Interrupt();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            var deactivationTimeout = TimeSpan.FromSeconds(30);
            var deactivationStartedAt = _framework.UtcNow;

            while ( true )
            {
                var jobsStillRunning = _scheduler.GetCurrentlyExecutingJobs();

                if ( jobsStillRunning.Count == 0 )
                {
                    _logger.AllJobsFinished();
                    break;
                }

                var runningJobNames = string.Join(",", jobsStillRunning.Select(j => j.JobDetail.Key.Name));

                if ( _framework.UtcNow.Subtract(deactivationStartedAt) > deactivationTimeout )
                {
                    _logger.SomeJobsDidNotFinish(runningJobNames);
                    break;
                }

                _logger.WaitingForJobsToFinish(runningJobNames);
                Thread.Sleep(250);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Unload()
        {
            _scheduler.Shutdown(waitForJobsToComplete: false);
        }
    }
}
