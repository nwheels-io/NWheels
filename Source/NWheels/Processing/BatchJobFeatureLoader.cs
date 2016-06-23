using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;
using NWheels.Processing.Jobs.Core;
using NWheels.Processing.Jobs.Impl;

namespace NWheels.Processing
{
    public class BatchJobFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<SequentialBatchJobScheduler>();
            builder.NWheelsFeatures().Configuration().RegisterSection<IFrameworkApplicationJobConfig>();
            builder.NWheelsFeatures().Logging().RegisterLogger<IApplicationJobLogger>();
        }

        #endregion
    }
}
