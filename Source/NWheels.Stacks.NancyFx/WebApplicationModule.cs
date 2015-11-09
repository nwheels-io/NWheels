using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy;
using Nancy.Json;
using Nancy.ModelBinding;
using Nancy.Responses;
using Newtonsoft.Json;
using NWheels.Authorization;
using NWheels.Endpoints.Core;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Processing.Commands;
using NWheels.Processing.Commands.Factories;
using NWheels.Processing.Messages;
using NWheels.UI;
using NWheels.UI.Impl;
using NWheels.UI.Uidl;
using NWheels.Utilities;

namespace NWheels.Stacks.NancyFx
{
    public class WebApplicationModule : NancyModule, IEndpoint
    {
        private readonly IFramework _framework;
        private readonly IComponentContext _components;
        private readonly IWebModuleContext _context;
        private readonly IServiceBus _serviceBus;
        private readonly IMethodCallObjectFactory _callFactory;
        private readonly ISessionManager _sessionManager;
        private readonly Dictionary<string, TransactionScriptEntry> _transactionScriptByName;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<IMessageObject>> _pendingPushMessagesBySessionId;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WebApplicationModule(
            IFramework framework,
            IComponentContext components,
            IWebModuleContext context, 
            IServiceBus serviceBus, 
            IMethodCallObjectFactory callFactory,
            ISessionManager sessionManager)
        {
            _framework = framework;
            _components = components;
            _context = context;
            _serviceBus = serviceBus;
            _callFactory = callFactory;
            _sessionManager = sessionManager;

            _transactionScriptByName = new Dictionary<string, TransactionScriptEntry>(StringComparer.InvariantCultureIgnoreCase);
            _pendingPushMessagesBySessionId = new ConcurrentDictionary<string, ConcurrentQueue<IMessageObject>>();

            RegisterRoutes();
            RegisterTransactionScripts(components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEndpoint

        void IEndpoint.PushMessage(ISession session, IMessageObject message)
        {
            var sessionQueue = _pendingPushMessagesBySessionId.GetOrAdd(session.Id, id => new ConcurrentQueue<IMessageObject>());
            sessionQueue.Enqueue(message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        string IEndpoint.Name
        {
            get
            {
                return "http.app://" + _context.Application.IdName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IEndpoint.IsPushSupprted
        {
            get
            {
                return true;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterRoutes()
        {
            base.Get["/"] = route => {
                return GetIndexHtml(); //Response.AsFile("Skin." + _context.Application.DefaultSkin + "\\index.html", "text/html");
            };

            base.Get["/uidl.json"] = route => {
                return GetUidl();
            };

            base.Post["/api/{contract}/{operation}"] = route => {
                return ExecuteDomainApi(route.contract, route.operation);
            };

            base.Post["/command/requestReply/{target}/{contract}/{operation?}/{entityId?}"] = (route) => {
                return ExecuteCommand(route.target, route.contract, route.operation, route.entityId);
            };

            base.Post["/command/oneWay/{target}/{contract}/{operation?}/{entityId?}"] = (route) => {
                return EnqueueCommand(route.target, route.contract, route.operation, route.entityId);
            };

            base.Post["/takeMessages"] = (route) => {
                return TakePendingPushMessages();
            };

            base.Get["/uidl-element-template/{templateName}"] = (route) => {
                return GetApplicationTemplate(route.templateName);
            };

            base.Get["/entity/new/{entityName}"] = (route) => {
                return NewEntity(route.entityName);
            };

            base.Get["/entity/query/{entityName}"] = (route) => {
                return QueryEntity(route.entityName);
            };

            base.Post["/entity/store/{entityName}"] = (route) => {
                return StoreEntity(route.entityName, Request.Query.EntityState, Request.Query.EntityId);
            };

            base.Post["/entity/storeBatch"] = (route) => {
                return StoreEntityBatch();
            };

            base.Post["/entity/delete/{entityName}"] = (route) => {
                return DeleteEntity(route.entityName, Request.Query.EntityId);
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterTransactionScripts(IComponentContext components)
        {
            var transactionScriptTypes = components.ComponentRegistry
                .Registrations
                .Where(r => typeof(ITransactionScript).IsAssignableFrom(r.Activator.LimitType))
                .Select(r => r.Activator.LimitType);

            foreach ( var scriptType in transactionScriptTypes )
            {
                var entry = new TransactionScriptEntry(scriptType);

                _transactionScriptByName[scriptType.Name] = entry;
                _transactionScriptByName[scriptType.SimpleQualifiedName()] = entry;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response GetIndexHtml()
        {
            var filePath = Path.Combine(_context.PathProvider.GetRootPath(), _context.SkinSubFolderName, "index.html");
            var fileContents = File.ReadAllText(filePath);
            var resolvedMacrosFileContents = fileContents.Replace("##BASE_URL##", this.Request.Url.ToString().EnsureTrailingSlash());

            return Response.AsText(resolvedMacrosFileContents, "text/html");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response GetApplicationTemplate(string templateName)
        {
            var filePath = PathUtility.ModuleBinPath(
                _context.Application.GetType().Assembly, 
                Path.Combine("WebUI", _context.Application.IdName, templateName + ".html"));

            if ( File.Exists(filePath) )
            {
                var fileContents = File.ReadAllText(filePath);
                return Response.AsText(fileContents, "text/html");
            }
            else
            {
                return HttpStatusCode.NotFound;
            }
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

        //----------------------------------------------------------------------------- ------------------------------------------------------------------------

        private object ExecuteCommand(string target, string contractName, string operationName, string entityId)
        {
            var command = CreateCommandMessage(target, contractName, synchronous: true);

            _serviceBus.DispatchMessageOnCurrentThread(command);

            return Response.AsJson(command.Result.TakeSerializableSnapshot());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object EnqueueCommand(string target, string contractName, string operationName, string entityId)
        {
            var command = CreateCommandMessage(target, contractName, synchronous: false);

            _serviceBus.EnqueueMessage(command);

            return Response.AsJson(new {
                CommandMessageId = command.MessageId
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private AbstractCommandMessage CreateCommandMessage(string target, string contractName, bool synchronous)
        {
            AbstractCommandMessage command;
            var targetType = ParseUtility.Parse<ApiCallTargetType>(target);

            switch ( targetType )
            {
                case ApiCallTargetType.TransactionScript:
                    command = CreateTransactionScriptCommand(contractName, synchronous);
                    break;
                default:
                    throw new NotSupportedException("Command target '" + target + "' is not supported.");
            }

            return command;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private AbstractCommandMessage CreateTransactionScriptCommand(string contractName, bool synchronous)
        {
            var scriptEntry = _transactionScriptByName[contractName];
            var call = _callFactory.NewMessageCallObject(scriptEntry.ExecuteMethodInfo);

            var jsonString = new StreamReader(Request.Body).ReadToEnd();
            var serializerSettings = _context.EntityService.CreateSerializerSettings();
            JsonConvert.PopulateObject(jsonString, call, serializerSettings);

            return new TransactionScriptCommandMessage(_framework, _sessionManager.CurrentSession, scriptEntry.TransactionScriptType, call, synchronous);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object TakePendingPushMessages()
        {
            var results = new List<object>();
            ConcurrentQueue<IMessageObject> pendingQueue;

            if ( _pendingPushMessagesBySessionId.TryGetValue(_sessionManager.CurrentSession.Id, out pendingQueue) )
            {
                IMessageObject message;

                while ( pendingQueue.TryDequeue(out message) && results.Count < 100 )
                {
                    var pushMessage = message as AbstractSessionPushMessage;
                    var resultItem = (pushMessage != null ? pushMessage.TakeSerializableSnapshot() : message);
                    
                    results.Add(resultItem);
                }
            }

            return Response.AsJson(results);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object NewEntity(string entityName)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return HttpStatusCode.NotFound;
            }

            var json = _context.EntityService.NewEntityJson(entityName);
            return Response.AsText(json, "application/json");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object QueryEntity(string entityName)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return HttpStatusCode.NotFound;
            }

            var queryParameters = ((IEnumerable<KeyValuePair<string, object>>)this.Request.Query).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToStringOrDefault(),
                StringComparer.InvariantCultureIgnoreCase);

            var options = _context.EntityService.ParseQueryOptions(queryParameters);
            var json = _context.EntityService.QueryEntityJson(entityName, options);

            return Response.AsText(json, "application/json");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object StoreEntity(string entityName, string entityStateString, string entityIdString)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return HttpStatusCode.NotFound;
            }

            var entityState = ParseUtility.Parse<EntityState>(entityStateString);
            var jsonString = new StreamReader(Request.Body).ReadToEnd();
            
            var json = _context.EntityService.StoreEntityJson(entityName, entityState, entityIdString, jsonString);

            if ( json != null )
            {
                return Response.AsText(json, "application/json");
            }
            else
            {
                return HttpStatusCode.OK;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object StoreEntityBatch()
        {
            return HttpStatusCode.NotImplemented;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private object DeleteEntity(string entityName, string entityId)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return HttpStatusCode.NotFound;
            }

            _context.EntityService.DeleteEntity(entityName, entityId);

            return HttpStatusCode.OK;
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public string GetSkinViewPath(string viewName)
        //{
        //    return ("Content/Skin.Default/" + viewName + ".html");
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private abstract class CommandValueBinder
        //{
        //    public abstract void BindCommandValues(INancyModule module, IMethodCallObject callObject);
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private class CommandValueBinder<TCallObject> : CommandValueBinder
        //{
        //    #region Overrides of CommandValueBinder

        //    public override void BindCommandValues(INancyModule module, IMethodCallObject callObject)
        //    {
        //        module.Request.Body
        //        module.BindTo<TCallObject>((TCallObject)callObject, new BindingConfig { BodyOnly = true });
        //    }

        //    #endregion
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TransactionScriptEntry
        {
            public TransactionScriptEntry(Type transactionScriptType)
            {
                this.TransactionScriptType = transactionScriptType;
                this.ExecuteMethodInfo = transactionScriptType.GetMethod("Execute");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type TransactionScriptType { get; private set; }
            public MethodInfo ExecuteMethodInfo { get; private set; }
        }
    }
}
