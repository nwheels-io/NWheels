using NWheels.Injection;
using NWheels.Microservices;

namespace NWheels.Platform.Rest
{
    [DefaultFeatureLoader]
    public class RestApiFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            base.ContributeComponents(existingComponents, newComponents);

            newComponents.RegisterComponentType<RestApiService>()
                .SingleInstance()
                .ForService<IRestApiService>();
        }
    }
}
