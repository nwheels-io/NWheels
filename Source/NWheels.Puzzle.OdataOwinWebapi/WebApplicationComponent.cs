//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Http;
//using System.Web.OData.Builder;
//using System.Web.OData.Extensions;
//using Autofac;
//using Autofac.Integration.WebApi;
//using Microsoft.Owin.Hosting;
//using NWheels.Endpoints;
//using NWheels.Hosting;
//using NWheels.Logging;
//using NWheels.UI;
//using Owin;

//namespace NWheels.Puzzle.OdataOwinWebapi
//{
//    internal class WebApplicationComponent : LifecycleEventListenerBase
//    {
//        private readonly IUIApplication _app;
//        private readonly string _address;
//        private readonly ILogger _logger;
//        private readonly ILifetimeScope _container;
//        private IDisposable _host = null;

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public WebApplicationComponent(IComponentContext components,  WebAppEndpointRegistration endpoint, Auto<ILogger> logger)
//        {
//            _app = (IUIApplication)components.Resolve(endpoint.Contract);
//            _address = endpoint.Address.ToString();
//            _logger = logger.Instance;
//            _container = (ILifetimeScope)components;
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public override void Activate()
//        {
//            try
//            {
//                _host = WebApp.Start(_address, ConfigureWebApplication);
//                _logger.WebApplicationStarted(_app.GetType().Name, _address);
//            }
//            catch ( Exception e )
//            {
//                _logger.WebApplicationFailedToStart(_app.GetType().Name, e);
//                throw;
//            }
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public override void Deactivate()
//        {
//            try
//            {
//                _host.Dispose();
//                _logger.WebApplicationStopped(_app.GetType().Name);
//            }
//            catch ( Exception e )
//            {
//                _logger.WebApplicationFailedToStop(_app.GetType().Name, e);
//                throw;
//            }
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public void ConfigureWebApplication(IAppBuilder app)
//        {
//            HttpConfiguration config = new HttpConfiguration();

//            config.MapHttpAttributeRoutes();
//            config.DependencyResolver = new AutofacWebApiDependencyResolver(_container);

//            app.UseAutofacMiddleware(_container);
//            app.UseAutofacWebApi(config);
//            app.UseWebApi(config);
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public interface ILogger : IApplicationEventLogger
//        {
//            [LogInfo]
//            void WebApplicationStarted(string appName, string Url);
//            [LogError]
//            void WebApplicationFailedToStart(string appName, Exception e);
//            [LogInfo]
//            void WebApplicationStopped(string appName);
//            [LogError]
//            void WebApplicationFailedToStop(string appName, Exception e);
//        }
//    }
//}
