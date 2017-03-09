using NWheels.Injection;
using NWheels.Microservices;

namespace NWheels.Samples.FirstHappyPath
{
    [FeatureLoader(Name = "SpecificNamedFirstHappyPathFeatureLoader")]
    public class NamedFirstHappyPathFeatureLoader : FeatureLoaderBase
    {
        public override void RegisterComponents(IComponentContainerBuilder containerBuilder)
        {
        }
    }
}
