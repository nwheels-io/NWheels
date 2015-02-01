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
            var jobAdapter = (IJob)_components.Resolve(
                bundle.JobDetail.JobType,
                new TypedParameter(typeof(IJobDetail), bundle.JobDetail));

            return jobAdapter;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReturnJob(IJob job)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IJobDetail CreateJobDetail(IApplicationJob job)
        {
            var builder = JobBuilder.Create(typeof(QuartzJobAdapter<>).MakeGenericType(job.GetType()));

            builder.WithIdentity(job.JobId);
            builder.WithDescription(job.Description);

            return builder.Build();
        }
    }
}
