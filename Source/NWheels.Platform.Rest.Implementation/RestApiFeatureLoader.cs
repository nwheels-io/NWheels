using NWheels.Injection;
using NWheels.Microservices;

namespace NWheels.Platform.Rest
{
    [DefaultFeatureLoader]
    public class RestApiFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainerBuilder containerBuilder)
        {
            base.ContributeComponents(containerBuilder);

            containerBuilder.RegisterComponentType<RestApiService>()
                .SingleInstance()
                .ForService<IRestApiService>();
        }
    }
}
