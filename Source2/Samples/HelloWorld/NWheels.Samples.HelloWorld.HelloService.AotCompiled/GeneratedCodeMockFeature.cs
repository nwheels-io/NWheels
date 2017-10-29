using System.CodeDom.Compiler;
using NWheels.Communication.Api.Http;
using NWheels.Kernel.Api.Injection;
using NWheels.RestApi.Api;
using NWheels.RestApi.Runtime;

namespace NWheels.Samples.HelloWorld.HelloService.AotCompiled
{
    [DefaultFeatureLoader]
    public class GeneratedCodeMockFeature : AdvancedFeature
    {
        public override void ContributeConfigSections(IComponentContainerBuilder newComponents)
        {
            newComponents.RegisterComponentType<HttpEndpointConfigElement>().SingleInstance().ForService<IHttpEndpointConfigElement>();
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            base.ContributeCompiledComponents(existingComponents, newComponents);

            newComponents.RegisterComponentType<HelloTxHelloMethodInvocation>().InstancePerDependency();
            newComponents.RegisterComponentType<HelloTxHelloMethodResourceHandler>().InstancePerDependency();
            newComponents.RegisterComponentType<HelloTxHelloMethodNWheelsV1AspNetCoreBinding>().InstancePerDependency();
            newComponents.RegisterComponentType<HelloServiceResourceCatalog>().InstancePerDependency().ForService<ICompiledResourceCatalog>();
        }
    }
}
