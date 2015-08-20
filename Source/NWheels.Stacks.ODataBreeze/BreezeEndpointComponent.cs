#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Batch;
using System.Web.Http.Routing;
using Autofac;
using Autofac.Integration.WebApi;
using Hapil;
using Owin;
using Microsoft.Owin.Hosting;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Endpoints;

namespace NWheels.Stacks.ODataBreeze
{
    public class BreezeEndpointComponent : LifecycleEventListenerBase
    {
        private readonly IComponentContext _baseContainer;
        private readonly DynamicModule _dyanmicModule;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly RestApiEndpointRegistration _endpointRegistration;
        private readonly ILogger _logger;
        private ILifetimeScope _containerLifetimeScope = null;
        private IDisposable _host = null;
        private Type _apiControllerType = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BreezeEndpointComponent(
            IComponentContext components,
            DynamicModule dyanmicModule,
            ITypeMetadataCache metadataCache, 
            RestApiEndpointRegistration endpointRegistration,
            Auto<ILogger> logger)
        {
            _dyanmicModule = dyanmicModule;
            _metadataCache = metadataCache;
            _baseContainer = components;
            _endpointRegistration = endpointRegistration;
            _logger = logger.Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Load()
        {
            _containerLifetimeScope = ((ILifetimeScope)_baseContainer).BeginLifetimeScope();
            GenerateBreezeApiController();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            string url = null;

            try
            {
                url = _endpointRegistration.AddressWithTrailingSlash.ToString();
                
                _logger.RestEndpointStarting(_endpointRegistration.Contract.Name, url);

                _host = WebApp.Start(url, ConfigureWebService);

                _logger.RestEndpointStarted(_endpointRegistration.Contract.Name, url);
            }
            catch ( Exception e )
            {
                _logger.RestEndpointFailedToStart(_endpointRegistration.Contract.Name, url, e);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            try
            {
                _host.Dispose();
                _host = null;

                _logger.RestEndpointStopped(_endpointRegistration.Contract.Name);
            }
            catch ( Exception e )
            {
                _logger.RestEndpointFailedToStop(_endpointRegistration.Contract.Name, e);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Unload()
        {
            _containerLifetimeScope.Dispose();
            _containerLifetimeScope = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void GenerateBreezeApiController()
        {
            var factory = new BreezeApiControllerFactory(_dyanmicModule, _metadataCache);
            _apiControllerType = factory.CreateControllerType(_endpointRegistration.Contract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureWebService(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            //config.Routes.MapHttpRoute(
            //    name: "BreezeApi",
            //    routeTemplate: "{controller}/{action}");

            var containerUpdater = new ContainerBuilder();
            containerUpdater.RegisterWebApiFilterProvider(config);
            containerUpdater.RegisterType(_apiControllerType);
            containerUpdater.Update(_containerLifetimeScope.ComponentRegistry);

            config.Services.Replace(typeof(IHttpControllerTypeResolver), new SingleControllerTypeResolver(_apiControllerType));
            config.MapHttpAttributeRoutes(new DirectRouteProviderWithInheritance());
            config.DependencyResolver = new AutofacWebApiDependencyResolver(_containerLifetimeScope);

            app.UseAutofacMiddleware(_containerLifetimeScope);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogDebug]
            void RestEndpointStarting(string repositoryName, string url);
            [LogInfo]
            void RestEndpointStarted(string repositoryName, string url);
            [LogError]
            void RestEndpointFailedToStart(string repositoryName, string url, Exception e);
            [LogInfo]
            void RestEndpointStopped(string repositoryName);
            [LogError]
            void RestEndpointFailedToStop(string repositoryName, Exception e);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SingleControllerTypeResolver : IHttpControllerTypeResolver
        {
            private readonly Type _controllerType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SingleControllerTypeResolver(Type controllerType)
            {
                _controllerType = controllerType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new[] { _controllerType };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DirectRouteProviderWithInheritance : DefaultDirectRouteProvider
        {
            protected override IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
            {
                // inherit route attributes decorated on base class controller's actions
                return actionDescriptor.GetCustomAttributes<IDirectRouteFactory>(inherit: true);
            }
        }
    }
}
