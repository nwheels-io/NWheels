using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Injection;
using NWheels.RestApi.Api;
using NWheels.RestApi.Runtime;

namespace NWheels.Samples.HelloWorld.HelloService.AotCompiled
{
    [GeneratedCode(tool: "NWheels", version: "0.1.0-0.dev.1")]
    public class HelloServiceResourceCatalog : CompiledResourceCatalogBase
    {
        private readonly IComponentContainer _components;
        private static readonly string _s_baseUriPath = "/api/tx/Hello";
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public HelloServiceResourceCatalog(IComponentContainer components)
            : base(_s_baseUriPath)
        {
            _components = components;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public override void PopulateRouter(IDictionary<string, ProtocolBindingMap> bindingMapByProtocol)
        {
            var protocolBindingsMap = bindingMapByProtocol.GetOrAdd(NWheelsV1Protocol.Name);
            var thisCatalogBindings = new IResourceBinding[] {
                _components.Resolve<HelloTxHelloMethodNWheelsV1AspNetCoreBinding>()
            };


            for (int i = 0 ; i < thisCatalogBindings.Length ; i++)
            {
                var binding = thisCatalogBindings[i]; 
                protocolBindingsMap.Add(binding.UriPath, binding);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected override ResourceCatalogDescription BuildDescription()
        {
            var resources = new IResourceDescription[] {
                new HelloTxHelloMethodResourceDescription() 
            };
            
            return new ResourceCatalogDescription(_s_baseUriPath, resources);
        }
    }
}
