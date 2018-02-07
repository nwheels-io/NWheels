using NWheels.Kernel.Api.Injection;
using NWheels.RestApi.Api;

namespace NWheels.RestApi.Runtime
{
    [DefaultFeatureLoader]
    public class RestApiFeature : AdvancedFeature
    {
        public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            newComponents.RegisterComponentType<ResourceRouter>().SingleInstance().ForService<IResourceRouter>();
            newComponents.RegisterComponentType<NWheelsV1Protocol>().InstancePerDependency().ForService<IRestApiProtocol>();
            newComponents.RegisterComponentType<ConsoleRestApiLogger>().SingleInstance().ForService<IRestApiLogger>().AsFallback();
        }
    }
}
