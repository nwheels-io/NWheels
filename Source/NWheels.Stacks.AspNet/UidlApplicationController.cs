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
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NWheels.Authorization.Core;
using NWheels.Concurrency;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Endpoints.Core;
using NWheels.Entities.Core;
using NWheels.Processing.Documents;
using NWheels.TypeModel;
using NWheels.UI;
using NWheels.UI.Toolbox;

namespace NWheels.Stacks.AspNet
{
    public class UidlApplicationController : ApiController
    {
        private const string UploadSessionKey = "UPLOAD";
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly IFramework _framework;
        private readonly IComponentContext _components;
        private readonly IWebModuleContext _context;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IServiceBus _serviceBus;
        private readonly IMethodCallObjectFactory _callFactory;
        private readonly ISessionManager _sessionManager;
        private readonly IFrameworkUIConfig _uiConfig;
        private readonly Dictionary<string, TransactionScriptEntry> _transactionScriptByName;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<IMessageObject>> _pendingPushMessagesBySessionId;
        private readonly JsonSerializerSettings _uidlJsonSettings;
        private readonly ClientScriptBundles _scriptBundles;
        private readonly string _indexHtml;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlApplicationController(
            IFramework framework,
            IComponentContext components,
            IWebModuleContext context, 
            ITypeMetadataCache metadataCache,
            IServiceBus serviceBus, 
            IMethodCallObjectFactory callFactory,
            ISessionManager sessionManager,
            IFrameworkUIConfig uiConfig)
        {
            _framework = framework;
            _components = components;
            _context = context;
            _serviceBus = serviceBus;
            _metadataCache = metadataCache;
            _callFactory = callFactory;
            _sessionManager = sessionManager;
            _uiConfig = uiConfig;

            _transactionScriptByName = new Dictionary<string, TransactionScriptEntry>(StringComparer.InvariantCultureIgnoreCase);
            _pendingPushMessagesBySessionId = new ConcurrentDictionary<string, ConcurrentQueue<IMessageObject>>();
            _uidlJsonSettings = CreateUidlJsonSettings();

            if (_uiConfig.EnableContentBundling)
            {
                _scriptBundles = new ClientScriptBundles(
                    context,
                    jsBundleUrlPath: "app/v/" + context.Application.Version + "/bundle.js",
                    cssBundleUrlPath: "app/v/" + context.Application.Version + "/bundle.css",
                    mapPath: MapStaticContentPath);

                _indexHtml = _scriptBundles.ProcessHtml(LoadIndexHtml());
                _scriptBundles.BuildBundles();
            }
            else
            {
                _indexHtml = LoadIndexHtml();
            }

            RegisterTransactionScripts(components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetIndexHtml()
        {
            var fileContents = _indexHtml;
            var resolvedMacrosFileContents = fileContents.Replace("##BASE_URL##", this.Request.RequestUri.ToString().EnsureTrailingSlash());

            return base.ResponseMessage(
                new HttpResponseMessage() {
                    Content = new StringContent(resolvedMacrosFileContents, Encoding.UTF8, "text/html")
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("favicon.ico")]
        public IHttpActionResult GetFavicon()
        {
            var filePath = HttpContext.Current.Server.MapPath("~/UI/Web/favicon.ico");

            if ( File.Exists(filePath) )
            {
                var iconContents = File.ReadAllBytes(filePath);
                var response = new HttpResponseMessage() {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(iconContents)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/x-icon");
                return ResponseMessage(response);
            }
            else
            {
                return StatusCode(HttpStatusCode.NotFound);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/Application.js")]
        public IHttpActionResult GetApplicationJavaScript()
        {
            var filePath = HttpContext.Current.Server.MapPath("~/UI/Web/Scripts/" + _context.Application.IdName + ".js");

            if ( File.Exists(filePath) )
            {
                var fileContents = File.ReadAllText(filePath);

                return ResponseMessage(new HttpResponseMessage() {
                    Content = new StringContent(fileContents, Encoding.UTF8, "application/javascript")
                });
            }
            else
            {
                return StatusCode(HttpStatusCode.NotFound);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/v/{version}/bundle.js")]
        public IHttpActionResult GetJsBundle(string version)
        {
            if ( version == _context.Application.Version )
            {
                var response = new HttpResponseMessage() {
                    Content = new StringContent(_scriptBundles.JsBundle, Encoding.UTF8, "application/javascript")
                };
                response.Headers.CacheControl = new CacheControlHeaderValue() {
                    Public = true,
                    MaxAge = TimeSpan.FromDays(90),
                };

                return ResponseMessage(response);
            }
            else
            {
                return StatusCode(HttpStatusCode.NotFound);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/v/{version}/bundle.css")]
        public IHttpActionResult GetCssBundle(string version)
        {
            if ( version == _context.Application.Version )
            {
                var response = new HttpResponseMessage() {
                    Content = new StringContent(_scriptBundles.CssBundle, Encoding.UTF8, "text/css")
                };
                response.Headers.CacheControl = new CacheControlHeaderValue() {
                    Public = true,
                    MaxAge = TimeSpan.FromDays(90),
                };

                return ResponseMessage(response);
            }
            else
            {
                return StatusCode(HttpStatusCode.NotFound);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/skin/{*path}")]
        public HttpResponseMessage GetSkinStaticContent(string path)
        {
            var filePath = Path.Combine(_context.ContentRootPath, _context.SkinSubFolderName, path.Replace("/", "\\"));
            return LoadFileContentsAsResponse(filePath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/base/{*path}")]
        public HttpResponseMessage GetBaseStaticContent(string path)
        {
            var filePath = Path.Combine(_context.ContentRootPath, _context.BaseSubFolderName, path.Replace("/", "\\"));
            return LoadFileContentsAsResponse(filePath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/uidl-element-template/{templateName}")]
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
        [Route("app/uidl.json/{elementType?}/{elementName?}")]
        public IHttpActionResult GetUidl(string elementType = null, string elementName = null)
        {
            if ( string.IsNullOrEmpty(elementType) )
            {
                return Json(_context.Uidl, _uidlJsonSettings);
            }

            object element;

            switch ( elementType.ToUpper() )
            {
                case "SCREEN":
                    element = _context.Uidl.Applications[0].Screens.FirstOrDefault(s => s.ElementName.EqualsIgnoreCase(elementName));
                    break;
                case "SCREENPART":
                    element = _context.Uidl.Applications[0].ScreenParts.FirstOrDefault(s => s.ElementName.EqualsIgnoreCase(elementName));
                    break;
                default:
                    element = null;
                    break;
            }

            if ( element != null )
            {
                return Json(element, _uidlJsonSettings);
            }

            return StatusCode(HttpStatusCode.NotFound);

            //_context.Uidl.MetaTypes = null;
            //_context.Uidl.Locales = null;
            //_context.Uidl.Applications[0].Screens = null;
            //_context.Uidl.Applications[0].ScreenParts = null;
            //return Json(_context.Uidl, _uidlJsonSettings);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/stateRestore")]
        public IHttpActionResult RestoreAppViewState()
        {
            var viewState = _context.Application.CreateViewStateForCurrentUser(_components);

            var serializerSettings = _context.EntityService.CreateSerializerSettings();
            var jsonString = JsonConvert.SerializeObject(viewState, serializerSettings);

            return ResponseMessage(new HttpResponseMessage() {
                Content = new StringContent(jsonString, Encoding.UTF8, "application/json")
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/timeRangePresets")]
        public IHttpActionResult CalculateTimeRangePresets()
        {
            var now = _framework.UtcNow;
            var presetValues = new Dictionary<TimeRangePreset, Interval<DateTime>>() {
                { TimeRangePreset.Last5Minutes, new Interval<DateTime>(now.AddMinutes(-5), now) },
                { TimeRangePreset.Last15Minutes, new Interval<DateTime>(now.AddMinutes(-15), now) },
                { TimeRangePreset.Last30Minutes, new Interval<DateTime>(now.AddMinutes(-30), now) },
                { TimeRangePreset.LastHour, new Interval<DateTime>(now.AddHours(-1), now) },
                { TimeRangePreset.Last3Hours, new Interval<DateTime>(now.AddHours(-3), now) },
                { TimeRangePreset.Last4Hours, new Interval<DateTime>(now.AddHours(-4), now) },
                { TimeRangePreset.Last6Hours, new Interval<DateTime>(now.AddHours(-6), now) },
                { TimeRangePreset.Last12Hours, new Interval<DateTime>(now.AddHours(-12), now) },
                { TimeRangePreset.Last24Hours, new Interval<DateTime>(now.AddHours(-24), now) },
                { TimeRangePreset.Last3Days, new Interval<DateTime>(now.AddDays(-3), now) },
                { TimeRangePreset.Last7Days, new Interval<DateTime>(now.AddDays(-7), now) },
                { TimeRangePreset.Last30Days, new Interval<DateTime>(now.AddDays(-30), now) },
                { TimeRangePreset.Last3Months, new Interval<DateTime>(now.AddMonths(-3), now) },
                { TimeRangePreset.Last6Months, new Interval<DateTime>(now.AddMonths(-6), now) },
                { TimeRangePreset.Last12Months, new Interval<DateTime>(now.AddMonths(-12), now) },
                { TimeRangePreset.Today, new Interval<DateTime>(now.Date, now.Date.AddDays(1).AddSeconds(-1)) },
                { TimeRangePreset.Yesterday, new Interval<DateTime>(now.Date.AddDays(-1), now.Date.AddSeconds(-1)) },
                { TimeRangePreset.ThisWeek, new Interval<DateTime>(now.StartOfWeek(DayOfWeek.Sunday), now.StartOfWeek(DayOfWeek.Sunday).AddDays(7).AddSeconds(-1)) },
                { TimeRangePreset.LastWeek, new Interval<DateTime>(now.StartOfWeek(DayOfWeek.Sunday), now.StartOfWeek(DayOfWeek.Sunday).AddDays(7).AddSeconds(-1)) },
                { TimeRangePreset.ThisMonth, new Interval<DateTime>(now.StartOfMonth(), now.StartOfMonth().AddMonths(1).AddSeconds(-1)) },
                { TimeRangePreset.LastMonth, new Interval<DateTime>(now.StartOfMonth().AddMonths(-1), now.StartOfMonth().AddSeconds(-1)) },
                { TimeRangePreset.ThisQuarter, new Interval<DateTime>(now.StartOfQuarter(), now.StartOfQuarter().AddMonths(3).AddSeconds(-1)) },
                { TimeRangePreset.ThisYear, new Interval<DateTime>(now.StartOfyear(), now.StartOfyear().AddYears(1).AddSeconds(-1)) },
                { TimeRangePreset.LastYear, new Interval<DateTime>(now.StartOfyear().AddYears(-1), now.StartOfyear().AddSeconds(-1)) },
                { TimeRangePreset.AllTime, new Interval<DateTime>(DateTime.MinValue, now) },
            };

            return Json(presetValues.Select(
                kvp => new {
                    Name = kvp.Key.ToString(),
                    Start = kvp.Value.LowerBound.ToString("yyyy-MM-dd HH:mm:ss"),
                    End = kvp.Value.UpperBound.ToString("yyyy-MM-dd HH:mm:ss")
                }), 
                new JsonSerializerSettings() {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("app/api/oneWay/command/{target}/{contractName}/{operationName}/{entityName?}")]
        public IHttpActionResult ApiOneWayCommand(string target, string contractName, string operationName, string entityName = null)
        {
            var command = TryCreateCommandMessage(target, contractName, operationName, Request.GetQueryString(), synchronous: false);

            if ( command == null )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            _serviceBus.EnqueueMessage(command);

            return Json(
                new  {
                    CommandMessageId = command.MessageId
                },
                _context.EntityService.CreateSerializerSettings());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("app/api/requestReply/command/{target}/{contractName}/{operationName}/{entityName?}")]
        public IHttpActionResult ApiRequestReplyCommand(string target, string contractName, string operationName, string entityName = null)
        {
            var command = TryCreateCommandMessage(target, contractName, operationName, Request.GetQueryString(), synchronous: true);

            if ( command == null )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            using (new UIOperationContext(
                _context.EntityService, ApiCallType.RequestReply, ApiCallResultType.Command, 
                target, contractName, operationName, entity: entityName))
            { 
                try
                {
                    _serviceBus.DispatchMessageOnCurrentThread(command);
                
                    if ( command.Result.NewSessionId != null )
                    {
                        var sessionIdKey = _sessionManager.As<ICoreSessionManager>().SessionIdCookieName;
                        HttpContext.Current.Session[sessionIdKey] = command.Result.NewSessionId;
                    }
                }
                catch ( Exception e )
                {
                    _context.Logger.CommandFailed(this.Request.RequestUri.AbsolutePath, e);

                    if ( !command.HasFaultResult() )
                    {
                        throw;
                    }
                }

                var responseJsonString = JsonConvert.SerializeObject(
                    command.Result.TakeSerializableSnapshot(), 
                    _context.EntityService.CreateSerializerSettings());

                return ResponseMessage(new HttpResponseMessage() {
                    StatusCode = (command.HasFaultResult() ? HttpStatusCode.InternalServerError : HttpStatusCode.OK),
                    Content = new StringContent(responseJsonString, Encoding.UTF8, "application/json")
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("app/api/requestReplyAsync/command/{target}/{contractName}/{operationName}/{entityName?}")]
        public IHttpActionResult ApiRequestReplyAsyncCommand(string target, string contractName, string operationName, string entityName = null)
        {
            var command = TryCreateCommandMessage(target, contractName, operationName, Request.GetQueryString(), synchronous: false);

            if ( command == null )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            _serviceBus.EnqueueMessage(command);

            return Json(
                new {
                    CommandMessageId = command.MessageId
                },
                _context.EntityService.CreateSerializerSettings());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("app/api/requestReply/entityQuery/{entityName}/{target}/{contractName}/{operationName}")]
        public IHttpActionResult ApiRequestReplyEntityQuery(string entityName, string target, string contractName, string operationName)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            var queryParameters = this.Request.GetQueryString();
            var options = _context.EntityService.ParseQueryOptions(entityName, queryParameters);
            var command = TryCreateCommandMessage(target, contractName, operationName, queryParameters, synchronous: true);

            if ( command == null )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            var txViewModel = TryGetViewModelFrom(command);

            using (new UIOperationContext(
                _context.EntityService, ApiCallType.RequestReply, ApiCallResultType.EntityQuery, 
                target, contractName, operationName, entity: entityName, query: options))
            { 
                using (_context.EntityService.NewUnitOfWork(entityName, txViewModel))
                {
                    try
                    {
                        _serviceBus.DispatchMessageOnCurrentThread(command);

                        var query = (IQueryable)command.Result.Result;
                        var json = _context.EntityService.QueryEntityJson(entityName, query, options, txViewModel);

                        return ResponseMessage(new HttpResponseMessage() {
                            Content = new StringContent(json, Encoding.UTF8, "application/json")
                        });
                    }
                    catch ( Exception e )
                    {
                        _context.Logger.CommandFailed(this.Request.RequestUri.AbsolutePath, e);

                        if ( command.HasFaultResult() )
                        {
                            var responseJsonString = JsonConvert.SerializeObject(
                                command.Result.TakeSerializableSnapshot(),
                                _context.EntityService.CreateSerializerSettings());

                            return ResponseMessage(new HttpResponseMessage() {
                                StatusCode = HttpStatusCode.InternalServerError,
                                Content = new StringContent(responseJsonString, Encoding.UTF8, "application/json")
                            });
                        }

                        throw;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("app/api/requestReply/entityQueryExport/{entityName}/{target}/{contractName}/{operationName}/{outputFormat}")]
        public IHttpActionResult ApiRequestReplyEntityQueryExport(string entityName, string target, string contractName, string operationName, string outputFormat)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            var queryParameters = this.Request.GetQueryString();
            var options = _context.EntityService.ParseQueryOptions(entityName, queryParameters);
            var queryCommand = TryCreateCommandMessage(target, contractName, operationName, queryParameters, synchronous: true);

            if ( queryCommand == null )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            using (new UIOperationContext(
                _context.EntityService, ApiCallType.RequestReply, ApiCallResultType.EntityQuery, 
                target, contractName, operationName, entity: entityName, query: options, format: outputFormat))
            { 
                using (_context.EntityService.NewUnitOfWork(entityName, TryGetViewModelFrom(queryCommand)))
                {
                    DocumentFormatRequestMessage exportCommand = null;
                
                    try
                    {
                        _serviceBus.DispatchMessageOnCurrentThread(queryCommand);
                        var download = (queryCommand.Result as DocumentFormatReplyMessage);

                        if (download == null)
                        {
                            var query = (IQueryable)queryCommand.Result.Result;
                            exportCommand = new DocumentFormatRequestMessage(
                                _framework,
                                Session.Current,
                                isSynchronous: true,
                                entityService: _context.EntityService,
                                reportCriteria: null,
                                reportQuery: query,
                                reportQueryOptions: options,
                                documentDesign: null,
                                outputFormatIdName: outputFormat);

                            _serviceBus.DispatchMessageOnCurrentThread(exportCommand);
                            download = exportCommand.Result as DocumentFormatReplyMessage;
                        }

                        if ( download != null )
                        {
                            HttpContext.Current.Session[download.CommandMessageId.ToString("N")] = download.Document;
                            return Json(download.TakeSerializableSnapshot(), _context.EntityService.CreateSerializerSettings());
                        }

                        throw new Exception("No content was produced for download.");
                    }
                    catch (Exception e)
                    {
                        _context.Logger.CommandFailed(this.Request.RequestUri.AbsolutePath, e);

                        if ( queryCommand.HasFaultResult() || (exportCommand != null && exportCommand.HasFaultResult()) )
                        {
                            var responseJsonString = JsonConvert.SerializeObject(
                                (exportCommand ?? queryCommand).Result.TakeSerializableSnapshot(),
                                _context.EntityService.CreateSerializerSettings());

                            return ResponseMessage(new HttpResponseMessage() {
                                StatusCode = HttpStatusCode.InternalServerError,
                                Content = new StringContent(responseJsonString, Encoding.UTF8, "application/json")
                            });
                        }

                        throw;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/downloadContent/{contentId}")]
        public IHttpActionResult DownloadContent(string contentId)
        {
            var document = HttpContext.Current.Session[contentId] as FormattedDocument;

            if ( document == null )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            HttpResponseMessage download = new HttpResponseMessage(HttpStatusCode.OK);
            
            var stream = new MemoryStream(document.Contents);
            download.Content = new StreamContent(stream);
            download.Content.Headers.ContentType = new MediaTypeHeaderValue(document.Metadata.Format.ContentType);
            download.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            download.Content.Headers.ContentDisposition.FileName = document.Metadata.FileName;

            HttpContext.Current.Session.Remove(contentId);
            return ResponseMessage(download); 
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("app/uploadContent")]
        public IHttpActionResult UploadContent()
        {
            var httpContext = HttpContext.Current;

            if (httpContext.Request.Files.Count != 1)
            {
                return StatusCode(HttpStatusCode.BadRequest);
            }

            var file = httpContext.Request.Files[0];
            var buffer = new MemoryStream();
            file.InputStream.CopyTo(buffer);
            var document = new FormattedDocument(
                metadata: new DocumentMetadata(new DocumentFormat("UPLOAD", file.ContentType, Path.GetExtension(file.FileName), file.FileName)),
                contents: buffer.ToArray());

            httpContext.Session[UploadSessionKey] = document;

            return StatusCode(HttpStatusCode.OK);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/takeMessages")]
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
        [Route("app/entity/checkAuth/{entityName}/{entityId?}")]
        public IHttpActionResult CheckEntityAuthorization(string entityName, string entityId = null)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            using (new UIOperationContext(
                _context.EntityService, ApiCallType.RequestReply, ApiCallResultType.Command, 
                target: null, contract: "ApplicationEntityService", operation: "CheckAuth", entity: entityName))
            { 
                var checkResults = _context.EntityService.CheckEntityAuthorization(entityName, entityId);
                return Json(checkResults, _context.EntityService.CreateSerializerSettings());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("app/entity/new/{entityName}")]
        public IHttpActionResult NewEntity(string entityName)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            using (new UIOperationContext(
                _context.EntityService, ApiCallType.RequestReply, ApiCallResultType.Command, 
                target: null, contract: "ApplicationEntityService", operation: "New", entity: entityName))
            { 
                var json = _context.EntityService.NewEntityJson(entityName);

                return ResponseMessage(new HttpResponseMessage() {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet, HttpPost]
        [Route("app/entity/query/{entityName}")]
        public IHttpActionResult QueryEntity(string entityName)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            using (new UIOperationContext(
                _context.EntityService, ApiCallType.RequestReply, ApiCallResultType.Command, 
                target: null, contract: "ApplicationEntityService", operation: "QueryEntity", entity: entityName))
            { 
                var queryParameters = this.Request.GetQueryString();

                var options = _context.EntityService.ParseQueryOptions(entityName, queryParameters);
                var json = _context.EntityService.QueryEntityJson(entityName, options);

                return ResponseMessage(new HttpResponseMessage() {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet, HttpPost]
        [Route("app/entity/queryImage/{entityName}/{entityId}/{imageTypeProperty}/{imageContentProperty}")]
        public IHttpActionResult QueryImage(string entityName, string entityId, string imageTypeProperty, string imageContentProperty)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            using (new UIOperationContext(
                _context.EntityService, ApiCallType.RequestReply, ApiCallResultType.EntityQuery,
                target: null, contract: "ApplicationEntityService", operation: "QueryImage", entity: entityName))
            {
                IDomainObject entity;
                if (!_context.EntityService.TryGetEntityObjectById(entityName, entityId, out entity))
                {
                    return StatusCode(HttpStatusCode.NotFound);
                }

                var imageType = (string)entity.GetType().GetProperty(imageTypeProperty).GetValue(entity);
                var imageContents = (byte[])entity.GetType().GetProperty(imageContentProperty).GetValue(entity);

                var response = new HttpResponseMessage() {
                    Content = new ByteArrayContent(imageContents)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/" + imageType);

                return ResponseMessage(response);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("app/entity/store/{entityName}")]
        public IHttpActionResult StoreEntity(string entityName)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            using (new UIOperationContext(
                _context.EntityService, ApiCallType.RequestReply, ApiCallResultType.Command,
                target: null, contract: "ApplicationEntityService", operation: "StoreEntity", entity: entityName))
            {
                var queryString = Request.GetQueryString();
                var entityStateString = queryString.GetValueOrDefault("EntityState", EntityState.NewModified.ToString());
                var entityIdString = queryString.GetValueOrDefault("EntityId", null);
                var entityState = ParseUtility.Parse<EntityState>(entityStateString);
                var session = Session.Current;

                using (var activity = _context.Logger.StoreEntity(
                    entityName, entityState, entityIdString, session.UserIdentity.LoginName, session.Endpoint, session.UserPrincipal))
                {
                    try
                    {
                        var jsonString = Request.Content.ReadAsStringAsync().Result;
                        var json = _context.EntityService.StoreEntityJson(entityName, entityState, entityIdString, jsonString);

                        if (json != null)
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
                    catch (Exception e)
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("app/entity/storeBatch")]
        public IHttpActionResult StoreEntityBatch()
        {
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("app/entity/delete/{entityName}")]
        public IHttpActionResult DeleteEntity(string entityName, string entityId)
        {
            if ( !_context.EntityService.IsEntityNameRegistered(entityName) )
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            using (new UIOperationContext(
                _context.EntityService, ApiCallType.RequestReply, ApiCallResultType.EntityQuery,
                target: null, contract: "ApplicationEntityService", operation: "QueryImage", entity: entityName))
            {
                var session = Session.Current;

                using (var activity = _context.Logger.DeleteEntity(entityName, entityId, session.UserIdentity.LoginName, session.Endpoint, session.UserPrincipal))
                {
                    try
                    {
                        _context.EntityService.DeleteEntity(entityName, entityId);
                        return StatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception e)
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private string MapStaticContentPath(string urlPath)
        {
            var urlPathLowerCase = urlPath.ToLower();
            string pathSuffix;

            if ( MatchStaticContentPath(urlPathLowerCase, "app/base/", out pathSuffix) )
            {
                return Path.Combine(_context.ContentRootPath, _context.BaseSubFolderName, pathSuffix);
            }

            if ( MatchStaticContentPath(urlPathLowerCase, "app/skin/", out pathSuffix) )
            {
                return Path.Combine(_context.ContentRootPath, _context.SkinSubFolderName, pathSuffix);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool MatchStaticContentPath(string path, string prefix, out string suffix)
        {
            if ( path.StartsWith(prefix) )
            {
                suffix = path.Substring(prefix.Length);
                return true;
            }

            suffix = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string LoadIndexHtml()
        {
            var filePath = Path.Combine(_context.ContentRootPath, _context.SkinSubFolderName, "index.html");
            var fileContents = File.ReadAllText(filePath);
            return fileContents;
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

        private AbstractCommandMessage TryCreateCommandMessage(
            string target, 
            string contractName, 
            string operationName, 
            Dictionary<string, string> queryString, 
            bool synchronous,
            string entityName = null)
        {
            AbstractCommandMessage command;
            var targetType = ParseUtility.Parse<ApiCallTargetType>(target);

            switch ( targetType )
            {
                case ApiCallTargetType.TransactionScript:
                    command = CreateTransactionScriptCommand(contractName, operationName, synchronous);
                    break;
                case ApiCallTargetType.EntityMethod:
                    command = CreateEntityMethodCommand(contractName, operationName, queryString, synchronous);
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

            TryInjectUploadedFile(call);

            return new TransactionScriptCommandMessage(_framework, _sessionManager.CurrentSession, scriptEntry.TransactionScriptType, call, synchronous);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private AbstractCommandMessage CreateEntityMethodCommand(
            string contractName, 
            string operationName, 
            Dictionary<string, string> queryString, 
            bool synchronous)
        {
            string entityIdString;

            if ( !_context.EntityService.IsEntityNameRegistered(contractName) )
            {
                return null;
            }

            if ( !queryString.TryGetValue("$entityId", out entityIdString) )
            {
                return null;
            }

            var metaType = _context.EntityService.GetEntityMetadata(contractName);
            var method = metaType.Methods.FirstOrDefault(m => m.Name.EqualsIgnoreCase(operationName));

            if ( method == null )
            {
                return null;
            }

            Type domainContextType;;
            var parsedEntityId = _context.EntityService.ParseEntityId(contractName, entityIdString, out domainContextType);
            var call = _callFactory.NewMessageCallObject(method);
            var jsonString = Request.Content.ReadAsStringAsync().Result;
            var serializerSettings = _context.EntityService.CreateSerializerSettings();
            
            JsonConvert.PopulateObject(jsonString, call, serializerSettings);
            TryInjectUploadedFile(call);

            return new EntityMethodCommandMessage(
                _framework, 
                _sessionManager.CurrentSession, 
                parsedEntityId, 
                domainContextType, 
                call, 
                synchronous);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IObject TryGetViewModelFrom(AbstractCommandMessage command)
        {
            var haveMethodCall = command as IHaveMethodCall;

            if (haveMethodCall != null)
            {
                return TryGetViewModelFrom(haveMethodCall.Call);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IObject TryGetViewModelFrom(IMethodCallObject call)
        {
            var parameters = call.MethodInfo.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                var viewModel = call.GetParameterValue(index: i) as IObject;

                if (viewModel != null)
                {
                    return viewModel;
                }
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void TryInjectUploadedFile(IMethodCallObject call)
        {
            var viewModel = TryGetViewModelFrom(call);

            if (viewModel == null)
            {
                TryInjectUploadedFileWithoutViewModel(call);
                return;
            }

            var metaType = _metadataCache.GetTypeMetadata(viewModel.ContractType);
            var fileUploadProperty = metaType.Properties.FirstOrDefault(p =>
                (p.ClrType == typeof(byte[]) || p.ClrType == typeof(FormattedDocument)) &&
                p.SemanticType != null &&
                p.SemanticType.WellKnownSemantic.IsIn(WellKnownSemanticType.FileUpload, WellKnownSemanticType.ImageUpload));

            if (fileUploadProperty == null)
            {
                return;
            }

            var httpContextSession = HttpContext.Current.Session;
            var uploadDocument = httpContextSession[UploadSessionKey] as FormattedDocument;

            if (uploadDocument != null)
            {
                if (fileUploadProperty.ClrType == typeof(FormattedDocument))
                {
                    fileUploadProperty.WriteValue(viewModel, uploadDocument);
                }
                else
                {
                    fileUploadProperty.WriteValue(viewModel, uploadDocument.Contents);
                }
                
                httpContextSession.Remove(UploadSessionKey);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void TryInjectUploadedFileWithoutViewModel(IMethodCallObject call)
        {
            var httpContextSession = HttpContext.Current.Session;
            var uploadDocument = httpContextSession[UploadSessionKey] as FormattedDocument;

            if (uploadDocument == null)
            {
                return;
            }

            var parameters = call.MethodInfo.GetParameters();

            for (int i = 0 ; i < parameters.Length ; i++)
            {
                if (parameters[i].ParameterType == typeof(FormattedDocument))
                {
                    call.SetParameterValue(i, uploadDocument);
                }
            }
            
            httpContextSession.Remove(UploadSessionKey);
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

        private static JsonSerializerSettings CreateUidlJsonSettings()
        {
            var jsonSettings = new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                DefaultValueHandling = DefaultValueHandling.Ignore
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
                JsonConvert.PopulateObject(json, callObject, CreateUidlJsonSettings());
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
