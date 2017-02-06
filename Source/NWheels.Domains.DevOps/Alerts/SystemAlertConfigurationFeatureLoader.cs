using Autofac;
using NWheels.Domains.DevOps.Alerts.Entities;
using NWheels.Extensions;

namespace NWheels.Domains.DevOps.Alerts
{
    public class SystemAlertConfigurationFeatureLoader : Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Configuration().RegisterSection<ISystemAlertConfigurationFeatureSection>();

            builder.NWheelsFeatures().ObjectContracts().Concretize<ISystemAlertConfigurationEntity>().With<SystemAlertConfigurationEntity>();
            builder.NWheelsFeatures().ObjectContracts().Concretize<IEntityPartEmailAddressRecipient>().With<EntityPartEmailAddressRecipient>();
            builder.NWheelsFeatures().ObjectContracts().Concretize<IEntityPartUserAccountEmailRecipient>().With<EntityPartUserAccountEmailRecipient>();
            builder.NWheelsFeatures().ObjectContracts().Concretize<IEntityPartAlertByEmail>().With<EntityPartAlertByEmail>();
        }

        #endregion
    }
}
