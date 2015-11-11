using System;
using System.Collections.Generic;
using System.Configuration;
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
using NWheels.Authorization;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Stacks.Nlog;
using NWheels.Utilities;

namespace NWheels.Stacks.AspNet
{
    public class HttpApiEndpointApplication : HttpApplication
    {
        public const string BootConfigAppSettingsKey = "boot.config";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly IPlainLog _s_log;
        private static string _s_bootConfigFilePath;
        private NodeHost _nodeHost;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static HttpApiEndpointApplication()
        {
            CrashLog.RegisterUnhandledExceptionHandler();
            _s_log = NLogBasedPlainLog.Instance;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Init()
        {
            base.Init();
            base.BeginRequest += WebApiApplication_BeginRequest;
            base.PostAuthorizeRequest += WebApiApplication_PostAuthorizeRequest;
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

        private void WebApiApplication_PostAuthorizeRequest(object sender, EventArgs e)
        {
            System.Web.HttpContext.Current.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Required);
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
                        (context != null && context.Request != null ? context.Request.HttpMethod : "N/A"),
                        (context != null && context.Request != null ? context.Request.Path : "N/A"),
                        (context != null && context.Response != null ? context.Response.StatusCode : -1),
                        (activity != null ? (int)activity.MillisecondsDuration : -1));
                }
                else
                {
                    RequestLogger.RequestFailed(
                        (context != null && context.Request != null ? context.Request.HttpMethod : "N/A"),
                        (context != null && context.Request != null ? context.Request.Path : "N/A"),
                        (activity != null ? (int)activity.MillisecondsDuration : -1),
                        (context != null && context.Response != null ? context.Response.StatusCode : -1));
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
                        config.MessageHandlers.Add(new LoggingAndSessionHandler(
                            _nodeHost.Components.Resolve<ISessionManager>(),
                            _nodeHost.Components.Resolve<IWebApplicationLogger>()));
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
            _s_bootConfigFilePath = (Path.Combine(
                AppDomain.CurrentDomain.SetupInformation.PrivateBinPath, 
                ConfigurationManager.AppSettings[BootConfigAppSettingsKey] ?? BootConfiguration.DefaultBootConfigFileName));

            var bootConfig = BootConfiguration.LoadFromFile(_s_bootConfigFilePath);

            bootConfig.Validate();

            _s_log.Debug(bootConfig.ToLogString());

            return bootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            var assemblyProbePaths = new List<string>();

            assemblyProbePaths.Add(Path.Combine(Path.GetDirectoryName(_s_bootConfigFilePath), assemblyName.Name + ".dll"));
            assemblyProbePaths.Add(PathUtility.HostBinPath("..\\..\\Core\\" + assemblyName.Name + ".dll"));
            assemblyProbePaths.Add(PathUtility.HostBinPath("..\\..\\Bin\\Core\\" + assemblyName.Name + ".dll"));

            foreach ( var probePath in assemblyProbePaths )
            {
                if ( File.Exists(probePath) )
                {
                    return Assembly.LoadFrom(probePath);
                }
            }

            return null;
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
                try
                {
                    base.Log(context);

                    if ( context != null )
                    {
                        if ( RequestLogger != null )
                        {
                            RequestLogger.RequestError(
                                (context.Request != null ? context.Request.Method.ToString() : "N/A"),
                                (context.Request != null && context.Request.RequestUri != null ? context.Request.RequestUri.PathAndQuery : "N/A"),
                                (context.ExceptionContext != null && context.ExceptionContext.ControllerContext != null && context.ExceptionContext.ControllerContext.Controller != null
                                    ? context.ExceptionContext.ControllerContext.Controller.GetType().Name
                                    : "N/A"),
                                context.Exception ?? new Exception("N/A"));
                        }
                        else
                        {
                            _s_log.Error(string.Format(
                                "REQUEST FAILED|{0}|{1}|{2}", 
                                (context.Request != null ? context.Request.Method.ToString() : "N/A"), 
                                (context.Request != null && context.Request.RequestUri != null ? context.Request.RequestUri.PathAndQuery : "N/A"),
                                context.Exception ?? new Exception("N/A")));
                        }
                    }
                }
                catch ( Exception e )
                {
                    _s_log.Error("FAILED TO LOG REQUEST FAILURE: " + e);

                    if ( context != null && context.Exception != null )
                    {
                        _s_log.Error(context.Exception.ToString());
                    }
                }
            }
        }
    }
}
