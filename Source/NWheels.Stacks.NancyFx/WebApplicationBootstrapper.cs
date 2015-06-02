using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy;
using Nancy.Bootstrappers.Autofac;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationBootstrapper : AutofacNancyBootstrapper
    {
        private readonly ILifetimeScope _externalContainer;
        private readonly INancyModule _module;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationBootstrapper(INancyModule module)
        {
            _externalContainer = null;
            _module = module;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationBootstrapper(ILifetimeScope externalContainer, INancyModule module)
        {
            _externalContainer = externalContainer;
            _module = module;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override ILifetimeScope GetApplicationContainer()
        {
            if ( _externalContainer != null )
            {
                return _externalContainer;
            }
            else
            {
                return base.GetApplicationContainer();
            }
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
    }
}
