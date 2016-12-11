using NWheels.Configuration;

namespace NWheels.Domains.DevOps.Alerts
{
    [ConfigurationSection(XmlName = "SystemAlerts")]
    public interface ISystemAlertConfigurationFeatureSection : IConfigurationSection
    {
        INamedObjectCollection<ISystemAlertConfigurationElement> SystemAlerts { get; }
    }

    [ConfigurationElement(XmlName = "SystemAlert")]
    public interface ISystemAlertConfigurationElement : INamedConfigurationElement
    {
    }
}
