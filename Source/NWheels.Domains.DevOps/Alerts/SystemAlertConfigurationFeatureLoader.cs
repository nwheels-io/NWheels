using Autofac;
using NWheels.Extensions;

namespace NWheels.Domains.DevOps.Alerts
{
    class SystemAlertConfigurationFeatureLoader : Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Configuration().RegisterSection<ISystemAlertConfigurationFeatureSection>();
        }

        #endregion
    }
}
