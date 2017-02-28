using NWheels.Injection;

namespace NWheels.Configuration
{
    public interface IFeatureLoader
    {
        void RegisterConfigSections();

        void RegisterComponents(IContainer container);
    }
}
