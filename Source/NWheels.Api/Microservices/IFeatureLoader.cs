using NWheels.Injection;

namespace NWheels.Microservices
{
    public interface IFeatureLoader
    {
        void RegisterConfigSections();

        void RegisterComponents(IContainer container);
    }
}
