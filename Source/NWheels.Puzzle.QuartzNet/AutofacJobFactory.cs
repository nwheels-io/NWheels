using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using NWheels.Processing;
using Quartz.Spi;
using Quartz;

namespace NWheels.Puzzle.QuartzNet
{
    internal class AutofacJobFactory : IJobFactory
    {
        private readonly IComponentContext _components;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AutofacJobFactory(IComponentContext components)
        {
            _components = components;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobComponentInstance = (IApplicationJob)_components.Resolve(bundle.JobDetail.JobType);
            var jobAdapter = _components.Resolve<QuartzJobAdapter>(
                new TypedParameter(typeof(IJobDetail), bundle.JobDetail),
                new TypedParameter(typeof(IApplicationJob), jobComponentInstance));

            return jobAdapter;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReturnJob(IJob job)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IJobDetail CreateJobDetail(IApplicationJob job)
        {
            var builder = JobBuilder.Create(job.GetType());

            builder.WithIdentity(job.JobId);
            builder.WithDescription(job.Description);

            return builder.Build();
        }
    }
}
