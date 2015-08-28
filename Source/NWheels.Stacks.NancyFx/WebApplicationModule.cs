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
using NWheels.Authorization;
using NWheels.Endpoints.Core;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Processing.Commands;
using NWheels.Processing.Commands.Factories;
using NWheels.Processing.Messages;
using NWheels.UI;
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
        private readonly string _contentRootPath;

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

            _contentRootPath = PathUtility.ModuleBinPath(_context.Application.GetType().Assembly, _context.Application.IdName) + "\\Skin.Default";

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
                return View["index.html"];
            };

            base.Get["/uidl.json"] = route => {
                return GetUidl();
            };

            base.Post["/api/{contract}/{operation}"] = route => {
                return ExecuteDomainApi(route.contract, route.operation);
            };

            base.Post["/command/{target}/{contract}/{operation?}/{entityId?}"] = (route) => {
                return EnqueueCommand(route.target, route.contract, route.operation, route.entityId);
            };

            base.Post["/takeMessages"] = (route) => {
                return TakePendingPushMessages();
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

        private object EnqueueCommand(string target, string contractName, string operationName, string entityId)
        {
            AbstractCommandMessage command;
            var targetType = ParseUtility.Parse<ApiCallTargetType>(target);

            switch ( targetType )
            {
                case ApiCallTargetType.TransactionScript:
                    var scriptEntry = _transactionScriptByName[contractName];
                    var call = _callFactory.NewMessageCallObject(scriptEntry.ExecuteMethodInfo);
                    var commandValueBinder = (CommandValueBinder)Activator.CreateInstance(typeof(CommandValueBinder<>).MakeGenericType(call.GetType()));
                    commandValueBinder.BindCommandValues(this, call);
                    command = new TransactionScriptCommandMessage(_framework, _sessionManager.CurrentSession, scriptEntry.TransactionScriptType, call);
                    break;
                default:
                    throw new NotSupportedException("Command target '" + target + "' is not supported.");
            }

            _serviceBus.EnqueueMessage(command);
            
            return Response.AsJson(new {
                CommandMessageId = command.MessageId
            });
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

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public string GetSkinViewPath(string viewName)
        //{
        //    return ("Content/Skin.Default/" + viewName + ".html");
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class CommandValueBinder
        {
            public abstract void BindCommandValues(INancyModule module, IMethodCallObject callObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CommandValueBinder<TCallObject> : CommandValueBinder
        {
            #region Overrides of CommandValueBinder

            public override void BindCommandValues(INancyModule module, IMethodCallObject callObject)
            {
                module.BindTo<TCallObject>((TCallObject)callObject, new BindingConfig { BodyOnly = true });
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestMethodCall : IMethodCallObject
        {
            #region Implementation of IMethodCallObject

            public void ExecuteOn(object target)
            {
                throw new NotImplementedException();
            }

            public MethodInfo MethodInfo
            {
                get { throw new NotImplementedException(); }
            }

            #endregion

            public string LoginName { get; set; }
            public string Password { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ContentRootPath
        {
            get { return _contentRootPath; }
        }

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
