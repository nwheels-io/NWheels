using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using NWheels.Extensions;

namespace NWheels.Stacks.MongoDb.Logging
{
    public class LogPersistorFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<MongoDbThreadLogPersistor>().LastInPipeline();
        }

        #endregion
    }
}
