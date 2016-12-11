using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Processing.Jobs;
using NWheels.Processing.Jobs.Core;
using NWheels.Processing.Jobs.Impl;

namespace NWheels.Processing
{
    public class BackgroundJobFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SingleBackgroundJobScheduler>().InstancePerDependency();
            
            builder.RegisterAdapter<AutofacExtensions.ApplicationJobRegistration, ILifecycleEventListener>((ctx, reg) => {
                var jobComponent = (ApplicationJobBase)ctx.Resolve(reg.JobComponentType);
                return ctx.Resolve<SingleBackgroundJobScheduler>(TypedParameter.From<ApplicationJobBase>(jobComponent));
            }).SingleInstance();

            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkApplicationJobConfig>();
            builder.NWheelsFeatures().Logging().RegisterLogger<IApplicationJobLogger>();
        }

        #endregion
    }
}
