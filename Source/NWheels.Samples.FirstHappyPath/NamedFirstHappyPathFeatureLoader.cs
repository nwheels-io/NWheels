using NWheels.Injection;
using NWheels.Microservices;

namespace NWheels.Samples.FirstHappyPath
{
    [FeatureLoader(Name = "SpecificNamedFirstHappyPathFeatureLoader")]
    public class NamedFirstHappyPathFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainerBuilder containerBuilder)
        {
        }
    }
}
