using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace NWheels.Stacks.AspNet
{
    public class DynamicControllerSelector : IHttpControllerSelector
    {
        private readonly ControllerInitializer _initializer;
        private readonly WebApiControllerFactory _factory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DynamicControllerSelector(ControllerInitializer initializer, WebApiControllerFactory factory)
        {
            _factory = factory;
            _initializer = initializer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            var mappings = new Dictionary<string, HttpControllerDescriptor>();

            //foreach (var controllerType in _initializer.ControllerTypes)
            //{
            //    var descriptor = new HttpControllerDescriptor(GlobalConfiguration.Configuration, controllerType.Name, controllerType);
            //    mappings.Add(controllerType.Name, descriptor);
            //}

            return mappings;
        }
    }
}
