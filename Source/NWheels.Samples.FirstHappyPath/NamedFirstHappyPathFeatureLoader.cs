using NWheels.Injection;
using NWheels.Microservices;

namespace NWheels.Samples.FirstHappyPath
{
    [FeatureLoader(Name = "SpecificNamedFirstHappyPathFeatureLoader")]
    public class NamedFirstHappyPathFeatureLoader : IFeatureLoader
    {
        public void RegisterComponents(IContainerBuilderWrapper containerBuilder)
        {
        }

        public void RegisterConfigSections()
        {
        }
    }
}
