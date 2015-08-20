using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Bootstrapper;

namespace NWheels.Stacks.NancyFx
{
    public class WebModuleLoggingHook
    {
        private readonly IWebApplicationLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebModuleLoggingHook(IWebApplicationLogger logger)
        {
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Attach(IPipelines pipelines)
        {
            pipelines.BeforeRequest += (ctx) => {
                string verb = ctx.Request.Method;
                string path = ctx.Request.Path;

                ctx.Items[this.GetType().FullName] = _logger.Request(verb, path, ctx.Request.Url.ToString());
                return null;
            };

            pipelines.AfterRequest += (ctx) => {
                _logger.RequestCompleted(ctx.Response.StatusCode);
            };

            pipelines.OnError += (ctx, error) => {
                _logger.RequestFailed(error);
                return null;
            };
        }
    }
}
