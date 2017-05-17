using NWheels.Microservices;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using NWheels.Frameworks.Uidl.Injection;
using NWheels.Platform.Rest;
using NWheels.Extensions;
using NWheels.Platform.Messaging;

namespace NWheels.Frameworks.Uidl.Adapters.WebAngular
{
    [DefaultFeatureLoader]
    public class WebAngularFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            base.ContributeAdapterComponents(existingComponents, newComponents);

            var platformConfig = existingComponents.Resolve<IMessagingPlatformConfiguration>();

            var webAppPorts = existingComponents.ResolveAll<WebAppInjectorPort>();
            var httpEndpointConfigs = existingComponents.ResolveAll<HttpEndpointInjectorPort>()
                .Select(endpoint => platformConfig.Endpoints[endpoint.Name])
                .Cast<IHttpEndpointConfig>()
                .ToArray();

            foreach (var appPort in webAppPorts)
            {
                var assetFolderPath = PathUtility.ExpandPathFromBinary(
                    "GeneratedDeployables",
                    "NWheels.Frameworks.Uidl.Adapters.WebAngular",
                    appPort.ApplicationName);

                foreach (var endpointConfig in httpEndpointConfigs)
                {
                    endpointConfig.StaticFolders.Add(ConfigureStaticResourceFolder(
                        appPort.UriPathBase,
                        assetFolderPath,
                        platformConfig.NewHttpStaticFolderConfig()));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IHttpStaticFolderConfig ConfigureStaticResourceFolder(string urlPath, string localPath, IHttpStaticFolderConfig config)
        {
            config.RequestBasePath = urlPath;
            config.LocalRootPath = localPath;
            config.DefaultFiles.Add("index.html");

            return config;
        }
    }
}
