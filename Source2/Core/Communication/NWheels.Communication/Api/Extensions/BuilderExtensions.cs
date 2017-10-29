using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Injection;
using NWheels.Microservices.Api;
using NWheels.Communication.Api.Http;
using System.Linq;
using System.IO;

namespace NWheels.Communication.Api.Extensions
{
    public static class BuilderExtensions
    {
        public static MicroserviceHostBuilder UseHttpEndpoint(
            this MicroserviceHostBuilder builder, 
            Action<HttpEndpointConfigurationBuilder> configure = null)
        {
            return builder.UseComponents((exitingComponents, newComponents) => {
                newComponents.ContributeHttpEndpoint(configure);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static void ContributeHttpEndpoint(
            this IComponentContainerBuilder builder, 
            Action<HttpEndpointConfigurationBuilder> configure = null)
        {
            var adapterPort = new HttpEndpointAdapterInjectionPort(builder, DoConfigure); 
            builder.RegisterAdapterPort(adapterPort);

            void DoConfigure(IHttpEndpointConfigElement config, IComponentContainer exisgingComponents, IComponentContainerBuilder newComponents)
            {
                var configBuilder = new HttpEndpointConfigurationBuilder(newComponents, config);
                configure?.Invoke(configBuilder);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HttpEndpointConfigurationBuilder
        {
            private readonly IComponentContainerBuilder _containerBuilder;
            private readonly IHttpEndpointConfigElement _configElement;

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public HttpEndpointConfigurationBuilder(IComponentContainerBuilder containerBuilder, IHttpEndpointConfigElement configElement)
            {
                _containerBuilder = containerBuilder;
                _configElement = configElement;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HttpEndpointConfigurationBuilder Name(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Endpoint name must be a non-empty string", nameof(name));
                }

                _configElement.Name = name;
                return this;
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HttpEndpointConfigurationBuilder Http(int port)
            {
                _configElement.Port = port;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HttpEndpointConfigurationBuilder Https(int port, string certFilePath, string certFilePassword)
            {
                _configElement.Https = _configElement.NewHttpsConfig();
                _configElement.Https.Port = port;
                _configElement.Https.RequireHttps = true;
                _configElement.Https.CertFilePath = certFilePath;
                _configElement.Https.CertFilePassword = certFilePassword;

                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HttpEndpointConfigurationBuilder StaticFolder(
                string requestPath, 
                string[] localPath, 
                string[] defaultFiles = null, 
                string cacheControl = null,
                string defaultContentType = null,
                bool browasble = false)
            {
                var item = _configElement.StaticFolders.NewItem();

                item.RequestBasePath = requestPath;
                item.LocalRootPath = Path.Combine(localPath.Select(p => p.ToPathString()).ToArray());
                item.DefaultFiles = defaultFiles;
                item.DefaultContentType = defaultContentType;
                item.CacheControl = cacheControl;
                item.EnableDirectoryBrowsing = browasble;

                _configElement.StaticFolders.Add(item);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HttpEndpointConfigurationBuilder Middleware<TMiddleware>()
                where TMiddleware : ICommunicationMiddleware<HttpContext>
            {
                _containerBuilder.RegisterComponentType<TMiddleware>();
                _configElement.MiddlewarePipeline.Add(typeof(TMiddleware));

                return this;
            }
            
            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentContainer ExistingComponents => _containerBuilder.AsInternal().RootContainer;
            public IComponentContainerBuilder NewComponents => _containerBuilder;
        }
    }
}
