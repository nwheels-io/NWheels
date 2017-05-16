using NWheels.Injection;
using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public static class ComponentContainerBuilderExtensions
    {
        public static HttpEndpointInjectorPort ServeRestApiRequests<TProtocol>(this HttpEndpointInjectorPort port)
            where TProtocol : MessageProtocol
        {
            var components = port.Components;
            var protocol = new LazySlim<TProtocol>(factory: () => components.Resolve<TProtocol>());

            port.OnRequest = (context) => {
                var restApiService = components.Resolve<IRestApiService>();
                return restApiService.HandleHttpRequest(context, protocol.Value.ProtocolName);
            };

            port.OnConfiguration += new Action<IHttpEndpointConfig>((endpointConfig) => {
                var platformConfig = components.Resolve<IMessagingPlatformConfiguration>();
                var staticFolders = components.ResolveAll<StaticResourceFolderDescription>();

                foreach (var folder in staticFolders)
                {
                    endpointConfig.StaticFolders.Add(ConfigureStaticResourceFolder(folder, platformConfig.NewHttpStaticFolderConfig()));
                }
            });

            return port;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IHttpStaticFolderConfig ConfigureStaticResourceFolder(StaticResourceFolderDescription description, IHttpStaticFolderConfig config)
        {
            config.RequestBasePath = description.FolderUriPath;
            config.LocalRootPath = description.FolderLocalPath;

            if (!string.IsNullOrEmpty(description.DefaultFileName))
            {
                config.DefaultFiles.Add(description.DefaultFileName);
            }

            return config;
        }
    }
}
