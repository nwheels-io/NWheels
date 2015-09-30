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
using NWheels.Utilities;
using NWheels.UI;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationBootstrapper : AutofacNancyBootstrapper, IRootPathProvider
    {
        private readonly IWebModuleContext _context;
        private readonly WebApplicationModule _module;
        private readonly WebModuleLoggingHook _loggingHook;
        private readonly WebModuleSessionHook _sessionHook;
        private readonly string _contentRootPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationBootstrapper(
            IWebModuleContext context, 
            WebApplicationModule module, 
            WebModuleLoggingHook loggingHook, 
            WebModuleSessionHook sessionHook,
            IFrameworkUIConfig frameworkUIConfig)
        {
            _context = context;
            _module = module;
            _loggingHook = loggingHook;
            _sessionHook = sessionHook;
            _contentRootPath = frameworkUIConfig.WebContentRootPath;
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
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/base", "Base"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/skin", "Skin." + _context.Application.DefaultSkin));
            
            base.ConfigureConventions(nancyConventions);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
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
            return _contentRootPath;
        }
    }
}
