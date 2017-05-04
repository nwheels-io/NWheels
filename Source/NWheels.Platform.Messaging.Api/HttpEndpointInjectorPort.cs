using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NWheels.Platform.Messaging
{
    public class HttpEndpointInjectorPort : InjectorPort
    {
        public HttpEndpointInjectorPort(IComponentContainerBuilder containerBuilder, string name, Func<HttpContext, Task> handler)
            : base(containerBuilder)
        {
            this.Name = name;
            this.Handler = handler;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; }
        public Func<HttpContext, Task> Handler { get; set; }
    }
}
