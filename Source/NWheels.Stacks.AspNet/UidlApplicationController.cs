using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using NWheels.Authorization;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Processing.Commands;
using NWheels.Processing.Commands.Factories;
using NWheels.Processing.Messages;
using NWheels.UI.Uidl;
using NWheels.Utilities;
using System.Reflection;
using NWheels.Processing;

namespace NWheels.Stacks.AspNet
{
    public class UidlApplicationController : ApiController
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

        public UidlApplicationController(
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

            RegisterTransactionScripts(components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/")]
        public IHttpActionResult GetIndexHtml()
        {
            var filePath = Path.Combine(_context.ContentRootPath, _context.SkinSubFolderName, "index.html");
            var fileContents = File.ReadAllText(filePath);
            var resolvedMacrosFileContents = fileContents.Replace("##BASE_URL##", this.Request.RequestUri.ToString().EnsureTrailingSlash());

            return base.ResponseMessage(
                new HttpResponseMessage() {
                    Content = new StringContent(resolvedMacrosFileContents, Encoding.UTF8, "text/html")
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/uidl-element-template/{templateName}")]
        public IHttpActionResult GetApplicationTemplate(string templateName)
        {
            var filePath = PathUtility.ModuleBinPath(
                _context.Application.GetType().Assembly,
                Path.Combine("WebUI", _context.Application.IdName, templateName + ".html"));

            if ( File.Exists(filePath) )
            {
                var fileContents = File.ReadAllText(filePath);
                
                return ResponseMessage(new HttpResponseMessage() {
                    Content = new StringContent(fileContents, Encoding.UTF8, "text/html")
                });
            }
            else
            {
                return StatusCode(HttpStatusCode.NotFound);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/uidl.json")]
        public IHttpActionResult GetUidl()
        {
            return Json(_context.Uidl);
        }

        //----------------------------------------------------------------------------- ------------------------------------------------------------------------

        [HttpPost]
        [Route("/command/requestReply/{target}/{contractName}/{operationName?}/{entityId?}")]
        public IHttpActionResult ExecuteCommand(string target, string contractName, string operationName, string entityId)
        {
            var command = CreateCommandMessage(target, contractName, synchronous: true);

            _serviceBus.DispatchMessageOnCurrentThread(command);

            return Json(command.Result.TakeSerializableSnapshot());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/command/oneWay/{target}/{contractName}/{operationName?}/{entityId?}")]
        public IHttpActionResult EnqueueCommand(string target, string contractName, string operationName, string entityId)
        {
            var command = CreateCommandMessage(target, contractName, synchronous: false);

            _serviceBus.EnqueueMessage(command);

            return Json(new {
                CommandMessageId = command.MessageId
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/takeMessages")]
        public IHttpActionResult TakePendingPushMessages()
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

            return Json(results);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/entity/new/{entityName}")]
        public IHttpActionResult NewEntity(string entityName)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            var json = _context.EntityService.NewEntityJson(entityName);

            return ResponseMessage(new HttpResponseMessage() {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/entity/query/{entityName}")]
        public IHttpActionResult QueryEntity(string entityName)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            var queryParameters = this.Request.GetQueryString();

            var options = _context.EntityService.ParseQueryOptions(queryParameters);
            var json = _context.EntityService.QueryEntityJson(entityName, options);

            return ResponseMessage(new HttpResponseMessage() {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/entity/store/{entityName}")]
        public IHttpActionResult StoreEntity(string entityName, string entityStateString, string entityIdString)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            var entityState = ParseUtility.Parse<EntityState>(entityStateString);
            var jsonString = Request.Content.ReadAsStringAsync().Result;

            var json = _context.EntityService.StoreEntityJson(entityName, entityState, entityIdString, jsonString);

            if ( json != null )
            {
                return ResponseMessage(new HttpResponseMessage() {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
            else
            {
                return StatusCode(HttpStatusCode.OK);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/entity/storeBatch")]
        public IHttpActionResult StoreEntityBatch()
        {
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/entity/delete/{entityName}")]
        public IHttpActionResult DeleteEntity(string entityName, string entityId)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            _context.EntityService.DeleteEntity(entityName, entityId);

            return StatusCode(HttpStatusCode.OK);
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

            var commandValueBinder = (CommandValueBinder)Activator.CreateInstance(typeof(CommandValueBinder<>).MakeGenericType(call.GetType()));
            commandValueBinder.BindCommandValues(this, call);

            return new TransactionScriptCommandMessage(_framework, _sessionManager.CurrentSession, scriptEntry.TransactionScriptType, call, synchronous);
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

        private abstract class CommandValueBinder
        {
            public abstract void BindCommandValues(UidlApplicationController controller, IMethodCallObject callObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CommandValueBinder<TCallObject> : CommandValueBinder
        {
            #region Overrides of CommandValueBinder

            public override void BindCommandValues(UidlApplicationController controller, IMethodCallObject callObject)
            {
                //module.BindTo<TCallObject>((TCallObject)callObject, new BindingConfig { BodyOnly = true });
            }

            #endregion
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
