using NWheels.Configuration;

namespace NWheels.Domains.DevOps.Alerts
{
    [ConfigurationSection(XmlName = "SystemAlerts")]
    public interface ISystemAlertConfigurationFeatureSection : IConfigurationSection
    {
        INamedObjectCollection<ISystemAlertConfigurationElement> AlertList { get; }
    }

    [ConfigurationElement(XmlName = "SystemAlert")]
    public interface ISystemAlertConfigurationElement : INamedConfigurationElement
    {
        string Description { get; set; }
        string PossibleReason { get; set; }
        string SuggestedAction { get; set; }
    }
}
