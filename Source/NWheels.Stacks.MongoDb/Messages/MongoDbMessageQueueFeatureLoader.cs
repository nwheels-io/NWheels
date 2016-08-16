using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Endpoints.Core;
using NWheels.Extensions;

namespace NWheels.Stacks.MongoDb.Messages
{
    public class MongoDbMessageQueueFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Configuration().RegisterSection<PollingMongoDbMessageQueueEndpoint.IConfig>();
            builder.NWheelsFeatures().Logging().RegisterLogger<PollingMongoDbMessageQueueEndpoint.ILogger>();
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<PollingMongoDbMessageQueueEndpoint>().As<IEndpoint>();
        }

        #endregion
    }
}
