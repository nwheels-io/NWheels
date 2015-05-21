using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Hosting;
using NWheels.Processing;
using NWheels.Processing.Jobs;
using Quartz;

namespace NWheels.Puzzle.QuartzNet
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutofacJobFactory>().SingleInstance();
            builder.RegisterAdapter<IApplicationJob, IJobDetail>(AutofacJobFactory.CreateJobDetail);
            builder.RegisterType<SchedulerLifecycleManager>().As<ILifecycleEventListener>().SingleInstance();
            builder.RegisterGeneric(typeof(QuartzJobAdapter<>)).InstancePerDependency();
        }
    }
}
