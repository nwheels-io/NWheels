using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Mvc.Routing;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.WebApi;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Stacks.Nlog;

namespace NWheels.Stacks.AspNet
{
    public class HttpApiEndpointApplication : HttpApplication
    {
        private static readonly IPlainLog _s_log;
        private NodeHost _nodeHost;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static HttpApiEndpointApplication()
        {
            CrashLog.RegisterUnhandledExceptionHandler();
            _s_log = NLogBasedPlainLog.Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Init()
        {
            base.Init();
            base.BeginRequest += WebApiApplication_BeginRequest;
            base.EndRequest += WebApiApplication_EndRequest;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WebApiApplication_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                if ( RequestLogger != null )
                {
                    var context = HttpContext.Current;
                    context.Items[RequestLogger] = RequestLogger.IncomingRequest(context.Request.HttpMethod, context.Request.Path);
                }
            }
            catch ( Exception exc )
            {
                _s_log.Error("WebApiApplication_BeginRequest {0}", exc.ToString());
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WebApiApplication_EndRequest(object sender, EventArgs e)
        {
            if ( RequestLogger != null )
            {
                var context = HttpContext.Current;
                var activity = (ActivityLogNode)context.Items[RequestLogger];

                if ( context.Response.StatusCode < 400 )
                {
                    RequestLogger.RequestCompleted(
                        context.Request.HttpMethod,
                        context.Request.Path,
                        context.Response.StatusCode,
                        (int)activity.MillisecondsDuration);
                }
                else
                {
                    RequestLogger.RequestFailed(
                        context.Request.HttpMethod,
                        context.Request.Path,
                        (int)activity.MillisecondsDuration,
                        context.Response.StatusCode);
                }

                ((IDisposable)activity).Dispose();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void Application_Start()
        {
            _s_log.Info("Application_Start: privateBinPath=[{0}]", AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);

            try
            {
                var globalWebAppConfig = GlobalConfiguration.Configuration;

                var bootConfig = BuildBootConfiguration();

                _nodeHost = new NodeHost(
                    bootConfig,
                    registerHostComponents: builder => {
                        builder.RegisterInstance(this);
                        builder.RegisterModule<NWheels.Stacks.Nlog.ModuleLoader>();
                        //builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                        builder.RegisterType<AspNetRegistrationComponent>().AsImplementedInterfaces().SingleInstance();
                        builder.RegisterType<RequestLoggerComponent>().AsImplementedInterfaces().SingleInstance();
                        builder.RegisterWebApiFilterProvider(globalWebAppConfig);
                    });

                _nodeHost.LoadAndActivate();

                GlobalConfiguration.Configure(
                    config => {
                        config.Services.Replace(typeof(IHttpControllerTypeResolver), _nodeHost.Components.Resolve<DynamicControllerTypeResolver>());
                        //config.Services.Replace(typeof(IHttpControllerSelector), _nodeHost.Components.Resolve<DynamicControllerSelector>());
                        //config.Routes.MapHttpRoute(
                        //    name: "DefaultApi",
                        //    routeTemplate: "{controller}/{action}");
                        config.DependencyResolver = new AutofacWebApiDependencyResolver(_nodeHost.Components);
                        config.MapHttpAttributeRoutes();
                    });

                _s_log.Info("Application_Start: SUCCESS");
            }
            catch ( Exception e )
            {
                _s_log.Critical("Application_Start: FAILURE! {0}", e.ToString());
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ILogger RequestLogger { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static BootConfiguration BuildBootConfiguration()
        {
            var bootConfigFilePath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath, BootConfiguration.DefaultBootConfigFileName);
            var bootConfig = BootConfiguration.LoadFromFile(bootConfigFilePath);

            bootConfig.Validate();

            _s_log.Debug(bootConfig.ToLogString());

            return bootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WebAppModuleLoader : Autofac.Module
        {
            #region Overrides of Module

            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<RequestLoggerComponent>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<AspNetRegistrationComponent>().AsImplementedInterfaces().SingleInstance();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogThread(ThreadTaskType.IncomingRequest)]
            ILogActivity IncomingRequest(string verb, string path);

            [LogInfo]
            void RequestCompleted(string verb, string path, int statusCode, int duration);

            [LogError]
            void RequestError(string verb, string uri, string controller, Exception error);

            [LogError]
            void RequestFailed(string verb, string path, int duration, int statusCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class AspNetRegistrationComponent : LifecycleEventListenerBase
        {
            private readonly IComponentContext _components;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AspNetRegistrationComponent(IComponentContext components)
            {
                _components = components;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void NodeLoading()
            {
                GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver((ILifetimeScope)_components);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class RequestLoggerComponent : LifecycleEventListenerBase
        {
            public RequestLoggerComponent(Auto<ILogger> logger)
            {
                GlobalConfiguration.Configuration.Services.Replace(typeof(IExceptionLogger), new RequestExceptionLogger());
                HttpApiEndpointApplication.RequestLogger = logger.Instance;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class RequestExceptionLogger : ExceptionLogger
        {
            public override void Log(ExceptionLoggerContext context)
            {
                base.Log(context);

                if ( RequestLogger != null )
                {
                    RequestLogger.RequestError(
                        context.Request.Method.ToString(),
                        context.Request.RequestUri.PathAndQuery,
                        context.ExceptionContext.ControllerContext.Controller.GetType().Name,
                        context.Exception);
                }
            }
        }
    }
}
