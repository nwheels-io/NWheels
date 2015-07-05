using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Owin;
using Microsoft.Owin.Hosting;
using NWheels.Entities;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Endpoints;

namespace NWheels.Stacks.ODataBreeze
{
    internal class ODataEndpointComponent : LifecycleEventListenerBase
    {
        private readonly ILifetimeScope _container;
        private readonly RestApiEndpointRegistration _endpointRegistration;
        private readonly IApplicationDataRepository _repository;
        private readonly ILogger _logger;
        private IDisposable _host = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ODataEndpointComponent(IComponentContext components, RestApiEndpointRegistration endpointRegistration, Auto<ILogger> logger)
        {
            _container = (ILifetimeScope)components;
            _endpointRegistration = endpointRegistration;
            _repository = (IApplicationDataRepository)components.Resolve(endpointRegistration.Contract);
            _logger = logger.Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            try
            {
                var url = _endpointRegistration.AddressWithTrailingSlash.ToString();
                _host = WebApp.Start(url, ConfigureWebService);
                _logger.RestEndpointStarted(_repository.GetType().Name, url);
            }
            catch ( Exception e )
            {
                _logger.RestEndpointFailedToStart(_repository.GetType().Name, e);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            try
            {
                _host.Dispose();
                _logger.RestEndpointStopped(_repository.GetType().Name);
            }
            catch ( Exception e )
            {
                _logger.RestEndpointFailedToStop(_repository.GetType().Name, e);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ConfigureWebService(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "BreezeApi",
                routeTemplate: "breeze/{controller}/{action}");

            config.DependencyResolver = new AutofacWebApiDependencyResolver(_container);

            app.UseAutofacMiddleware(_container);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogInfo]
            void RestEndpointStarted(string repositoryName, string url);
            [LogError]
            void RestEndpointFailedToStart(string repositoryName, Exception e);
            [LogInfo]
            void RestEndpointStopped(string repositoryName);
            [LogError]
            void RestEndpointFailedToStop(string repositoryName, Exception e);
        }
    }
}
