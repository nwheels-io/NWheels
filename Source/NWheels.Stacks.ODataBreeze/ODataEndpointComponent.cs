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
    public class ODataEndpointComponent : LifecycleEventListenerBase
    {
        private readonly ILifetimeScope _baseContainer;
        private readonly RestApiEndpointRegistration _endpointRegistration;
        private readonly ILogger _logger;
        private IDisposable _host = null;
        private ILifetimeScope _hostLifetimeContainer = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ODataEndpointComponent(IComponentContext components, RestApiEndpointRegistration endpointRegistration, Auto<ILogger> logger)
        {
            _baseContainer = (ILifetimeScope)components;
            _endpointRegistration = endpointRegistration;
            _logger = logger.Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            try
            {
                var url = _endpointRegistration.AddressWithTrailingSlash.ToString();
                
                _hostLifetimeContainer = _baseContainer.BeginLifetimeScope();
                _host = WebApp.Start(url, ConfigureWebService);

                _logger.RestEndpointStarted(_endpointRegistration.Contract.Name, url);
            }
            catch ( Exception e )
            {
                _logger.RestEndpointFailedToStart(_endpointRegistration.Contract.Name, e);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            try
            {
                _host.Dispose();
                _hostLifetimeContainer.Dispose();
                
                _host = null;
                _hostLifetimeContainer = null;

                _logger.RestEndpointStopped(_endpointRegistration.Contract.Name);
            }
            catch ( Exception e )
            {
                _logger.RestEndpointFailedToStop(_endpointRegistration.Contract.Name, e);
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

            config.DependencyResolver = new AutofacWebApiDependencyResolver(_hostLifetimeContainer);

            app.UseAutofacMiddleware(_hostLifetimeContainer);
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
