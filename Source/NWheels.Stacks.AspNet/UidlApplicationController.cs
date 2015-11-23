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
using System.Web;
using NWheels.Processing;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NWheels.Authorization.Core;

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
        private readonly JsonSerializerSettings _jsonSettings;

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
            _jsonSettings = CreateJsonSerializerSettings();

            RegisterTransactionScripts(components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("")]
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
        [Route("skin/{*path}")]
        public HttpResponseMessage GetSkinStaticContent(string path)
        {
            var filePath = Path.Combine(_context.ContentRootPath, _context.SkinSubFolderName, path.Replace("/", "\\"));
            return LoadFileContentsAsResponse(filePath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("base/{*path}")]
        public HttpResponseMessage GetBaseStaticContent(string path)
        {
            var filePath = Path.Combine(_context.ContentRootPath, _context.BaseSubFolderName, path.Replace("/", "\\"));
            return LoadFileContentsAsResponse(filePath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("uidl-element-template/{templateName}")]
        public IHttpActionResult GetApplicationTemplate(string templateName)
        {
            var filePath = HttpContext.Current.Server.MapPath("~/UI/Web/Templates/" + templateName + ".html");

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
        [Route("uidl.json")]
        public IHttpActionResult GetUidl()
        {
            return Json(_context.Uidl, _jsonSettings);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("command/requestReply/{target}/{contractName}/{operationName}")]
        public IHttpActionResult ExecuteCommand(string target, string contractName, string operationName)
        {
            var command = CreateCommandMessage(target, contractName, operationName, synchronous: true);

            _serviceBus.DispatchMessageOnCurrentThread(command);

            if ( command.Result.NewSessionId != null )
            {
                var sessionIdKey = _sessionManager.As<ICoreSessionManager>().SessionIdCookieName;
                HttpContext.Current.Session[sessionIdKey] = command.Result.NewSessionId;
            }

            return Json(command.Result.TakeSerializableSnapshot(), _context.EntityService.CreateSerializerSettings());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("command/query/{entityName}/{target}/{contractName}/{operationName}")]
        public IHttpActionResult ExecuteCommandAsQuery(string entityName, string target, string contractName, string operationName)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            var queryParameters = this.Request.GetQueryString();
            var options = _context.EntityService.ParseQueryOptions(queryParameters);
            var command = CreateCommandMessage(target, contractName, operationName, synchronous: true);

            _serviceBus.DispatchMessageOnCurrentThread(command);

            var query = (IQueryable)command.Result.Result;
            var json = _context.EntityService.QueryEntityJson(entityName, query, options);

            return ResponseMessage(new HttpResponseMessage() {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("command/oneWay/{target}/{contractName}/{operationName}")]
        public IHttpActionResult EnqueueCommand(string target, string contractName, string operationName)
        {
            var command = CreateCommandMessage(target, contractName, operationName, synchronous: false);

            _serviceBus.EnqueueMessage(command);

            return Json(
                new {
                    CommandMessageId = command.MessageId
                },
                _context.EntityService.CreateSerializerSettings());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("takeMessages")]
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

            return Json(results, _context.EntityService.CreateSerializerSettings());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("entity/new/{entityName}")]
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
        [Route("entity/query/{entityName}")]
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
        [Route("entity/store/{entityName}")]
        public IHttpActionResult StoreEntity(string entityName)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            var queryString = Request.GetQueryString();
            var entityStateString = queryString.GetValueOrDefault("EntityState", EntityState.NewModified.ToString());
            var entityIdString = queryString.GetValueOrDefault("EntityId", null);

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
        [Route("entity/storeBatch")]
        public IHttpActionResult StoreEntityBatch()
        {
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("entity/delete/{entityName}")]
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

        private static HttpResponseMessage LoadFileContentsAsResponse(string filePath)
        {
            if ( File.Exists(filePath) )
            {
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                var fileStream = File.OpenRead(filePath);
                result.Content = new StreamContent(fileStream);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = Path.GetFileName(filePath);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(filePath));
                result.Content.Headers.ContentLength = fileStream.Length;
                return result;
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private AbstractCommandMessage CreateCommandMessage(string target, string contractName, string operationName, bool synchronous)
        {
            AbstractCommandMessage command;
            var targetType = ParseUtility.Parse<ApiCallTargetType>(target);

            switch ( targetType )
            {
                case ApiCallTargetType.TransactionScript:
                    command = CreateTransactionScriptCommand(contractName, operationName, synchronous);
                    break;
                default:
                    throw new NotSupportedException("Command target '" + target + "' is not supported.");
            }

            return command;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private AbstractCommandMessage CreateTransactionScriptCommand(string contractName, string operationName, bool synchronous)
        {
            var scriptEntry = _transactionScriptByName[contractName];
            var call = _callFactory.NewMessageCallObject(scriptEntry.MethodInfoByName[operationName]);

            var jsonString = Request.Content.ReadAsStringAsync().Result;
            var serializerSettings = _context.EntityService.CreateSerializerSettings();
            JsonConvert.PopulateObject(jsonString, call, serializerSettings);

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

        private static JsonSerializerSettings CreateJsonSerializerSettings()
        {
            var jsonSettings = new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };

            jsonSettings.Converters.Add(new StringEnumConverter());
            
            return jsonSettings;
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
                var json = controller.Request.Content.ReadAsStringAsync().Result;
                JsonConvert.PopulateObject(json, callObject, CreateJsonSerializerSettings());
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TransactionScriptEntry
        {
            public TransactionScriptEntry(Type transactionScriptType)
            {
                this.TransactionScriptType = transactionScriptType;
                this.MethodInfoByName = transactionScriptType.GetMethods().ToDictionary(m => m.Name, m => m, StringComparer.InvariantCultureIgnoreCase);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type TransactionScriptType { get; private set; }
            public IReadOnlyDictionary<string, MethodInfo> MethodInfoByName { get; private set; }
        }
    }
}
