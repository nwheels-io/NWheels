using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Session;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationBootstrapper : AutofacNancyBootstrapper, IRootPathProvider
    {
        private readonly WebApplicationModule _module;
        private readonly WebModuleLoggingHook _loggingHook;
        private readonly WebModuleSessionHook _sessionHook;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationBootstrapper(WebApplicationModule module, WebModuleLoggingHook loggingHook, WebModuleSessionHook sessionHook)
        {
            _module = module;
            _loggingHook = loggingHook;
            _sessionHook = sessionHook;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<INancyModule> GetAllModules(ILifetimeScope container)
        {
            return new[] { _module };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override INancyModule GetModule(ILifetimeScope container, Type moduleType)
        {
            return _module;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/assets", "assets"));
            base.ConfigureConventions(nancyConventions);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            //CookieBasedSessions.Enable(pipelines);
            _loggingHook.Attach(pipelines);
            _sessionHook.Attach(pipelines);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IRootPathProvider RootPathProvider
        {
            get { return this; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetRootPath()
        {
            return _module.ContentRootPath;
        }
    }
}
