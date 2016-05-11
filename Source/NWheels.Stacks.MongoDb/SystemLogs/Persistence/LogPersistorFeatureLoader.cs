using Autofac;
using NWheels.Extensions;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
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
