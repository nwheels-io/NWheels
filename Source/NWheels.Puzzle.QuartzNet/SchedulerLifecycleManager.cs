using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Processing;
using Quartz;
using Quartz.Impl;

namespace NWheels.Puzzle.QuartzNet
{
    internal class SchedulerLifecycleManager : LifecycleEventListenerBase
    {
        private readonly ILogger _logger;
        private readonly IJobDetail[] _jobDetails;
        private readonly AutofacJobFactory _jobFactory;
        private IScheduler _scheduler;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SchedulerLifecycleManager(AutofacJobFactory jobFactory, IEnumerable<IJobDetail> jobDetails, Auto<ILogger> logger)
        {
            _logger = logger.Instance;
            _jobDetails = jobDetails.ToArray();
            _jobFactory = jobFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Load()
        {
            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
            _scheduler.JobFactory = _jobFactory;

            var trigger = TriggerBuilder.Create()
                .WithIdentity("Timer")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())
                .Build();

            foreach ( var job in _jobDetails )
            {
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Unload()
        {
            _scheduler.Shutdown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogVerbose]
            void RegisteringJob(string name);
        }
    }
}
