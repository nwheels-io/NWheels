using NWheels.Microservices;
using NWheels.Injection;

namespace NWheels.Samples.FirstHappyPath
{
    [DefaultFeatureLoader]
    public class FirstHappyPathFeatureLoader : IFeatureLoader
    {
        public void RegisterComponents(IContainerBuilderWrapper containerBuilder)
        {
        }

        public void RegisterConfigSections()
        {
        }
    }
}
