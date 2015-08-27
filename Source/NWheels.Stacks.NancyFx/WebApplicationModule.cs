using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Json;
using Nancy.Responses;
using NWheels.Processing.Commands.Factories;
using NWheels.Processing.Messages;
using NWheels.UI;
using NWheels.UI.Uidl;
using NWheels.Utilities;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationModule : NancyModule
    {
        private readonly IWebModuleContext _context;
        private readonly IServiceBus _serviceBus;
        private readonly IMethodCallObjectFactory _callFactory;
        private readonly string _contentRootPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationModule(IWebModuleContext context, IServiceBus serviceBus, IMethodCallObjectFactory callFactory)
        {
            _context = context;
            _serviceBus = serviceBus;
            _callFactory = callFactory;

            base.Get["/"] = route => View["index.html"];
            base.Get["/uidl.json"] = route => GetUidl();
            base.Post["/api/{contract}/{operation}"] = route => ExecuteDomainApi(route.contract, route.operation);
            base.Post["/command/{target}/{contract}/{operation}"] = route => EnqueueCommand(route.target, route.contract, route.operation);

            _contentRootPath = PathUtility.ModuleBinPath(_context.Application.GetType().Assembly, _context.Application.IdName) + "\\Skin.Default";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response GetUidl()
        {
            var serializer = new MetadataJsonSerializer();
            return new JsonResponse<UidlDocument>(_context.Uidl, serializer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object ExecuteDomainApi(string contractName, string operationName)
        {
            object apiService;
            WebApiDispatcherBase apiDispatcher;

            if ( !_context.ApiDispatchersByContractName.TryGetValue(contractName, out apiDispatcher) ||
                !_context.ApiServicesByContractName.TryGetValue(contractName, out apiService) )
            {
                return HttpStatusCode.NotFound;
            }

            var replyObject = apiDispatcher.DispatchOperation(operationName, this, apiService);

            if ( replyObject is HttpStatusCode )
            {
                return replyObject;
            }

            return Response.AsJson(replyObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object EnqueueCommand(ApiCallTargetType target, string contractName, string operationName)
        {


            object apiService;
            WebApiDispatcherBase apiDispatcher;

            if (!_context.ApiDispatchersByContractName.TryGetValue(contractName, out apiDispatcher) ||
                !_context.ApiServicesByContractName.TryGetValue(contractName, out apiService))
            {
                return HttpStatusCode.NotFound;
            }

            var replyObject = apiDispatcher.DispatchOperation(operationName, this, apiService);

            if (replyObject is HttpStatusCode)
            {
                return replyObject;
            }

            return Response.AsJson(replyObject);
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public string GetSkinViewPath(string viewName)
        //{
        //    return ("Content/Skin.Default/" + viewName + ".html");
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ContentRootPath
        {
            get { return _contentRootPath; }
        }
    }
}
