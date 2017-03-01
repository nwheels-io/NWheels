using NWheels.Injection;
using NWheels.Microservices;

namespace NWheels.Samples.FirstHappyPath
{
    [FeatureLoader(Name = "NamedFirstHappyPathFeatureLoader")]
    public class NamedFirstHappyPathFeatureLoader : IFeatureLoader
    {
        public void RegisterComponents(IContainerBuilder containerBuilder)
        {
        }

        public void RegisterConfigSections()
        {
        }
    }
}
