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
using System.Web.SessionState;
using Autofac;
using Autofac.Integration.WebApi;
using Hapil;
using NWheels.Authorization;
using NWheels.Endpoints.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Stacks.Nlog;
using NWheels.UI;
using NWheels.UI.Core;
using NWheels.UI.Uidl;
using NWheels.Utilities;

namespace NWheels.Stacks.AspNet
{
    public class HttpApiEndpointApplication : HttpApplication, IUidlApplicationEndpoint
    {
        public const string BootConfigAppSettingsKey = "boot.config";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly object _s_nodeHostSyncRoot = new object();
        private static readonly IPlainLog _s_log;
        private static NodeHost _s_nodeHost;
        private static string _s_bootConfigFilePath;
        private static UidlApplication _s_uidlApplication;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly IReadOnlyList<string> _s_writableSessionRequestPathPrefixes = new[] {
            "/app/api/",
            "/app/entity/",
            "/app/downloadContent/",
            "/app/uploadContent",
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static HttpApiEndpointApplication()
        {
            CrashLog.RegisterUnhandledExceptionHandler();
            _s_log = NLogBasedPlainLog.Instance;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEndpoint.PushMessage(ISession session, Processing.Messages.IMessageObject message)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IEndpoint.Name
        {
            get
            {
                return "HttpApiEndpoint";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IEndpoint.IsPushSupported
        {
            get { return false; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TimeSpan? SessionIdleTimeoutDefault
        {
            get
            {
                var currentHttpContext = HttpContext.Current;

                if ( HttpContext.Current != null )
                {
                    var currentSession = currentHttpContext.Session;

                    if ( currentSession != null )
                    {
                        return TimeSpan.FromMinutes(currentSession.Timeout);
                    }
                }

                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        UidlApplication IUidlApplicationEndpoint.UidlApplication
        {
            get
            {
                UidlApplication app;
                TryGetUidlApplication(out app);
                return app;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Init()
        {
            base.Init();
            base.BeginRequest += WebApiApplication_BeginRequest;
            base.PostAuthorizeRequest += WebApiApplication_PostAuthorizeRequest;
            base.EndRequest += WebApiApplication_EndRequest;

            var sessionModule = (SessionStateModule)base.Modules["Session"];
            sessionModule.Start += SessionModule_OnStart;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetUidlApplication(out UidlApplication application)
        {
            if (_s_uidlApplication != null)
            {
                application = _s_uidlApplication;
                return true;
            }

            if ( _s_nodeHost != null )
            {
                return _s_nodeHost.Components.TryResolve<UidlApplication>(out application);
            }

            throw new InvalidOperationException("Node host is not started.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override string ToString()
        {
            var httpContext = HttpContext.Current;

            if (httpContext != null)
            {
                return "HttpApiEndpointApplication @ " + httpContext.Request.Url.Authority + httpContext.Request.ApplicationPath;
            }

            return "HttpApiEndpointApplication";
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SessionModule_OnStart(object sender, EventArgs eventArgs)
        {
            var sessionId = Session.SessionID;
            Session.Timeout = 40;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WebApiApplication_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                var context = HttpContext.Current;

                if ( RequestLogger != null )
                {
                    context.Items[RequestLogger] = RequestLogger.IncomingRequest(context.Request.HttpMethod, context.Request.Path);
                }

                if (!_s_writableSessionRequestPathPrefixes.Any(prefix => context.Request.Path.StartsWithIgnoreCase(prefix)))
                {
                    context.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
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
            if (RequestLogger != null)
            {
                var context = HttpContext.Current;
                var activity = (ActivityLogNode)context.Items[RequestLogger];

                if (context.Response.StatusCode < 400)
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
                        (context != null && context.Response != null ? context.Response.StatusCode : -1),
                        (context != null && context.Error != null ? context.Error : new Exception("Error is not available")));
                }

                if (activity != null)
                {
                    ((IDisposable)activity).Dispose();
                }
            }

            //if (HttpContext.Current.Request.QueryString.Get("savertdll") != null)
            //{
            //    _s_nodeHost.Components.Resolve<DynamicModule>().SaveAssembly();
            //}
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void Application_Start()
        {
            _s_log.Info("Application_Start: privateBinPath=[{0}]", AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);

            try
            {
                var globalWebAppConfig = GlobalConfiguration.Configuration;

                var bootConfig = BuildBootConfiguration();
                EnsureNodeHostStarted(bootConfig, globalWebAppConfig);

                GlobalConfiguration.Configure(
                    config => {
                        config.Services.Replace(typeof(IHttpControllerTypeResolver), _s_nodeHost.Components.Resolve<DynamicControllerTypeResolver>());
                        //config.Services.Replace(typeof(IHttpControllerSelector), _nodeHost.Components.Resolve<DynamicControllerSelector>());
                        //config.Routes.MapHttpRoute(
                        //    name: "DefaultApi",
                        //    routeTemplate: "{controller}/{action}");
                        config.DependencyResolver = new AutofacWebApiDependencyResolver(_s_nodeHost.Components);
                        config.MapHttpAttributeRoutes();
                        config.MessageHandlers.Add(new LoggingAndSessionHandler(
                            _s_nodeHost.Components.Resolve<ISessionManager>(),
                            _s_nodeHost.Components.Resolve<IWebApplicationLogger>()));
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

        protected void Application_End()
        {
            _s_log.Info("Application_End: privateBinPath=[{0}]", AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);

            try
            {
                if (_s_nodeHost != null)
                {
                    _s_nodeHost.DeactivateAndUnload();
                }
                
                _s_log.Info("Application_End: SUCCESS");
            }
            catch (Exception e)
            {
                _s_log.Critical("Application_End: FAILURE! {0}", e.ToString());
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnsureNodeHostStarted(BootConfiguration bootConfig, HttpConfiguration globalWebAppConfig)
        {
            if ( !Monitor.TryEnter(_s_nodeHostSyncRoot, 60000) )
            {
                throw new TimeoutException("Timed out waiting for Node Host initialization to complete on a different thread.");
            }

            try
            {
                if ( _s_nodeHost == null )
                {
                    _s_nodeHost = new NodeHost(
                        bootConfig,
                        registerHostComponents: builder => {
                            builder.RegisterInstance(this);
                            builder.RegisterModule<NWheels.Stacks.Nlog.ModuleLoader>();
                            //builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                            builder.RegisterType<AspNetRegistrationComponent>().AsImplementedInterfaces().SingleInstance();
                            builder.RegisterType<RequestLoggerComponent>().AsImplementedInterfaces().SingleInstance();
                            builder.RegisterWebApiFilterProvider(globalWebAppConfig);
                        });

                    _s_nodeHost.LoadAndActivate();
                    _s_nodeHost.Components.TryResolve<UidlApplication>(out _s_uidlApplication);
                }
            }
            finally
            {
                Monitor.Exit(_s_nodeHostSyncRoot);
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
            [LogThread(ThreadTaskType.ApiRequest)]
            ILogActivity IncomingRequest(string verb, string path);

            [LogInfo]
            void RequestCompleted(string verb, string path, int statusCode, int duration);

            [LogError]
            void RequestError(string verb, string uri, string controller, Exception error);

            [LogError]
            void RequestFailed(string verb, string path, int duration, int statusCode, Exception error);
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
