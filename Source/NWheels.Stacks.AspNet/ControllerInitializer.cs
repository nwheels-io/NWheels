using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Autofac;
using Autofac.Integration.WebApi;
using Hapil;
using NWheels.Endpoints;
using NWheels.Hosting;
using NWheels.Logging;

namespace NWheels.Stacks.AspNet
{
    public class ControllerInitializer : LifecycleEventListenerBase
    {
        private readonly IComponentContext _components;
        private readonly ILogger _logger;
        private readonly WebApiControllerFactory _factory;
        private readonly List<Type> _controllerTypes;
        private readonly DynamicModule _module;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ControllerInitializer(IComponentContext components, ILogger logger, WebApiControllerFactory factory, DynamicModule module)
        {
            _module = module;
            _factory = factory;
            _logger = logger;
            _components = components;
            _controllerTypes = new List<Type>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            var endpointRegistrations = _components.Resolve<IEnumerable<HttpApiEndpointRegistration>>();

            foreach ( var registration in endpointRegistrations )  
            {
                using ( var activity = _logger.InitializingWebApiController(registration.Contract.FullName))
                {
                    try
                    {
                        var handlerType = registration.Contract;
                        _controllerTypes.Add(_factory.CreateControllerType(handlerType));
                    }
                    catch ( Exception e )
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }

            var dynamicAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.IsDynamic && a.GetName().Name == _module.SimpleName);
            var updater = new ContainerBuilder();
            updater.RegisterAssemblyTypes(dynamicAssembly).Where(t => typeof(IHttpController).IsAssignableFrom(t));
            updater.Update(_components.ComponentRegistry);

            if ( _components.Resolve<IEnumerable<WebAppEndpointRegistration>>().Any() )
            {
                _controllerTypes.Add(typeof(UidlApplicationController));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyCollection<Type> ControllerTypes
        {
            get { return _controllerTypes; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogActivity]
            ILogActivity InitializingWebApiController(string handlerType);
        }
    }
}
