using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using NWheels.Frameworks.Uidl.Injection;
using NWheels.Platform.Rest;
using NWheels.Extensions;

namespace NWheels.Frameworks.Uidl.Adapters.WebAngular
{
    [DefaultFeatureLoader]
    public class WebAngularFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            base.ContributeAdapterComponents(existingComponents, newComponents);

            var allPorts = existingComponents.ResolveAll<WebAppInjectorPort>();
            
            foreach (var port in allPorts)
            {
                // more TODO here!
                // TODO: register additional dynamic resources (e.g. server-side methods in view models)
                // TODO: check for the case static resources are deployed to a CDN and not locally
                newComponents.RegisterComponentInstance(new StaticResourceFolderDescription(
                    folderUriPath: port.UriPathBase,
                    folderLocalPath: PathUtility.ExpandPathFromBinary(
                        "GeneratedDeployables",
                        "NWheels.Frameworks.Uidl.Adapters.WebAngular", 
                        port.ApplicationName),
                    defaultFileName: "index.html"));
            }
        }
    }
}
