using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Hosting.Self;
using Nancy.Responses;
using NWheels.Authorization;
using NWheels.DataObjects;
using NWheels.Endpoints;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Globalization;
using NWheels.Hosting;
using NWheels.UI;
using NWheels.UI.Uidl;
using NWheels.Utilities;

namespace NWheels.Stacks.NancyFx
{
    public class HttpApiEndpointComponent : LifecycleEventListenerBase
    {
        private readonly IFramework _framework;
        private readonly IComponentContext _components;
        private readonly HttpApiEndpointRegistration _endpointRegistration;
        private readonly IHttpApiEndpointLogger _logger;
        private NancyHost _host;
        private HttpApiModule _module;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public HttpApiEndpointComponent(
            IFramework framework,
            IComponentContext components,
            HttpApiEndpointRegistration endpointRegistration,
            IHttpApiEndpointLogger logger)
        {
            _framework = framework;
            _components = components;
            _endpointRegistration = endpointRegistration;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Load()
        {
            var codeBehind = _components.Resolve(_endpointRegistration.Contract);
            _module = new HttpApiModule(_framework, _components, codeBehind);

            var bootstrapper = new Bootstrapper(_module, new LoggingHook(_logger));
            _host = new NancyHost(bootstrapper, new[] { TrailingSlashSafeUri(_endpointRegistration.Address) });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            _logger.EndpointActivating(_endpointRegistration.Address, _endpointRegistration.Contract);
            _host.Start();
            _logger.EndpointActive(_endpointRegistration.Address, _endpointRegistration.Contract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            _host.Stop();
            _logger.EndpointDeactivated(_endpointRegistration.Address, _endpointRegistration.Contract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Unload()
        {
            _host.Dispose();
            _host = null;
            _module = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Uri TrailingSlashSafeUri(Uri uri)
        {
            var uriString = uri.ToString();

            if ( uriString.EndsWith("/") )
            {
                return uri;
            }

            return new Uri(uriString + "/");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class HttpApiModule : NancyModule
        {
            private readonly IFramework _framework;
            private readonly IComponentContext _components;
            private readonly object _codeBehind;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HttpApiModule(IFramework framework, IComponentContext components, object codeBehind)
            {
                _framework = framework;
                _components = components;
                _codeBehind = codeBehind;

                RegisterRoutes();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RegisterRoutes()
            {
                var actionMethods = _codeBehind.GetType().GetMethods().Where(m => m.HasAttribute<HttpOperationAttribute>()).ToArray();

                foreach ( var method in actionMethods )
                {
                    var attribute = ValidateActionMethod(method);

                    if ( attribute.Verbs.HasFlag(HttpOperationVerbs.Get) )
                    {
                        var dispatcher = new ActionDispatcher(_codeBehind, method);
                        this.Get[attribute.Route] = (parameters) => {
                            var response = DispatchAction(dispatcher, parameters);
                            return response;
                        };
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private HttpOperationAttribute ValidateActionMethod(MethodInfo method)
            {
                var parameters = method.GetParameters();

                if ( parameters.Length != 1 || parameters[0].ParameterType != typeof(HttpRequestMessage) || method.ReturnType != typeof(HttpResponseMessage) )
                {
                    throw new ContractConventionException(typeof(HttpApiEndpointComponent), _codeBehind.GetType(), method, "Bad method signature");
                }

                return method.GetCustomAttribute<HttpOperationAttribute>();
            }


            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Response DispatchAction(ActionDispatcher dispatcher, dynamic parameters)
            {
                var requestMessage = CreateRequestMessage(parameters);
                var responseMessage = dispatcher.Dispatch(requestMessage);
                var nancyResponse = CreateNancyResponse(responseMessage);
                return nancyResponse;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private HttpRequestMessage CreateRequestMessage(dynamic parameters)
            {
                var isPostMethod = this.Request.Method.EqualsIgnoreCase("POST");
                var message = new HttpRequestMessage(
                    method: (isPostMethod ? HttpMethod.Post : HttpMethod.Get), 
                    requestUri: (string)this.Request.Url);

                foreach ( var header in this.Request.Headers )
                {
                    message.Headers.Add(header.Key, header.Value);
                }

                message.Content = new StreamContent(this.Request.Body);
                return message;
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Response CreateNancyResponse(HttpResponseMessage responseMessage)
            {
                var responseStreamFactory = new Func<Stream>(
                    () => {
                        var buffer = new MemoryStream();
                        responseMessage.Content.CopyToAsync(buffer).Wait();
                        buffer.Position = 0;
                        return buffer;
                    });

                var nancyResponse = new StreamResponse(
                    source: responseStreamFactory,
                    contentType: responseMessage.Content.Headers.ContentType.ToString());

                return nancyResponse;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ActionDispatcher
        {
            public ActionDispatcher(object target, MethodInfo method)
            {
                this.Target = target;
                this.Method = method;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HttpResponseMessage Dispatch(HttpRequestMessage message)
            {
                var response = (HttpResponseMessage)this.Method.Invoke(this.Target, new object[] { message });
                return response;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object Target { get; private set; }
            public MethodInfo Method { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class Bootstrapper : AutofacNancyBootstrapper, IRootPathProvider
        {
            private readonly HttpApiModule _module;
            private readonly LoggingHook _loggingHook;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Bootstrapper(HttpApiModule module, LoggingHook loggingHook)
            {
                _module = module;
                _loggingHook = loggingHook;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override IEnumerable<INancyModule> GetAllModules(ILifetimeScope container)
            {
                return new[] { _module };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override INancyModule GetModule(ILifetimeScope container, Type moduleType)
            {
                return _module;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
            {
                _loggingHook.Attach(pipelines);
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            protected override IRootPathProvider RootPathProvider
            {
                get { return this; }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public string GetRootPath()
            {
                return PathUtility.HostBinPath();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LoggingHook
        {
            private readonly IHttpApiEndpointLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoggingHook(IHttpApiEndpointLogger logger)
            {
                _logger = logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Attach(IPipelines pipelines)
            {
                pipelines.BeforeRequest.AddItemToStartOfPipeline(
                    ctx => {
                        string verb = ctx.Request.Method;
                        string path = ctx.Request.Path;

                        ctx.Items[this.GetType().FullName] = _logger.Request(verb, path, ctx.Request.Url.ToString());
                            // will be disposed by Nancy upon end of request
                        return null;
                    });

                pipelines.AfterRequest.AddItemToEndOfPipeline(
                    ctx => {
                        _logger.RequestCompleted(ctx.Response.StatusCode);
                    });

                pipelines.OnError.AddItemToStartOfPipeline(
                    (ctx, error) => {
                        _logger.RequestFailed(error);
                        return null;
                    });
            }
        }
    }
}
