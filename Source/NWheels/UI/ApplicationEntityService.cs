using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NWheels.Concurrency;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using Hapil;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.DataObjects.Core;
using NWheels.Entities.Factories;
using NWheels.Processing.Documents;
using NWheels.TypeModel;
using NWheels.UI.Core;
using NWheels.UI.Factories;
using NWheels.UI.Toolbox;
using NWheels.Utilities;

namespace NWheels.UI
{
    [SecurityCheck.AllowAnonymous]
    public class ApplicationEntityService
    {
        private readonly IFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IViewModelObjectFactory _viewModelFactory;
        private readonly IQueryResultAggregatorObjectFactory _aggregatorFactory;
        private readonly IDomainContextLogger _domainContextLogger;
        private readonly Dictionary<string, EntityHandler> _handlerByEntityName;
        private readonly IJsonSerializationExtension[] _jsonExtensions;
        private readonly ConcurrentDictionary<string, JsonSerializerSettings> _serializerSettingsCache;
        private readonly JsonSerializerSettings _defaultSerializerSettings;
        private readonly IEntityHandlerExtension[] _entityHandlerExtensions;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationEntityService(
            IFramework framework,
            ITypeMetadataCache metadataCache,
            IViewModelObjectFactory viewModelFactory,
            IQueryResultAggregatorObjectFactory aggregatorFactory,
            IEnumerable<IJsonSerializationExtension> jsonExtensions,
            IDomainContextLogger domainContextLogger,
            //IEnumerable<Type> domainContextTypes,
            Pipeline<IEntityHandlerExtension> entityHandlerExtensions)
        {
            _framework = framework;
            _metadataCache = metadataCache;
            _viewModelFactory = viewModelFactory;
            _aggregatorFactory = aggregatorFactory;
            _domainContextLogger = domainContextLogger;
            _handlerByEntityName = new Dictionary<string, EntityHandler>(StringComparer.InvariantCultureIgnoreCase);
            _jsonExtensions = jsonExtensions.ToArray();
            _entityHandlerExtensions = entityHandlerExtensions.ToArray();

            //RegisterDomainObjects(domainContextTypes);

            _serializerSettingsCache = new ConcurrentDictionary<string, JsonSerializerSettings>();
            _defaultSerializerSettings = CreateSerializerSettings(queryOptions: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool IsEntityNameRegistered(string entityName)
        {
            return _handlerByEntityName.ContainsKey(entityName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual EntityHandler GetEntityHandler(string entityName)
        {
            return _handlerByEntityName[entityName];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual AuthorizationCheckResults CheckEntityAuthorization(string entityName, string entityId = null)
        {
            var handler = _handlerByEntityName[entityName];
            var checkResults = handler.CheckAuthorization(entityId);
            return checkResults;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IUnitOfWork NewUnitOfWork(string entityName, object txViewModel = null, bool debugPerformStaleCheck = false)
        {
            var handler = _handlerByEntityName[entityName];
            return handler.NewUnitOfWork(txViewModel, debugPerformStaleCheck);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string NewEntityJson(string entityName)
        {
            var handler = _handlerByEntityName[entityName];
            string json;

            using (handler.NewUnitOfWork(debugPerformStaleCheck: true))
            {
                var newEntity = handler.CreateNew();
                json = JsonConvert.SerializeObject(newEntity, _defaultSerializerSettings);
            }

            return json;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual QueryOptions ParseQueryOptions(string entityName, IDictionary<string, string> parameters)
        {
            return new QueryOptions(entityName, parameters);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string QueryEntityJson(string entityName, QueryOptions options)
        {
            var handler = _handlerByEntityName[entityName];
            string json;

            using (QueryContext.NewQuery(this, options))
            {
                using (handler.NewUnitOfWork(debugPerformStaleCheck: true))
                {
                    var results = handler.Query(options);
                    json = JsonConvert.SerializeObject(results, GetCachedSerializerSettings(options));
                }
            }

            return json;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string QueryEntityJson(string entityName, IQueryable query, QueryOptions options, object txViewModel = null)
        {
            var handler = _handlerByEntityName[entityName];
            string json;

            using ( QueryContext.NewQuery(this, options) )
            {
                using (handler.NewUnitOfWork(txViewModel))
                {
                    var results = handler.Query(options, query, txViewModel);
                    json = JsonConvert.SerializeObject(results, GetCachedSerializerSettings(options));
                }
            }

            return json;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void ProcessEntityCursor(string entityName, IQueryable query, QueryOptions options, Action<EntityCursor> action, object txViewModel = null)
        {
            var handler = _handlerByEntityName[entityName];

            using (QueryContext.NewQuery(this, options))
            {
                using (handler.NewUnitOfWork(txViewModel))
                {
                    var cursor = handler.QueryCursor(options, query, txViewModel);
                    action(cursor);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IEntityId ParseEntityId(string entityName, string entityId, out Type domainContextType)
        {
            var handler = _handlerByEntityName[entityName];
            domainContextType = handler.DomainContextType;
            return handler.ParseEntityId(entityId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IDomainObject GetEntityObjectById(string entityName, string entityId)
        {
            var handler = _handlerByEntityName[entityName];
            return handler.GetById(entityId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool TryGetEntityObjectById(string entityName, string entityId, out IDomainObject entity)
        {
            var handler = _handlerByEntityName[entityName];
            return handler.TryGetById(entityId, out entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string StoreEntityJson(string entityName, EntityState entityState, string entityId, string json)
        {
            var handler = _handlerByEntityName[entityName];
            IDomainObject domainObject = null;

            using (var context = handler.NewUnitOfWork(debugPerformStaleCheck: true))
            {
                if (entityState.IsNew())
                {
                    //var traceWriter = new MemoryTraceWriter();
                    //var populationSerializerSettings = CreateSerializerSettings();
                    //populationSerializerSettings.TraceWriter = traceWriter;

                    domainObject = handler.CreateNew();
                    JsonConvert.PopulateObject(json, domainObject, _defaultSerializerSettings);// populationSerializerSettings);

                    //Debug.WriteLine(traceWriter.ToString());
                    
                    handler.Insert(domainObject);
                }
                else if (entityState.IsModified())
                {
                    var traceWriter = new MemoryTraceWriter();
                    var populationSerializerSettings = CreateSerializerSettings();
                    populationSerializerSettings.TraceWriter = traceWriter;

                    domainObject = handler.GetById(entityId);
                    JsonConvert.PopulateObject(json, domainObject, populationSerializerSettings);//_defaultSerializerSettings);// populationSerializerSettings);

                    Debug.WriteLine(traceWriter.ToString());

                    handler.Update(domainObject);
                }
                else if (entityState.IsDeleted())
                {
                    handler.Delete(entityId);
                    return null;
                }
                else
                {
                    throw new ArgumentException("Unexpected value of entity state: " + entityState);
                }

                context.CommitChanges();

                var resultJson = (domainObject != null ? JsonConvert.SerializeObject(domainObject, _defaultSerializerSettings) : null);
                return resultJson;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string RecalculateEntityJson(string entityName, EntityState entityState, string entityId, string json)
        {
            var handler = _handlerByEntityName[entityName];
            IDomainObject domainObject = null;

            using (handler.NewUnitOfWork(debugPerformStaleCheck: true))
            {
                if (entityState.IsNew())
                {
                    domainObject = handler.CreateNew();
                    JsonConvert.PopulateObject(json, domainObject, _defaultSerializerSettings);
                    handler.Insert(domainObject);
                }
                else if (entityState.IsModified())
                {
                    var populationSerializerSettings = CreateSerializerSettings();
                    domainObject = handler.GetById(entityId);
                    JsonConvert.PopulateObject(json, domainObject, populationSerializerSettings); //_serializerSettings);
                }
                else
                {
                    throw new ArgumentException("Unexpected value of entity state: " + entityState);
                }

                var resultJson = (domainObject != null ? JsonConvert.SerializeObject(domainObject, _defaultSerializerSettings) : null);
                return resultJson;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void DeleteEntity(string entityName, string entityId)
        {
            var handler = _handlerByEntityName[entityName];

            using (var context = handler.NewUnitOfWork(debugPerformStaleCheck: true))
            {
                handler.Delete(entityId);
                context.CommitChanges();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual ITypeMetadata GetEntityMetadata(string entityName)
        {
            var handler = _handlerByEntityName[entityName];
            return handler.MetaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void StoreEntityBatchJson(string json)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual JsonSerializerSettings CreateSerializerSettings()
        {
            return CreateSerializerSettings(queryOptions: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private JsonSerializerSettings GetCachedSerializerSettings(QueryOptions queryOptions)
        {
            var cacheKey = queryOptions.BuildCacheKey();
            var cachedSettings = _serializerSettingsCache.GetOrAdd(cacheKey, key => CreateSerializerSettings(queryOptions));
            return cachedSettings;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private JsonSerializerSettings CreateSerializerSettings(QueryOptions queryOptions)
        {
            var settings = new JsonSerializerSettings() {
                ContractResolver = new DomainObjectContractResolver(_metadataCache, this, queryOptions),
                DateFormatString = "yyyy-MM-dd HH:mm:ss.fff",
                MaxDepth = 10,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new DomainObjectConverter(this, queryOptions));
            settings.Converters.Add(new ViewModelObjectConverter(this, _viewModelFactory));
            settings.Converters.Add(new FormattedDocumentConverter());
            settings.Converters.Add(new TimeSeriesPointListConverter());
            settings.Converters.Add(new MetaTypeQualifiedNameConverter(_metadataCache));

            foreach ( var extension in _jsonExtensions )
            {
                extension.ApplyTo(settings);
            }

            return settings;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void RegisterDomainObjects(IEnumerable<Type> domainContextTypes)
        {
            foreach ( var contextType in domainContextTypes )
            {
                var contractTypes = FindDomainObjectContractsFromContextType(contextType);

                foreach ( var domainObjectContract in contractTypes )
                {
                    var metaType = _metadataCache.GetTypeMetadata(domainObjectContract);
                    RegisterDomainObjectType(contextType, metaType, explicitlyDeclaredInContext: true);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private void RegisterDomainObjectTypesFromContext(Type contextType, IApplicationDataRepository contextInstance)
        //{
        //    foreach ( var entityContract in contextInstance.GetEntityContractsInRepository().Where(t => t != null) )
        //    {
        //        var metaType = _metadataCache.GetTypeMetadata(entityContract);
        //        RegisterDomainObjectType(contextType, metaType);
        //    }
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IEnumerable<Type> FindDomainObjectContractsFromContextType(Type contextType)
        {
            var allInterfaces = new HashSet<Type>(contextType.GetInterfaces().ConcatOne(contextType));
            var allProperties = new HashSet<PropertyInfo>();
            var allMethods = new HashSet<MethodInfo>();
            var allDomainObjectTypes = new HashSet<Type>();

            foreach ( var intf in allInterfaces )
            {
                allProperties.UnionWith(intf.GetProperties());
                allMethods.UnionWith(intf.GetMethods());
            }

            allDomainObjectTypes.UnionWith(
                allProperties.Select(p => p.PropertyType)
                .Where(t => t.IsConstructedGenericTypeOf(typeof(IEntityRepository<>)))
                .Select(t => t.GetGenericArguments()[0]));

            allDomainObjectTypes.UnionWith(
                allProperties.Select(p => p.PropertyType)
                .Where(t => t.IsConstructedGenericTypeOf(typeof(IPartitionedRepository<,>)))
                .Select(t => t.GetGenericArguments()[0]));

            allDomainObjectTypes.UnionWith(
                allMethods.Select(m => m.ReturnType).Where(t => t != null && t.IsInterface && (t.IsEntityContract() || t.IsEntityPartContract())));

            return allDomainObjectTypes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterDomainObjectType(Type contextType, ITypeMetadata metaType, bool explicitlyDeclaredInContext)
        {
            EntityHandler existingHandler;

            if (_handlerByEntityName.TryGetValue(metaType.QualifiedName, out existingHandler))
            {
                if (explicitlyDeclaredInContext && metaType.DefaultContextContract != existingHandler.DomainContextType)
                {
                    _handlerByEntityName.Remove(metaType.QualifiedName);
                }
                else
                {
                    return;
                }
            }

            var handlerExtensions = SelectEntityHandlerExtensionsFor(metaType);
            var handler = EntityHandler.Create(this, metaType, contextType, handlerExtensions);
            _handlerByEntityName[metaType.QualifiedName] = handler;

            foreach ( var property in metaType.Properties.Where(p => p.Kind.IsIn(PropertyKind.Part, PropertyKind.Relation)) )
            {
                RegisterDomainObjectType(contextType, property.Relation.RelatedPartyType, explicitlyDeclaredInContext: false);
            }

            foreach ( var derivedType in metaType.DerivedTypes )
            {
                RegisterDomainObjectType(contextType, derivedType, explicitlyDeclaredInContext: false);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IEntityHandlerExtension[] SelectEntityHandlerExtensionsFor(ITypeMetadata metaType)
        {
            return _entityHandlerExtensions
                .Where(x => x.EntityContract.IsAssignableFrom(metaType.ContractType))
                .ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Type[] GetContractTypes(Type type)
        {
            var contracts = new List<Type>();

            if ( type.IsInterface && (type.IsEntityContract() || type.IsEntityPartContract() || type.IsViewModelContract()) )
            {
                contracts.Add(type);
            }

            contracts.AddRange(type.GetInterfaces().Where(intf => intf.IsEntityContract() || intf.IsEntityPartContract() || intf.IsViewModelContract()));

            return contracts.ToArray();
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IPropertyMetadata[] BuildNavigationMetaPath(ITypeMetadata metaType, string propertyPath)
        {
            var pathSteps = propertyPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var stepMetaType = metaType;
            var metaPropertyPath = new IPropertyMetadata[pathSteps.Length];

            for ( int i = 0 ; i < pathSteps.Length ; i++ )
            {
                var stepMetaProperty = stepMetaType.FindPropertyByNameIncludingDerivedTypes(pathSteps[i]);
                metaPropertyPath[i] = stepMetaProperty;
                
                if ( stepMetaProperty.Relation != null && stepMetaProperty.Relation.RelatedPartyType != null )
                {
                    stepMetaType = stepMetaProperty.Relation.RelatedPartyType;
                }
                else if ( i < pathSteps.Length - 1 )
                {
                    return null;
                }
            }

            return metaPropertyPath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IEntityHandlerExtension
        {
            EntityHandler CreateEntityHandler(
                ApplicationEntityService owner,
                ITypeMetadata metaType,
                Type domainContextType,
                IEntityHandlerExtension[] extensions);
            bool CanOpenNewUnitOfWork(object txViewModel);
            IUnitOfWork OpenNewUnitOfWork(object txViewModel);
            Type EntityContract { get; }
            bool CanCreateEntityHandler { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class EntityHandlerExtension<TEntity> : IEntityHandlerExtension
        {
            public virtual EntityHandler CreateEntityHandler(
                ApplicationEntityService owner, 
                ITypeMetadata metaType, 
                Type domainContextType, 
                IEntityHandlerExtension[] extensions)
            {
                throw new NotSupportedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual bool CanOpenNewUnitOfWork(object txViewModel)
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual IUnitOfWork OpenNewUnitOfWork(object txViewModel)
            {
                throw new NotSupportedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual Type EntityContract
            {
                get { return typeof(TEntity); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public virtual bool CanCreateEntityHandler 
            {
                get { return false; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AuthorizationCheckResults
        {
            public bool CanRetrieve { get; set; }
            public bool CanCreate { get; set; }
            public bool CanUpdate { get; set; }
            public bool CanDelete { get; set; }
            public bool IsRestrictedEntry { get; set; }
            public List<string> RestrictedEntryProperties { get; set; }
            public List<string> EnabledOperations { get; set; }
            public IDomainObject FullEntity { get; set; }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static AuthorizationCheckResults AllTrue()
            {
                return new AuthorizationCheckResults() {
                    CanRetrieve = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true,
                    IsRestrictedEntry = false
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static AuthorizationCheckResults AllFalse()
            {
                return new AuthorizationCheckResults();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryContext
        {
            private readonly ApplicationEntityService _ownerService;
            private readonly ITypeMetadata _entityMetaType;
            private readonly QueryOptions _options;
            private readonly QueryResults _results;
            private readonly Dictionary<IPropertyMetadata, LeftJoinOperation> _leftJoinByNavigation;
            private IQueryResultAggregator _aggregator;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryContext(ApplicationEntityService ownerService, QueryOptions options)
            {
                _ownerService = ownerService;
                _options = options;
                _results = new QueryResults();
                _leftJoinByNavigation = new Dictionary<IPropertyMetadata, LeftJoinOperation>();
                _entityMetaType = ownerService._metadataCache.GetTypeMetadata(options.EntityName);
                _aggregator = null;

                foreach ( var selectItem in options.SelectPropertyNames.Concat(options.IncludePropertyNames) )
                {
                    selectItem.BuildMetaPropertyPath(_entityMetaType);
                }
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryOptions Options
            {
                get
                {
                    return _options;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadata EntityMetaType
            {
                get
                {
                    return _entityMetaType;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryResults Results
            {
                get
                {
                    return _results;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryResultAggregator Aggregator
            {
                get
                {
                    if ( _aggregator == null )
                    {
                        _aggregator = _ownerService._aggregatorFactory.GetQueryResultAggregator(this);
                    }

                    return _aggregator;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal LeftJoinOperation GetLeftJoinForNavigation(IPropertyMetadata navigationProperty)
            {
                var leftSideFactory = GetJoinLeftSideFactory(navigationProperty);

                return _leftJoinByNavigation.GetOrAdd(
                    navigationProperty,
                    valueFactory: navigation => new LeftJoinOperation(_ownerService, leftSideFactory, navigation));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Func<IEnumerable<object>> GetJoinLeftSideFactory(IPropertyMetadata navigationProperty)
            {
                if ( navigationProperty.DeclaringContract.ContractType.IsAssignableFrom(_entityMetaType.ContractType) )
                {
                    return () => Results.ResultSet;
                }
                else
                {
                    return () => {
                        var leftSideJoin = _leftJoinByNavigation.Values.First(j => j.Navigation.Relation.RelatedPartyType == navigationProperty.DeclaringContract);
                        return leftSideJoin.RightSideResults;
                    };
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static IDisposable NewQuery(ApplicationEntityService ownerService, QueryOptions options)
            {
                return new ThreadStaticResourceConsumerScope<QueryContext>(
                    handle => new QueryContext(ownerService, options),
                    externallyOwned: true,
                    forceNewResource: true);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static QueryContext Current
            {
                get
                {
                    return ThreadStaticResourceConsumerScope<QueryContext>.CurrentResource;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryResults
        {
            public int? PageNumber { get; set; }
            public int? PageSize { get; set; }
            public long? ResultCount { get; set; }
            public bool? MoreAvailable { get; set; }
            public object[] ResultSet { get; set; }
            public ChartData Visualization { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [JsonIgnore]
            public EntityCursor ResultCursor { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int? PageCount
            {
                get
                {
                    if ( ResultCount.HasValue && PageSize.HasValue )
                    {
                        var count = (int)(ResultCount.Value / PageSize.Value);

                        if ( (ResultCount.Value % PageSize.Value) != 0 )
                        {
                            count++;
                        }

                        return count;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityCursorMetadata
        {
            private readonly ITypeMetadata _entityMetaType;
            private readonly List<QuerySelectItem> _columns;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal EntityCursorMetadata(QueryContext context)
            {
                _entityMetaType = context.EntityMetaType;
                _columns = new List<QuerySelectItem>();

                var selectList = context.Options.SelectPropertyNames;
                var includeList = context.Options.IncludePropertyNames;

                if ( selectList.Count > 0 )
                {
                    _columns.AddRange(selectList);
                }
                else
                {
                    _columns.AddRange(_entityMetaType.Properties.Where(p => p.Kind == PropertyKind.Scalar).Select(p => new QuerySelectItem(p.Name)));
                }

                if ( includeList.Count > 0 )
                {
                    _columns.AddRange(includeList);
                }

                for ( int i = 0 ; i < _columns.Count ; i++ )
                {
                    _columns[i].BuildMetaPropertyPath(_entityMetaType);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadata EntityMetaType
            {
                get { return _entityMetaType; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IReadOnlyList<QuerySelectItem> Columns
            {
                get { return _columns; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityCursor : IEnumerable<EntityCursorRow>
        {
            private readonly EntityCursorMetadata _metadata;
            private readonly QueryContext _queryContext;
            private readonly EntityHandler _entityHandler;
            private readonly IEnumerable<IDomainObject> _dataCursor;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal EntityCursor(EntityCursorMetadata metadata, QueryContext queryContext, EntityHandler entityHandler, IEnumerable<IDomainObject> dataCursor)
            {
                _metadata = metadata;
                _queryContext = queryContext;
                _entityHandler = entityHandler;
                _dataCursor = dataCursor;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            #region Implementation of IEnumerable

            IEnumerator<EntityCursorRow> IEnumerable<EntityCursorRow>.GetEnumerator()
            {
                return _dataCursor.Select(record => new EntityCursorRow(_queryContext, _metadata, record)).GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<EntityCursorRow>)this).GetEnumerator();
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityCursorMetadata Metadata
            {
                get { return _metadata; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryContext QueryContext
            {
                get { return _queryContext; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadata PrimaryEntity
            {
                get { return _entityHandler.MetaType; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public IReadOnlyList<QuerySelectItem> Columns 
            {
                get { return _metadata.Columns; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int ColumnCount
            {
                get { return _metadata.Columns.Count; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public long RowCount
            {
                get
                {
                    return _queryContext.Results.ResultCount.GetValueOrDefault(-1);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityCursorRow
        {
            private readonly QueryContext _queryContext;
            private readonly EntityCursorMetadata _metadata;
            private readonly IDomainObject _record;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal EntityCursorRow(QueryContext queryContext, EntityCursorMetadata metadata, IDomainObject record)
            {
                _metadata = metadata;
                _queryContext = queryContext;
                _record = record;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IDomainObject Record
            {
                get { return _record; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityCursorMetadata Metadata
            {
                get { return _metadata; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object this[int columnIndex]
            {
                get
                {
                    return _metadata.Columns[columnIndex].ReadValue(_queryContext, _queryContext.EntityMetaType, _record);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum AggregationType
        {
            None,
            Sum,
            Avg,
            Min,
            Max
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryOptions
        {
            public const string IdPropertyName = "$id";

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public const string TypeParameterKey = "$type";
            public const string SelectParameterKey = "$select";
            public const string IncludeParameterKey = "$include";
            public const string PropertyAliasModifier = ":as:";
            public const string CountParameterKey = "$count";
            public const string SkipParameterKey = "$skip";
            public const string TakeParameterKey = "$take";
            public const string PageNumberParameterKey = "$page";
            public const string OrderByParameterKey = "$orderby";
            public const string AscendingParameterModifier = ":asc";
            public const string DescendingParameterModifier = ":desc";

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public const string EqualOperator = ":eq";
            public const string NotEqualOperator = ":ne";
            public const string GreaterThanOperator = ":gt";
            public const string GreaterThanOrEqualOperator = ":ge";
            public const string LessThanOperator = ":lt";
            public const string LessThanOrEqualOperator = ":le";
            public const string StringStartsWithOperator = ":bw";
            public const string StringDoesNotStartWithOperator = ":bn";
            public const string StringEndsWithOperator = ":ew";
            public const string StringDoesNotEndWithOperator = ":en";
            public const string StringContainsOperator = ":cn";
            public const string StringDoesNotContainOperator = ":nc";
            public const string IsInOperator = ":in";
            public const string IsNotInOperator = ":ni";
            public const string IsNull = ":nu";
            public const string IsNotNull = ":nn";

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public const string PropertyAggregationSeparator = "!";

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private readonly HashSet<string> _selectListLookup = null;
            private string _cacheKey = null;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryOptions(string entityName, IDictionary<string, string> queryParams)
            {
                EntityName = entityName;
                SelectPropertyNames = new List<QuerySelectItem>();
                IncludePropertyNames = new List<QuerySelectItem>();
                Filter = new List<QueryFilterItem>();
                InMemoryFilter = new List<QueryFilterItem>();
                OrderBy = new List<QueryOrderByItem>();
                InMemoryOrderBy = new List<QueryOrderByItem>();
                InMemoryAggregations = new List<QuerySelectItem>();

                foreach ( var parameter in queryParams )
                {
                    if ( parameter.Key.EqualsIgnoreCase(SelectParameterKey) )
                    {
                        SelectPropertyNames = ParsePropertyList(parameter.Value);
                    }
                    else if ( parameter.Key.EqualsIgnoreCase(IncludeParameterKey) )
                    {
                        IncludePropertyNames = ParsePropertyList(parameter.Value);
                    }
                    else if ( parameter.Key.EqualsIgnoreCase(CountParameterKey) )
                    {
                        IsCountOnly = true;
                    }
                    else if ( parameter.Key.EqualsIgnoreCase(SkipParameterKey) )
                    {
                        Skip = Int32.Parse(parameter.Value);
                    }
                    else if ( parameter.Key.EqualsIgnoreCase(TakeParameterKey) )
                    {
                        Take = Int32.Parse(parameter.Value);
                    }
                    else if ( parameter.Key.EqualsIgnoreCase(PageNumberParameterKey) )
                    {
                        Page = Int32.Parse(parameter.Value);
                    }
                    else if (parameter.Key.EqualsIgnoreCase(TypeParameterKey) || parameter.Key.EqualsIgnoreCase(TypeParameterKey + EqualOperator))
                    {
                        OfType = parameter.Value;
                    }
                    else if ( parameter.Key.EqualsIgnoreCase(OrderByParameterKey) )
                    {
                        AddOrderByItem(parameter);
                    }
                    else
                    {
                        AddFilterItem(parameter);
                    }
                }

                if ( Page.HasValue && Take.HasValue && !Skip.HasValue )
                {
                    Skip = (Page.Value - 1) * Take.Value;
                }

                _selectListLookup = new HashSet<string>();
                BuildSelectListLookup();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string EntityName { get; private set; }
            public bool IsCountOnly { get; private set; }
            public IList<QuerySelectItem> SelectPropertyNames { get; private set; }
            public IList<QuerySelectItem> IncludePropertyNames { get; private set; }
            public IList<QueryFilterItem> Filter { get; private set; }
            public IList<QueryFilterItem> InMemoryFilter { get; private set; }
            public IList<QueryOrderByItem> OrderBy { get; private set; }
            public IList<QueryOrderByItem> InMemoryOrderBy { get; private set; }
            public IList<QuerySelectItem> InMemoryAggregations { get; private set; }
            public int? Skip { get; private set; }
            public int? Take { get; private set; }
            public int? Page { get; private set; }
            public string OfType { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsPropertyIncludedInSelectList(string propertyName)
            {
                return (_selectListLookup.Count == 0 || _selectListLookup.Contains(propertyName));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool NeedInMemoryOperations
            {
                get { return (InMemoryFilter.Count > 0 || InMemoryOrderBy.Count > 0); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool NeedCountOperation
            {
                get { return (IsCountOnly || Page.HasValue); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool NeedAggregations
            {
                get
                {
                    return (
                        SelectPropertyNames.Any(s => s.IsAggregation) ||
                        IncludePropertyNames.Any(s => s.IsAggregation));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal string BuildCacheKey()
            {
                if ( _cacheKey != null )
                {
                    return _cacheKey;
                }

                var key = new StringBuilder();

                key.Append(EntityName);

                BuildListCacheKey(SelectPropertyNames.OrderBy(p => p.AliasName), key);
                BuildListCacheKey(IncludePropertyNames.OrderBy(p => p.AliasName), key);

                key.Append("/"); key.Append(OfType);
                key.Append("/"); key.Append(IsCountOnly);

                _cacheKey = key.ToString().ToLower();
                return _cacheKey;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal static void BuildListCacheKey<T>(IEnumerable<T> list, StringBuilder key) where T : IBuildCacheKey
            {
                key.Append("/[");

                if ( list != null )
                {
                    bool firstItem = true;

                    foreach ( var item in list )
                    {
                        if ( !firstItem )
                        {
                            key.Append(",");
                        }

                        item.BuildCacheKey(key);
                        firstItem = false;
                    }
                }

                key.Append("]");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private List<QuerySelectItem> ParsePropertyList(string parameterValue)
            {
                var destination = new List<QuerySelectItem>();
                var propertySpecifiers = parameterValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach ( var specifier in propertySpecifiers )
                {
                    var selectItem = new QuerySelectItem(specifier);
                    destination.Add(selectItem);

                    if ( selectItem.AggregationType != AggregationType.None )
                    {
                        this.InMemoryAggregations.Add(selectItem);
                    }
                }

                return destination;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AddOrderByItem(KeyValuePair<string, string> parameter)
            {
                var subParams = parameter.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach ( var subParam in subParams )
                {
                    OrderBy.Add(new QueryOrderByItem(subParam));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AddFilterItem(KeyValuePair<string, string> parameter)
            {
                string propertyName;
                string @operator;

                var operatorIndex = parameter.Key.IndexOf(':');

                if ( operatorIndex > 0 )
                {
                    propertyName = parameter.Key.Substring(0, operatorIndex);
                    @operator = parameter.Key.Substring(operatorIndex);
                }
                else
                {
                    propertyName = parameter.Key;
                    @operator = EqualOperator;
                }

                var filterItem = new QueryFilterItem(propertyName, @operator, parameter.Value);
                this.Filter.Add(filterItem);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void BuildSelectListLookup()
            {
                foreach (var selectItem in SelectPropertyNames.Concat(IncludePropertyNames))
                {
                    _selectListLookup.Add(selectItem.AliasName);
                    _selectListLookup.UnionWith(selectItem.AliasName.Split('.'));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EntityHandler CreateAdHocEntityHandler(string entityQualifiedName)
        {
            var metaType = _metadataCache.GetTypeMetadata(entityQualifiedName);
            return new AdHocEntityHandler(this, metaType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal interface IBuildCacheKey
        {
            void BuildCacheKey(StringBuilder key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QuerySelectItem : IBuildCacheKey
        {
            private IReadOnlyList<IPropertyMetadata> _metaPropertyPath;
            private bool _needsJoinOperation;
            private bool _needsForeignKeyNavigation;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QuerySelectItem(string propertySpecifier)
            {
                var aliasPosition = propertySpecifier.IndexOf(QueryOptions.PropertyAliasModifier, StringComparison.InvariantCultureIgnoreCase);
                var pathAndAggregation = (aliasPosition > 0 ? propertySpecifier.Substring(0, aliasPosition) : propertySpecifier);

                var aggregationPosition = pathAndAggregation.IndexOf(QueryOptions.PropertyAggregationSeparator, StringComparison.InvariantCultureIgnoreCase);
                var aggregationString = (aggregationPosition > 0 ? pathAndAggregation.Substring(aggregationPosition + 1) : null);

                if ( !string.IsNullOrEmpty(aggregationString) )
                {
                    this.AggregationType = (AggregationType)Enum.Parse(typeof(AggregationType), aggregationString, ignoreCase: true);
                }

                var propertyPathString = (aggregationPosition > 0 ? pathAndAggregation.Substring(0, aggregationPosition) : pathAndAggregation);

                this.PropertyPath = propertyPathString.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                if ( aliasPosition > 0 )
                {
                    this.AliasName = propertySpecifier.Substring(aliasPosition + QueryOptions.PropertyAliasModifier.Length);
                }
                else
                {
                    this.AliasName = propertyPathString;
                }

                if ( propertySpecifier.EqualsIgnoreCase("$type") )
                {
                    this.SpecialName = FieldSpecialName.Type;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IReadOnlyList<string> PropertyPath { get; private set; }
            public string AliasName { get; private set; }
            public AggregationType AggregationType { get; private set; }
            public FieldSpecialName SpecialName { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsAggregation
            {
                get { return (AggregationType != AggregationType.None); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IPropertyMetadata MetaProperty
            {
                get
                {
                    if ( _metaPropertyPath != null )
                    {
                        return _metaPropertyPath[_metaPropertyPath.Count - 1];
                    }

                    return null;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool NeedsJoinOperation
            {
                get
                {
                    if (SpecialName != FieldSpecialName.None)
                    {
                        return false;
                    }

                    if (_metaPropertyPath != null)
                    {
                        return _needsJoinOperation;
                    }

                    throw new InvalidOperationException("BuildMetaPropertyPath was not invoked on this QuerySelectItem instance.");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool NeedsForeignKeyNavigation
            {
                get
                {
                    if (SpecialName != FieldSpecialName.None)
                    {
                        return false;
                    }

                    if (_metaPropertyPath != null)
                    {
                        return _needsForeignKeyNavigation;
                    }

                    throw new InvalidOperationException("BuildMetaPropertyPath was not invoked on this QuerySelectItem instance.");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool NeedsForeignKeyDisplayName
            {
                get
                {
                    var property = this.MetaProperty;

                    if (property == null)
                    {
                        throw new InvalidOperationException("BuildMetaPropertyPath was not invoked on this QuerySelectItem instance.");
                    }

                    return (
                        PropertyPath.Count == 1 &&
                        !property.IsCollection &&
                        property.Relation != null &&
                        property.Relation.RelatedPartyType != null && 
                        property.Relation.RelatedPartyType.DisplayNameProperty != null);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IBuildCacheKey.BuildCacheKey(StringBuilder key)
            {
                key.Append("/[");
                key.Append(string.Join(",", PropertyPath));
                key.Append("]/");
                if ( this.AggregationType != AggregationType.None )
                {
                    key.Append(this.AggregationType.ToString());
                }
                key.Append("/");
                key.Append(AliasName);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal object ReadValue(QueryContext queryContext, ITypeMetadata metaType, object target, int skipSteps = 0, int makeSteps = 0)
            {
                if ( SpecialName == FieldSpecialName.Type )
                {
                    return ((IObject)target).ContractType.Name.TrimPrefix("I").TrimSuffix("Entity");
                }

                var stepTarget = target;
                var stepMetaType = metaType;
                var subsetOfPropertyPath = GetSubsetOfPropertyPath(PropertyPath, skipSteps, makeSteps);
                object value = null;

                foreach ( var stepPropertyName in subsetOfPropertyPath )
                {
                    var stepMetaProperty = stepMetaType.FindPropertyByNameIncludingDerivedTypes(stepPropertyName);
                    value = stepMetaProperty.ReadValue(stepTarget);

                    if ( stepMetaProperty.Relation != null )
                    {
                        if ( !(value is IDomainObject) )
                        {
                            var leftJoin = queryContext.GetLeftJoinForNavigation(stepMetaProperty);
                            var leftSideIdProperty = stepMetaProperty.DeclaringContract.EntityIdProperty;
                            var leftSideIdValue = leftSideIdProperty.ReadValue(stepTarget);

                            if ( !leftJoin.TryGetRightSideValue(leftSideIdValue, out value) || value == null )
                            {
                                return null;
                            }
                        }

                        stepTarget = value;
                        stepMetaType = stepMetaProperty.Relation.RelatedPartyType;
                    }
                    else
                    {
                        break;
                    }
                }

                return value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal IReadOnlyList<IPropertyMetadata> BuildMetaPropertyPath(ITypeMetadata metaType)
            {
                if ( _metaPropertyPath != null )
                {
                    return _metaPropertyPath;
                }

                if ( SpecialName != FieldSpecialName.None )
                {
                    return null;
                }

                var metaPropertyPath = new List<IPropertyMetadata>();
                var stepMetaType = metaType;

                for ( int i = 0 ; i < PropertyPath.Count ; i++ )
                {
                    var isLastStep = (i == PropertyPath.Count - 1);
                    var stepMetaProperty = stepMetaType.FindPropertyByNameIncludingDerivedTypes(PropertyPath[i]);
                    metaPropertyPath.Add(stepMetaProperty);

                    if ( stepMetaProperty.Relation != null )
                    {
                        stepMetaType = stepMetaProperty.Relation.RelatedPartyType;

                        if ( stepMetaProperty.ClrType != stepMetaProperty.Relation.RelatedPartyType.ContractType )
                        {
                            _needsJoinOperation = !isLastStep;
                        }
                        else if (stepMetaProperty.RelationalMapping != null && stepMetaProperty.RelationalMapping.IsForeignKeyEmbeddedInParent)
                        {
                            _needsForeignKeyNavigation = !isLastStep;
                        }
                    }
                    else if ( !isLastStep )
                    {
                        throw new ArgumentException(string.Format(
                            "Invalid property path for entity '{0}': {1}", metaType.QualifiedName, string.Join(".", PropertyPath)));
                    }
                }

                _metaPropertyPath = metaPropertyPath;
                return metaPropertyPath;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal bool IsForeignKeyDisplayNameFor(IPropertyMetadata navigationProperty)
            {
                return (
                    navigationProperty.Relation != null &&
                    navigationProperty.Relation.RelatedPartyType != null &&
                    navigationProperty.Relation.RelatedPartyType.DisplayNameProperty != null &&
                    PropertyPath.Count == 2 &&
                    PropertyPath[0] == navigationProperty.Name &&
                    PropertyPath[1] == navigationProperty.Relation.RelatedPartyType.DisplayNameProperty.Name);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IEnumerable<string> GetSubsetOfPropertyPath(IReadOnlyList<string> path, int skipSteps, int makeSteps)
            {
                IEnumerable<string> subset = path;

                if ( skipSteps > 0 )
                {
                    subset = subset.Skip(skipSteps);
                }

                if ( makeSteps > 0 )
                {
                    subset = subset.Take(makeSteps);
                }

                return subset;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryFilterItem : IBuildCacheKey
        {
            public QueryFilterItem(string propertyName, string @operator, string stringValue)
            {
                PropertyName = propertyName;
                Operator = @operator;
                StringValue = stringValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Expression<Func<TEntity, bool>> MakePredicateExpression<TEntity>(bool inMemory)
            {
                if ( MetaProperty == null )
                {
                    throw new InvalidOperationException("MetaProperty must be set before calling this method.");
                }

                var expressionFactory = (
                    inMemory
                    ? _s_binaryExpressionFactoryByInMemoryOperator[Operator] 
                    : _s_binaryExpressionFactoryByStorageOperator[Operator]);

                if ( MetaProperty.Kind == PropertyKind.Scalar )
                {
                    return MetaProperty.MakeBinaryExpression<TEntity>(NavigationMetaPath, StringValue, expressionFactory);
                }
                else if ( MetaProperty.Kind == PropertyKind.Relation )
                {
                    return MetaProperty.MakeForeignKeyBinaryExpression<TEntity>(NavigationMetaPath, StringValue, expressionFactory);
                }

                throw new NotSupportedException("Cannot create filter expression for property of kind: " + MetaProperty.Kind);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string PropertyName { get; private set; }
            public string Operator { get; private set; }
            public string StringValue { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IPropertyMetadata[] NavigationMetaPath { get; set; }
            public IPropertyMetadata MetaProperty { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IBuildCacheKey.BuildCacheKey(StringBuilder key)
            {
                key.Append("/"); key.Append(PropertyName);
                key.Append(":"); key.Append(Operator);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static readonly MethodInfo _s_stringStartsWithMethod =
                ExpressionUtility.GetMethodInfo<Expression<Func<string, string, bool>>>((s, value) => s.StartsWith(value));

            private static readonly MethodInfo _s_stringEndsWithMethod =
                ExpressionUtility.GetMethodInfo<Expression<Func<string, string, bool>>>((s, value) => s.EndsWith(value));

            private static readonly MethodInfo _s_stringContainsMethod =
                ExpressionUtility.GetMethodInfo<Expression<Func<string, string, bool>>>((s, value) => s.Contains(value));

            private static readonly MethodInfo _s_stringToLowerMethod =
                ExpressionUtility.GetMethodInfo<Expression<Func<string, string>>>(s => s.ToLower());

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static readonly MethodInfo _s_stringExtensionsStartsWithIgnoreCaseMethod =
                ExpressionUtility.GetMethodInfo<Expression<Func<string, string, bool>>>((s, value) => 
                    NWheels.Extensions.StringExtensions.StartsWithIgnoreCase(s, value));

            private static readonly MethodInfo _s_stringExtensionsEndsWithIgnoreCaseMethod =
                ExpressionUtility.GetMethodInfo<Expression<Func<string, string, bool>>>((s, value) =>
                    NWheels.Extensions.StringExtensions.EndsWithIgnoreCase(s, value));

            private static readonly MethodInfo _s_stringExtensionsContainsIgnoreCaseMethod =
                ExpressionUtility.GetMethodInfo<Expression<Func<string, string, bool>>>((s, value) =>
                    NWheels.Extensions.StringExtensions.ContainsIgnoreCase(s, value));

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static readonly Dictionary<string, Func<Expression, Expression, Expression>> _s_binaryExpressionFactoryByStorageOperator =
                new Dictionary<string, Func<Expression, Expression, Expression>>(StringComparer.InvariantCultureIgnoreCase) {
                    { QueryOptions.EqualOperator, Expression.Equal },
                    { QueryOptions.NotEqualOperator, Expression.NotEqual },
                    { QueryOptions.GreaterThanOperator, Expression.GreaterThan },
                    { QueryOptions.GreaterThanOrEqualOperator, Expression.GreaterThanOrEqual },
                    { QueryOptions.LessThanOperator, Expression.LessThan },
                    { QueryOptions.LessThanOrEqualOperator, Expression.LessThanOrEqual },
                    { QueryOptions.StringContainsOperator, (left, right) => 
                        Expression.Call(Expression.Call(left, _s_stringToLowerMethod), _s_stringContainsMethod, right) },
                    { QueryOptions.StringStartsWithOperator, (left, right) => 
                        Expression.Call(Expression.Call(left, _s_stringToLowerMethod), _s_stringStartsWithMethod, right) },
                    { QueryOptions.StringEndsWithOperator, (left, right) => 
                        Expression.Call(Expression.Call(left, _s_stringToLowerMethod), _s_stringEndsWithMethod, right) },
                };

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static readonly Dictionary<string, Func<Expression, Expression, Expression>> _s_binaryExpressionFactoryByInMemoryOperator =
                new Dictionary<string, Func<Expression, Expression, Expression>>(StringComparer.InvariantCultureIgnoreCase) {
                    { QueryOptions.EqualOperator, Expression.Equal },
                    { QueryOptions.NotEqualOperator, Expression.NotEqual },
                    { QueryOptions.GreaterThanOperator, Expression.GreaterThan },
                    { QueryOptions.GreaterThanOrEqualOperator, Expression.GreaterThanOrEqual },
                    { QueryOptions.LessThanOperator, Expression.LessThan },
                    { QueryOptions.LessThanOrEqualOperator, Expression.LessThanOrEqual },
                    { QueryOptions.StringContainsOperator, (left, right) => Expression.Call(_s_stringExtensionsContainsIgnoreCaseMethod, left, right) },
                    { QueryOptions.StringStartsWithOperator, (left, right) => Expression.Call(_s_stringExtensionsStartsWithIgnoreCaseMethod, left, right) },
                    { QueryOptions.StringEndsWithOperator, (left, right) => Expression.Call(_s_stringExtensionsEndsWithIgnoreCaseMethod, left, right) },
                };

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static readonly Dictionary<ExpressionType, string> _s_binaryOperatorByExpressionType =
                new Dictionary<ExpressionType, string>() {
                    { ExpressionType.Equal, QueryOptions.EqualOperator },
                    { ExpressionType.NotEqual, QueryOptions.NotEqualOperator },
                    { ExpressionType.GreaterThan, QueryOptions.GreaterThanOperator },
                    { ExpressionType.GreaterThanOrEqual, QueryOptions.GreaterThanOrEqualOperator },
                    { ExpressionType.LessThan, QueryOptions.LessThanOperator },
                    { ExpressionType.LessThanOrEqual, QueryOptions.LessThanOrEqualOperator }
                };

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static string GetOperatorFromExpression(ExpressionType nodeType)
            {
                return _s_binaryOperatorByExpressionType[nodeType];
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryOrderByItem : IBuildCacheKey
        {
            public QueryOrderByItem(string propertyName, bool @ascending)
            {
                PropertyName = propertyName;
                Ascending = @ascending;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryOrderByItem(string parameterValue)
            {
                if ( parameterValue.EndsWith(QueryOptions.DescendingParameterModifier, ignoreCase: true, culture: CultureInfo.InvariantCulture) )
                {
                    PropertyName = parameterValue.Substring(0, parameterValue.Length - QueryOptions.DescendingParameterModifier.Length);
                    Ascending = false;
                }
                else if ( parameterValue.EndsWith(QueryOptions.AscendingParameterModifier, ignoreCase: true, culture: CultureInfo.InvariantCulture) )
                {
                    PropertyName = parameterValue.Substring(0, parameterValue.Length - QueryOptions.AscendingParameterModifier.Length);
                    Ascending = true;
                }
                else
                {
                    PropertyName = parameterValue;
                    Ascending = true;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable<TEntity> ApplyToQuery<TEntity>(IQueryable<TEntity> query, bool first)
            {
                //throw new NotSupportedException();

                if (MetaProperty == null)
                {
                    throw new InvalidOperationException("MetaProperty must be set before calling this method.");
                }

                return MetaProperty.MakeOrderBy(this.NavigationMetaPath, query, first, ascending: this.Ascending);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string PropertyName { get; private set; }
            public bool Ascending { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IPropertyMetadata[] NavigationMetaPath { get; set; }
            public IPropertyMetadata MetaProperty { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IBuildCacheKey.BuildCacheKey(StringBuilder key)
            {
                key.Append("/"); key.Append(PropertyName);
                key.Append("/"); key.Append(Ascending);
            }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class LeftJoinOperation
        {
            private readonly ApplicationEntityService _ownerService;
            private readonly Func<IEnumerable<object>> _sourceFactory;
            private readonly IPropertyMetadata _navigation;
            private readonly Dictionary<object, object> _rightSideValueByLeftSideId;
            private bool _executed;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LeftJoinOperation(ApplicationEntityService ownerService, Func<IEnumerable<object>> sourceFactory, IPropertyMetadata navigation)
            {
                _ownerService = ownerService;
                _sourceFactory = sourceFactory;
                _navigation = navigation;
                _rightSideValueByLeftSideId = new Dictionary<object, object>();
                _executed = false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool TryGetRightSideValue(object leftSideId, out object rightSideValue)
            {
                if ( !_executed )
                {
                    Execute();
                }

                return _rightSideValueByLeftSideId.TryGetValue(leftSideId, out rightSideValue);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IPropertyMetadata Navigation
            {
                get
                {
                    return _navigation;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<object> RightSideResults
            {
                get
                {
                    return _rightSideValueByLeftSideId.Values.Distinct();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void Execute()
            {
                var source = _sourceFactory().ToArray();
                var leftSideIdProperty = _navigation.DeclaringContract.EntityIdProperty;
                var rightSideIdProperty = _navigation.Relation.RelatedPartyType.EntityIdProperty;
                var rightSideHandler = _ownerService._handlerByEntityName[_navigation.Relation.RelatedPartyType.QualifiedName];

                if ( _navigation.Kind == PropertyKind.Relation || _navigation.Kind == PropertyKind.Part )
                {
                    //TODO: implement one-shot left join, instead
                    foreach ( var leftSideObject in source )
                    {
                        var leftSideId = leftSideIdProperty.ReadValue(leftSideObject);
                        var rightSideObject = _navigation.ReadValue(leftSideObject);
                        _rightSideValueByLeftSideId[leftSideId] = rightSideObject;
                    }
                }
                else if ( _navigation.RelationalMapping == null || _navigation.RelationalMapping.IsForeignKeyEmbeddedInParent )
                {
                    var rightSideIds = source.Select(s => _navigation.ReadValue(s)).ToArray();
                    var rightSideById = rightSideHandler.GetByIdList(rightSideIds).ToDictionary(r => rightSideIdProperty.ReadValue(r));

                    for ( int i = 0 ; i < source.Length ; i++ )
                    {
                        var leftSideId = leftSideIdProperty.ReadValue(source[i]);
                        var rightSideId = rightSideIds[i];

                        IDomainObject rightSideValue;
                        if (rightSideById.TryGetValue(rightSideId, out rightSideValue))
                        {
                            _rightSideValueByLeftSideId[leftSideId] = rightSideValue;
                        }
                    }
                }
                else 
                {
                    var leftSideIds = source.Select(s => leftSideIdProperty.ReadValue(s)).ToArray();
                    var rightSideByLeftSideId = rightSideHandler
                        .GetByForeignKeyList(_navigation.Relation.InverseProperty, leftSideIds)
                        .ToDictionary(r => _navigation.Relation.InverseProperty.ReadValue(r));

                    foreach ( var kvp in rightSideByLeftSideId )
                    {
                        _rightSideValueByLeftSideId[kvp.Key] = kvp.Value;
                    }
                }

                _executed = true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class EntityHandler
        {
            private readonly IEntityHandlerExtension[] _extensions;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected EntityHandler(ApplicationEntityService owner, ITypeMetadata metaType, Type domainContextType, IEntityHandlerExtension[] extensions)
            {
                _extensions = extensions;
                this.Owner = owner;
                this.MetaType = metaType;
                this.DomainContextType = domainContextType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract IUnitOfWork NewUnitOfWork(object txViewModel = null, bool debugPerformStaleCheck = false);
            public abstract QueryResults Query(QueryOptions options, IQueryable query = null, object txViewModel = null);
            public abstract EntityCursor QueryCursor(QueryOptions options, IQueryable query = null, object txViewModel = null);
            public abstract IEntityId ParseEntityId(string id);
            public abstract IDomainObject GetById(string id);
            public abstract IDomainObject[] GetByIdList(object[] idList);
            public abstract IDomainObject[] GetByForeignKeyList(IPropertyMetadata inverseForeignKeyProperty, object[] leftSideIds);
            public abstract bool TryGetById(string id, out IDomainObject entity);
            public abstract IDomainObject CreateNew();
            public abstract void Insert(IDomainObject entity);
            public abstract void Update(IDomainObject entity);
            public abstract void Delete(string id);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AuthorizationCheckResults CheckAuthorization(string entityId = null)
            {
                if ( !MetaType.IsEntity )
                {
                    return AuthorizationCheckResults.AllTrue();
                }

                using ( var domainContext = NewUnitOfWork(debugPerformStaleCheck: true) )
                {
                    var accessControl = Framework.CurrentIdentity.GetAccessControlList().GetEntityAccessControl(MetaType.ContractType);
                    var authorizationContext = (IAccessControlContext)domainContext;
                    AuthorizationCheckResults checkResults;

                    if (entityId != null)
                    {
                        checkResults = CheckAuthorizationForEntityInstance(entityId, accessControl, authorizationContext);
                    }
                    else
                    {
                        checkResults = CheckAuthorizationForEntityType(accessControl, authorizationContext);
                    }

                    return checkResults;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ApplicationEntityService Owner { get; private set; }
            public ITypeMetadata MetaType { get; private set; }
            public Type DomainContextType { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IFramework Framework
            {
                get { return Owner._framework; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IDomainContextLogger DomainContextLogger
            {
                get { return Owner._domainContextLogger; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IReadOnlyList<IEntityHandlerExtension> Extensions
            {
                get { return _extensions; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private AuthorizationCheckResults CheckAuthorizationForEntityType(IEntityAccessControl accessControl, IAccessControlContext authorizationContext)
            {
                var checkResults = new AuthorizationCheckResults() {
                    CanRetrieve = accessControl.CanRetrieve(authorizationContext).GetValueOrDefault(false),
                    CanCreate = accessControl.CanInsert(authorizationContext).GetValueOrDefault(false),
                    CanUpdate = accessControl.CanUpdate(authorizationContext).GetValueOrDefault(false),
                    CanDelete = accessControl.CanDelete(authorizationContext).GetValueOrDefault(false),
                };
                return checkResults;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private AuthorizationCheckResults CheckAuthorizationForEntityInstance(
                string entityId,
                IEntityAccessControl accessControl,
                IAccessControlContext authorizationContext)
            {
                AuthorizationCheckResults checkResults;
                IDomainObject entity;

                if (TryGetById(entityId, out entity))
                {
                    checkResults = new AuthorizationCheckResults() {
                        CanRetrieve = accessControl.CanRetrieve(authorizationContext, entity).GetValueOrDefault(false),
                        CanCreate = accessControl.CanInsert(authorizationContext, entity).GetValueOrDefault(false),
                        CanUpdate = accessControl.CanUpdate(authorizationContext, entity).GetValueOrDefault(false),
                        CanDelete = accessControl.CanDelete(authorizationContext, entity).GetValueOrDefault(false),
                    };

                    var selfAccessControl = (entity as IEntitySelfAccessControl);

                    if (selfAccessControl != null)
                    {
                        var instanceMetaType = this.Owner._metadataCache.GetTypeMetadata(entity.ContractType);
                        var memberAccessControl = new OutputEntityPropertyAccessControl(instanceMetaType);
                        selfAccessControl.SetMemberAccessControl(memberAccessControl);

                        checkResults.CanUpdate &= selfAccessControl.CanUpdateEntity.GetValueOrDefault(true);
                        checkResults.CanDelete &= selfAccessControl.CanDeleteEntity.GetValueOrDefault(true);
                        checkResults.IsRestrictedEntry = true;
                        checkResults.RestrictedEntryProperties = memberAccessControl.GetPropertiesAllowedToChange();
                        checkResults.EnabledOperations = memberAccessControl.GetMethodsAllowedToInvoke();
                    }

                    if (checkResults.CanRetrieve)
                    {
                        checkResults.FullEntity = entity;
                    }
                }
                else
                {
                    checkResults = AuthorizationCheckResults.AllFalse();
                }

                return checkResults;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static EntityHandler Create(
                ApplicationEntityService owner, 
                ITypeMetadata metaType, 
                Type domainContextType, 
                IEntityHandlerExtension[] extensions)
            {
                EntityHandler handler;
                
                if (!TryCreateFromExtensions(owner, metaType, domainContextType, extensions, out handler))
                {
                    handler = CreateDefault(owner, metaType, domainContextType, extensions);
                }

                return handler;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static EntityHandler CreateDefault(
                ApplicationEntityService owner,
                ITypeMetadata metaType,
                Type domainContextType,
                IEntityHandlerExtension[] extensions)
            {
                var concreteClosedType = typeof(EntityHandler<,>).MakeGenericType(domainContextType, metaType.ContractType);
                return (EntityHandler)Activator.CreateInstance(concreteClosedType, owner, metaType, domainContextType, extensions);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private static bool TryCreateFromExtensions(
                ApplicationEntityService owner,
                ITypeMetadata metaType,
                Type domainContextType,
                IEntityHandlerExtension[] extensions,
                out EntityHandler handler)
            {
                for (int i = 0 ; i < extensions.Length ; i++)
                {
                    if (extensions[i].CanCreateEntityHandler)
                    {
                        {
                            handler = extensions[i].CreateEntityHandler(owner, metaType, domainContextType, extensions);
                            return true;
                        }
                    }
                }

                handler = null;
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityHandler<TContext, TEntity> : EntityHandler
            where TContext : class, IApplicationDataRepository
            where TEntity : class
        {
            public EntityHandler(
                ApplicationEntityService owner, 
                ITypeMetadata metaType, 
                Type domainContextType,
                IEntityHandlerExtension[] extensions)
                : base(owner, metaType, domainContextType, extensions)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IUnitOfWork NewUnitOfWork(object txViewModel = null, bool debugPerformStaleCheck = false)
            {
                //TODO: remove once solid test coverage is provided
                if (debugPerformStaleCheck)
                {
                    //TODO: remove this once we are sure the bug is solved
                    var anchor = new ThreadStaticAnchor<PerContextResourceConsumerScope<TContext>>();
                    var stale = anchor.Current;

                    if (stale != null)
                    {
                        DomainContextLogger.StaleUnitOfWorkEncountered(
                            stale.Resource.ToString(),
                            ((DataRepositoryBase)(object)stale.Resource).InitializerThreadText);

                        //ahcnor.Clear(); //!!!dangerous - if false positive, the scopes will be messed up
                    }
                }

                var extensionToUse = Extensions.FirstOrDefault(x => x.CanOpenNewUnitOfWork(txViewModel));

                if (extensionToUse != null)
                {
                    return extensionToUse.OpenNewUnitOfWork(txViewModel);
                }

                return Framework.NewUnitOfWork<TContext>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //TODO: refactor to reuse QueryCursor(...)
            public override QueryResults Query(QueryOptions options, IQueryable query = null, object txViewModel = null)
            {
                var results = QueryContext.Current.Results;
                results.Visualization = TryGetVisualizationFrom(query);

                using (var contextOrNull = NewUnitOfWork(txViewModel) as TContext)
                {
                    IQueryable<TEntity> dbQuery;

                    if (contextOrNull != null)
                    {
                        var repository = contextOrNull.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                        dbQuery = (IQueryable<TEntity>)query ?? repository.AsQueryable();
                    }
                    else
                    {
                        dbQuery = (IQueryable<TEntity>)query;
                    }

                    dbQuery = HandleFilter(options, dbQuery);

                    if ( options.NeedCountOperation )
                    {
                        results.ResultCount = dbQuery.Count();
                    }

                    if ( !options.IsCountOnly || options.NeedInMemoryOperations )
                    {
                        if ( !options.NeedAggregations )
                        {
                            dbQuery = HandleOrderBy(options, dbQuery);
                            dbQuery = HandlePaging(options, dbQuery);
                        }

                        IEnumerable<TEntity> queryResults = new QueryResultEnumerable<TEntity>(dbQuery);

                        if ( options.NeedInMemoryOperations )
                        {
                            queryResults = HandleInMemoryOperations(options, queryResults);
                        }

                        if ( options.IsCountOnly )
                        {
                            results.ResultCount = queryResults.Count();
                        }
                        else if ( options.NeedAggregations )
                        {
                            ExecuteInMemoryAggregation(options, results, queryResults);
                        }
                        else
                        {
                            if ( options.Take.HasValue )
                            {
                                var buffer = queryResults.ToList();
                                results.MoreAvailable = (buffer.Count > options.Take.Value);
                                results.ResultSet = buffer.Take(options.Take.Value).Cast<IDomainObject>().ToArray();
                            }
                            else
                            {
                                results.ResultSet = queryResults.Cast<IDomainObject>().ToArray();
                            }
                        }
                    }
                }

                if ( options.Page.HasValue && options.Take.HasValue )
                {
                    results.PageNumber = options.Page;
                    results.PageSize = options.Take;
                }

                return results;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override EntityCursor QueryCursor(QueryOptions options, IQueryable query = null, object txViewModel = null)
            {
                using (var context = NewUnitOfWork(txViewModel) as TContext)
                {
                    var dbQuery = GetCursorDbQuery(query, context);

                    dbQuery = HandleFilter(options, dbQuery);
                    dbQuery = HandleOrderBy(options, dbQuery);

                    IEnumerable<TEntity> queryResults = new QueryResultEnumerable<TEntity>(dbQuery);

                    if ( options.NeedInMemoryOperations )
                    {
                        queryResults = HandleInMemoryOperations(options, queryResults);
                    }

                    var queryContext = QueryContext.Current;
                    queryContext.Results.ResultSet = queryResults.Cast<object>().ToArray();

                    var metaCursor = new EntityCursorMetadata(queryContext);
                    return new EntityCursor(metaCursor, queryContext, this, queryResults.Cast<IDomainObject>());
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ExecuteInMemoryAggregation(QueryOptions options, QueryResults results, IEnumerable<TEntity> dbCursor)
            {
                var queryContext = QueryContext.Current;
                var aggregator = queryContext.Aggregator;

                foreach ( var record in dbCursor )
                {
                    aggregator.Aggregate(record.As<IDomainObject>());
                }

                results.ResultSet = new object[] { aggregator };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IEnumerable<TEntity> HandleInMemoryOperations(QueryOptions options, IEnumerable<TEntity> dbCursor)
            {
                IQueryable<TEntity> inMemoryQuery = dbCursor.AsQueryable();

                foreach ( var filterItem in options.InMemoryFilter )
                {
                    inMemoryQuery = inMemoryQuery.Where(filterItem.MakePredicateExpression<TEntity>(inMemory: true));
                }

                for ( int i = 0 ; i < options.InMemoryOrderBy.Count ; i++ )
                {
                    inMemoryQuery = options.InMemoryOrderBy[i].ApplyToQuery(inMemoryQuery, first: i == 0);
                }

                if ( options.Skip.HasValue )
                {
                    inMemoryQuery = inMemoryQuery.Skip(options.Skip.Value);
                }

                if ( options.Take.HasValue )
                {
                    inMemoryQuery = inMemoryQuery.Take(options.Take.Value + 1);
                }

                return inMemoryQuery;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEntityId ParseEntityId(string id)
            {
                var entityIdProperty = MetaType.EntityIdProperty;
                var entityIdValue = entityIdProperty.ParseStringValue(id);
                var entityIdClosedType = typeof(EntityId<,>).MakeGenericType(MetaType.ContractType, entityIdProperty.ClrType);
                var entityIdInstance = Activator.CreateInstance(entityIdClosedType, entityIdValue);

                return (IEntityId)entityIdInstance;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IDomainObject GetById(string id)
            {
                IDomainObject entity;

                if ( TryGetById(id, out entity) )
                {
                    return entity;
                }

                throw new ArgumentException("Specified entity does not exist.");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IDomainObject[] GetByIdList(object[] idList)
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var repository = context.GetEntityRepository(typeof(TEntity));
                    var results = repository.GetByIdList(idList);
                    return results.Cast<IDomainObject>().ToArray();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IDomainObject[] GetByForeignKeyList(IPropertyMetadata inverseForeignKeyProperty, object[] leftSideIds)
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var repository = context.GetEntityRepository(typeof(TEntity));
                    var results = repository.GetByForeignKeyList(inverseForeignKeyProperty, leftSideIds);
                    return results.Cast<IDomainObject>().ToArray();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool TryGetById(string id, out IDomainObject entity)
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var repository = context.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                    var idProperty = MetaType.PrimaryKey.Properties[0];
                    IQueryable<TEntity> query = repository.AsQueryable().Where(idProperty.MakeBinaryExpression<TEntity>(
                        navigationPath: new[] { idProperty }, 
                        valueString: id, 
                        binaryFactory: Expression.Equal));

                    var result = query.FirstOrDefault();

                    entity = result as IDomainObject;
                    return (entity != null);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IDomainObject CreateNew()
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    IEntityRepository repository;

                    if ( context.TryGetEntityRepository(typeof(TEntity), out repository) )
                    {
                        var domainObject = repository.As<IEntityRepository<TEntity>>().New();
                        return (IDomainObject)domainObject;
                    }
                    else if ( MetaType.IsEntityPart )
                    {
                        var domainObject = Framework.As<ICoreFramework>().NewDomainObject<TEntity>(((DataRepositoryBase)(object)context).Components);
                        return (IDomainObject)domainObject;
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format(
                            "Entity repository for entity '{0}' is not declared in domain context.", 
                            MetaType.QualifiedName));
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Insert(IDomainObject entity)
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var repository = context.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                    repository.Insert((TEntity)entity);
                    context.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Update(IDomainObject entity)
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var repository = context.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                    repository.Update((TEntity)entity);
                    context.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Delete(string id)
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var entity = GetById(id);
                    var repository = context.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                    repository.Delete((TEntity)entity);
                    context.CommitChanges();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IQueryable<TEntity> GetCursorDbQuery(IQueryable query, TContext context)
            {
                IQueryable<TEntity> dbQuery;

                if (query != null)
                {
                    dbQuery = (IQueryable<TEntity>)query;
                }
                else
                {
                    var repository = context.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                    dbQuery = repository.AsQueryable();
                }
                return dbQuery;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ExecuteQuery(IQueryable<TEntity> query, QueryOptions options, out TEntity[] resultSet, out long resultCount)
            {
                if ( options.IsCountOnly )
                {
                    resultCount = query.Count();
                    resultSet = null;
                }
                else
                {
                    resultSet = query.ToArray().ToArray();
                    resultCount = resultSet.Length;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IQueryable<TEntity> HandleOrderBy(QueryOptions options, IQueryable<TEntity> dbQuery)
            {
                for ( int i = 0 ; i < options.OrderBy.Count ; )
                {
                    var orderItem = options.OrderBy[i];
                    var navigationMetaPath = BuildNavigationMetaPath(MetaType, orderItem.PropertyName);
                    var metaProperty = navigationMetaPath.Last();

                    orderItem.NavigationMetaPath = navigationMetaPath;
                    orderItem.MetaProperty = metaProperty;

                    if ( metaProperty.IsCalculated )
                    {
                        options.OrderBy.RemoveAt(i);
                        options.InMemoryOrderBy.Add(orderItem);
                    }
                    else
                    {
                        dbQuery = metaProperty.MakeOrderBy(navigationMetaPath, dbQuery, first: i == 0, ascending: orderItem.Ascending);
                        i++;
                    }
                }

                return dbQuery;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IQueryable<TEntity> HandleFilter(QueryOptions options, IQueryable<TEntity> dbQuery)
            {
                if ( !string.IsNullOrEmpty(options.OfType) )
                {
                    dbQuery = dbQuery.OfMetaType(this.MetaType, derivedTypeString: options.OfType);
                }

                for ( int i = 0 ; i < options.Filter.Count ; )
                {
                    var filterItem = options.Filter[i];
                    filterItem.NavigationMetaPath = BuildNavigationMetaPath(MetaType, filterItem.PropertyName);
                    filterItem.MetaProperty = filterItem.NavigationMetaPath.Last();

                    if ( filterItem.MetaProperty.IsCalculated )
                    {
                        options.Filter.RemoveAt(i);
                        options.InMemoryFilter.Add(filterItem);
                    }
                    else
                    {
                        var predicateExpression = filterItem.MakePredicateExpression<TEntity>(inMemory: false);
                        dbQuery = dbQuery.Where(predicateExpression);
                        i++;
                    }
                }

                return dbQuery;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //-- Moved into QueryableExtensions.OfMetaType
            //private IQueryable<TEntity> ApplyOfTypeFilter(IQueryable<TEntity> dbQuery, string typeString)
            //{
            //    if ( MetaType.QualifiedName.EqualsIgnoreCase(typeString) )
            //    {
            //        return dbQuery;
            //    }

            //    var filterMetaType = MetaType.DerivedTypes.FirstOrDefault(t => t.QualifiedName.EqualsIgnoreCase(typeString));

            //    if ( filterMetaType == null )
            //    {
            //        throw new ArgumentException(
            //            string.Format("Specified entity type '{0}' is not compatible with entity '{1}'.", typeString, MetaType.QualifiedName));
            //    }

            //    return filterMetaType.MakeOfType(dbQuery);
            //}

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IQueryable<TEntity> HandlePaging(QueryOptions options, IQueryable<TEntity> dbQuery)
            {
                if ( options.InMemoryFilter.Count == 0 && options.InMemoryOrderBy.Count == 0 )
                {
                    if ( options.Skip.HasValue )
                    {
                        dbQuery = dbQuery.Skip(options.Skip.Value);
                    }

                    if ( options.Take.HasValue )
                    {
                        dbQuery = dbQuery.Take(options.Take.Value);
                    }
                }

                return dbQuery;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private ChartData TryGetVisualizationFrom(IQueryable query)
            {
                var visualized = query as IVisualizedQueryable;
                
                if (visualized != null)
                {
                    return visualized.Visualization;
                }

                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AdHocEntityHandler : EntityHandler
        {
            public AdHocEntityHandler(ApplicationEntityService owner, ITypeMetadata metaType)
                : base(owner, metaType, domainContextType: null, extensions: new IEntityHandlerExtension[0])
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of EntityHandler

            public override IDomainObject CreateNew()
            {
                return Framework.As<ICoreFramework>().NewDomainObject(MetaType.ContractType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEntityId ParseEntityId(string id)
            {
                throw NewNotSupportedException();
            }
            public override IUnitOfWork NewUnitOfWork(object txViewModel = null, bool debugPerformStaleCheck = false)
            {
                throw NewNotSupportedException();
            }
            public override QueryResults Query(QueryOptions options, IQueryable query = null, object txViewModel = null)
            {
                throw NewNotSupportedException();
            }
            public override EntityCursor QueryCursor(QueryOptions options, IQueryable query = null, object txViewModel = null)
            {
                throw NewNotSupportedException();
            }
            public override IDomainObject GetById(string id)
            {
                throw NewNotSupportedException();
            }
            public override IDomainObject[] GetByIdList(object[] idList)
            {
                throw NewNotSupportedException();
            }
            public override IDomainObject[] GetByForeignKeyList(IPropertyMetadata inverseForeignKeyProperty, object[] leftSideIds)
            {
                throw NewNotSupportedException();
            }
            public override bool TryGetById(string id, out IDomainObject entity)
            {
                throw NewNotSupportedException();
            }
            public override void Insert(IDomainObject entity)
            {
                throw NewNotSupportedException();
            }
            public override void Update(IDomainObject entity)
            {
                throw NewNotSupportedException();
            }
            public override void Delete(string id)
            {
                throw NewNotSupportedException();
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private NotSupportedException NewNotSupportedException()
            {
                return new NotSupportedException("AdHocEntityHandler does not support requested operation.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class OutputEntityPropertyAccessControl : IEntityMemberAccessControl
        {
            private readonly ITypeMetadata _metaType;
            private readonly HashSet<IPropertyMetadata> _propertiesAllowedToChange;
            private readonly HashSet<MethodInfo> _methodsAllowedToInvoke;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public OutputEntityPropertyAccessControl(ITypeMetadata metaType)
            {
                _metaType = metaType;
                _propertiesAllowedToChange = new HashSet<IPropertyMetadata>();
                _methodsAllowedToInvoke = new HashSet<MethodInfo>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IEntityMemberAccessControl.AllowChangeAllProperties()
            {
                _propertiesAllowedToChange.UnionWith(_metaType.Properties.Where(p => !p.IsReadOnly));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IEntityMemberAccessControl.AllowChangeProperties(params Expression<Func<object>>[] properties)
            {
                foreach (var propertyExpression in properties)
                {
                    var declaration = propertyExpression.GetPropertyInfo();
                    var metaProperty = _metaType.GetPropertyByName(declaration.Name);
                    _propertiesAllowedToChange.Add(metaProperty);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            void IEntityMemberAccessControl.AllowInvokeAllMethods()
            {
                //TODO: add information on entity methods to ITypeMetadata + TypeMetadataBuilder
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IEntityMemberAccessControl.AllowInvokeMethods(params Expression<Action>[] methods)
            {
                foreach (var methodExpression in methods)
                {
                    var declaration = methodExpression.GetMethodInfo();
                    _methodsAllowedToInvoke.Add(declaration);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<string> GetPropertiesAllowedToChange()
            {
                return _propertiesAllowedToChange.Select(p => p.Name).ToList();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<string> GetMethodsAllowedToInvoke()
            {
                return _methodsAllowedToInvoke.Select(m => m.Name).ToList();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class QueryResultEnumerable<TEntity> : IEnumerable<TEntity>
        {
            private readonly IEnumerable<TEntity> _dbCursor;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryResultEnumerable(IEnumerable<TEntity> dbCursor)
            {
                _dbCursor = dbCursor;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEnumerable

            public IEnumerator<TEntity> GetEnumerator()
            {
                return _dbCursor.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class DomainObjectContractResolver : DefaultContractResolver
        {
            private readonly ITypeMetadataCache _metadataCache;
            private readonly ApplicationEntityService _ownerService;
            private readonly QueryOptions _queryOptions;
            private readonly CamelCasePropertyNamesContractResolver _camelCaseContractResolver;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public DomainObjectContractResolver(ITypeMetadataCache metadataCache, ApplicationEntityService ownerService, QueryOptions queryOptions)
            {
                _metadataCache = metadataCache;
                _ownerService = ownerService;
                _queryOptions = queryOptions;
                _camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of DefaultContractResolver

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var typeName = type.Name;
                var properties = base.CreateProperties(type, memberSerialization);
                var contractTypes = GetContractTypes(type);

                if ( contractTypes.Length > 0 )
                {
                    var metaTypes = contractTypes.Select(t => _metadataCache.GetTypeMetadata(t)).ToArray();

                    properties = ReplaceRelationPropertiesWithForeignKeys(metaTypes, properties);
                    ConfigureEmbeddedCollectionProperties(metaTypes, properties);
                    IncludeNavigationProperties(metaTypes, properties);
                    //IncludeForeignKeyDisplayNameProperties(metaTypes, properties);

                    AttachSerializationFilter(properties);

                    properties.Insert(0, CreateObjectTypeProperty());
                    properties.Insert(1, CreateEntityIdProperty());
                }

                return properties;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AttachSerializationFilter(IList<JsonProperty> properties)
            {
                foreach (var property in properties)
                {
                    var localProperty = property;
                    property.ShouldSerialize = (obj) => {
                        var query = QueryContext.Current;
                        
                        if (query != null)
                        {
                            return query.Options.IsPropertyIncludedInSelectList(localProperty.PropertyName);
                        }
                        
                        return true;
                    };
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ConfigureEmbeddedCollectionProperties(ITypeMetadata[] metaTypes, IList<JsonProperty> jsonProperties)
            {
                foreach ( var jsonProperty in jsonProperties )
                {
                    var metaProperty = TryGetPropertyByName(metaTypes, jsonProperty.PropertyName);

                    if (metaProperty == null)
                    {
                        continue;
                    }

                    if (IsEmbeddedObjectCollectionProperty(metaProperty) && metaProperty.Relation != null)
                    {
                        var converterClosedType =
                            typeof(EmbeddedDomainObjectCollectionConverter<>).MakeGenericType(metaProperty.Relation.RelatedPartyType.ContractType);
                        var converterInstance = (JsonConverter)Activator.CreateInstance(converterClosedType, _ownerService, metaProperty);
                        jsonProperty.MemberConverter = converterInstance;

                        //jsonProperty.ValueProvider = new EmbeddedCollectionValueProvider(innerProvider: jsonProperty.ValueProvider);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IList<JsonProperty> ReplaceRelationPropertiesWithForeignKeys(ITypeMetadata[] metaTypes, IList<JsonProperty> properties)
            {
                var resultList = new List<JsonProperty>();

                foreach ( var originalJsonProperty in properties )
                {
                    var metaProperty = TryGetPropertyByName(metaTypes, originalJsonProperty.PropertyName);

                    if (metaProperty == null)
                    {
                        resultList.Add(originalJsonProperty);
                        continue;
                    }

                    if ( ShouldExcludeProperty(metaProperty) )
                    {
                        continue;
                    }

                    if ( ShouldReplacePropertyWithForeignKey(metaProperty) )
                    {
                        resultList.Add(CreateReplacementForeignKeyProperty(metaProperty, originalJsonProperty));
                    }
                    else if ( ShouldReplacePropertyWithForeignKeyCollection(metaProperty) )
                    {
                        resultList.Add(CreateReplacementForeignKeyCollectionProperty(metaProperty, originalJsonProperty));
                    }
                    else
                    {
                        resultList.Add(originalJsonProperty);
                    }
                }

                return resultList;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void IncludeNavigationProperties(ITypeMetadata[] metaTypes, IList<JsonProperty> jsonProperties)
            {
                if ( _queryOptions != null )
                {
                    foreach ( var selectItem in _queryOptions.SelectPropertyNames.Where(s => s.NeedsJoinOperation || s.NeedsForeignKeyNavigation) )
                    {
                        IPropertyMetadata metaProperty;
                        var metaType = metaTypes.FirstOrDefault(t => t.TryGetPropertyByName(selectItem.PropertyPath.First(), out metaProperty));

                        if ( metaType != null )
                        {
                            metaProperty = metaType.GetPropertyByName(selectItem.PropertyPath.First());

                            if ( metaProperty.Relation != null )
                            {
                                if ( metaProperty.IsCollection && metaProperty.ClrType.IsCollectionTypeOfItem(metaProperty.Relation.RelatedPartyType.ContractType) )
                                {
                                    var reduceProperty = CreateReduceCollectionProperty(selectItem, metaProperty);
                                    jsonProperties.Add(reduceProperty);
                                }
                                else if ( !metaProperty.IsCollection )
                                {
                                    var navigationProperty = CreateNavigationProperty(selectItem, metaProperty);
                                    jsonProperties.Add(navigationProperty);
                                }
                            }
                        }
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void IncludeForeignKeyDisplayNameProperties(ITypeMetadata[] metaTypes, IList<JsonProperty> jsonPproperties)
            {
                if (_queryOptions != null)
                {
                    foreach (var selectItem in _queryOptions.SelectPropertyNames.Where(s => s.NeedsForeignKeyDisplayName))
                    {
                        IPropertyMetadata metaProperty = null;
                        var metaType = metaTypes.FirstOrDefault(t => t.TryGetPropertyByName(selectItem.PropertyPath.First(), out metaProperty));

                        if (metaType != null && metaProperty != null)
                        {
                            EnsureForeignKeyDisplayNameProperty(selectItem, metaProperty, jsonPproperties);
                        }
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void EnsureForeignKeyDisplayNameProperty(QuerySelectItem selectItem, IPropertyMetadata metaProperty, IList<JsonProperty> jsonProperties)
            {
                var displayNamePropertyExists = _queryOptions.SelectPropertyNames.Any(s => s.IsForeignKeyDisplayNameFor(metaProperty));

                if (!displayNamePropertyExists)
                {
                    var displayNameProperty = CreateNavigationDisplayNameProperty(selectItem, metaProperty);
                    jsonProperties.Add(displayNameProperty);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private JsonProperty CreateReplacementForeignKeyProperty(IPropertyMetadata metaProperty, JsonProperty originalJsonProperty)
            {
                var replacingJsonProperty = new JsonProperty {
                    PropertyType = typeof(string),
                    DeclaringType = metaProperty.ContractPropertyInfo.DeclaringType,
                    PropertyName = metaProperty.Name,
                    ValueProvider = new ForeignKeyValueProvider(_ownerService, metaProperty, originalJsonProperty),
                    Readable = true,
                    Writable = true
                };

                return replacingJsonProperty;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private JsonProperty CreateNavigationProperty(QuerySelectItem selectItem, IPropertyMetadata firstStepMetaProperty)
            {
                var navigationJsonProperty = new JsonProperty {
                    PropertyType = firstStepMetaProperty.Relation.RelatedPartyType.ContractType,
                    DeclaringType = firstStepMetaProperty.ContractPropertyInfo.DeclaringType,
                    PropertyName = selectItem.AliasName,
                    Readable = true,
                    Writable = false,
                };

                navigationJsonProperty.ValueProvider = new NavigationValueProvider(selectItem, firstStepMetaProperty);
                return navigationJsonProperty;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private JsonProperty CreateNavigationDisplayNameProperty(QuerySelectItem selectItem, IPropertyMetadata metaProperty)
            {
                var displayNameSelectItem = new QuerySelectItem(metaProperty.Name + "." + metaProperty.Relation.RelatedPartyType.DisplayNameProperty.Name);
                displayNameSelectItem.BuildMetaPropertyPath(metaProperty.DeclaringContract);
                return CreateNavigationProperty(displayNameSelectItem, metaProperty);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private JsonProperty CreateReduceCollectionProperty(QuerySelectItem selectItem, IPropertyMetadata firstStepMetaProperty)
            {
                var reduceJsonProperty = new JsonProperty {
                    PropertyType = typeof(string),
                    DeclaringType = firstStepMetaProperty.ContractPropertyInfo.DeclaringType,
                    PropertyName = selectItem.AliasName,
                    Readable = true,
                    Writable = false,
                };

                reduceJsonProperty.ValueProvider = new ReduceCollectionValueProvider(selectItem, firstStepMetaProperty);
                return reduceJsonProperty;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private JsonProperty CreateReplacementForeignKeyCollectionProperty(IPropertyMetadata metaProperty, JsonProperty originalJsonProperty)
            {
                var valueProviderClosedType = typeof(ForeignKeyCollectionValueProvider<>).MakeGenericType(metaProperty.Relation.RelatedPartyType.ContractType);

                var valueProviderInstance = (IValueProvider)Activator.CreateInstance(valueProviderClosedType, _ownerService, metaProperty, originalJsonProperty);

                var replacingJsonProperty = new JsonProperty {
                    PropertyType = typeof(string[]),
                    DeclaringType = metaProperty.ContractPropertyInfo.DeclaringType,
                    PropertyName = metaProperty.Name,
                    ValueProvider = valueProviderInstance,
                    Readable = true,
                    Writable = true
                };

                return replacingJsonProperty;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private JsonProperty CreateObjectTypeProperty()
            {
                return new JsonProperty() {
                    PropertyName = "$type",
                    Readable = true,
                    Writable = false,
                    PropertyType = typeof(string),
                    ValueProvider = new DomainObjectTypeValueProvider(_metadataCache),
                    NullValueHandling = NullValueHandling.Ignore
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private JsonProperty CreateEntityIdProperty()
            {
                return new JsonProperty() {
                    PropertyName = QueryOptions.IdPropertyName,
                    Readable = true,
                    Writable = false,
                    PropertyType = typeof(string),
                    ValueProvider = new EntityIdValueProvider(_metadataCache),
                    NullValueHandling = NullValueHandling.Ignore,
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IPropertyMetadata TryGetPropertyByName(ITypeMetadata[] metaTypes, string propertyName)
            {
                foreach ( var metaType in metaTypes )
                {
                    IPropertyMetadata metaProperty;

                    if ( metaType.TryGetPropertyByName(propertyName, out metaProperty) )
                    {
                        return metaProperty;
                    }
                }

                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool ShouldExcludeProperty(IPropertyMetadata metaProperty)
            {
                return (metaProperty.RelationalMapping != null && metaProperty.RelationalMapping.StorageStyle == PropertyStorageStyle.InverseForeignKey);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool ShouldReplacePropertyWithForeignKey(IPropertyMetadata metaProperty)
            {
                if ( metaProperty.Kind != PropertyKind.Relation || !metaProperty.Relation.RelatedPartyType.IsEntity )
                {
                    return false;
                }

                if ( metaProperty.DeclaringContract.IsViewModel && metaProperty.Relation.Multiplicity != RelationMultiplicity.ManyToMany )
                {
                    return true;
                }

                return (metaProperty.RelationalMapping != null && metaProperty.RelationalMapping.StorageStyle == PropertyStorageStyle.InlineForeignKey);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool ShouldReplacePropertyWithForeignKeyCollection(IPropertyMetadata metaProperty)
            {
                if ( metaProperty.Kind != PropertyKind.Relation || !metaProperty.Relation.RelatedPartyType.IsEntity )
                {
                    return false;
                }

                if ( metaProperty.DeclaringContract.IsViewModel && metaProperty.Relation.Multiplicity == RelationMultiplicity.ManyToMany )
                {
                    return true;
                }

                return (metaProperty.RelationalMapping != null &&
                    metaProperty.RelationalMapping.StorageStyle == PropertyStorageStyle.EmbeddedForeignKeyCollection);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsEmbeddedObjectCollectionProperty(IPropertyMetadata metaProperty)
            {
                if ( !metaProperty.IsCollection )
                {
                    return false;
                }

                if ( metaProperty.Relation != null && metaProperty.Relation.RelatedPartyType.IsEntityPart )
                {
                    return true;
                }

                if (metaProperty.IsCalculated)
                {
                    return true;
                }

                return (metaProperty.RelationalMapping != null && metaProperty.RelationalMapping.IsEmbeddedInParent);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DomainObjectTypeValueProvider : IValueProvider
        {
            private readonly ITypeMetadataCache _metadataCache;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DomainObjectTypeValueProvider(ITypeMetadataCache metadataCache)
            {
                _metadataCache = metadataCache;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetValue(object target)
            {
                var domainObject = target as IObject;

                if ( domainObject != null )
                {
                    var metaType = _metadataCache.GetTypeMetadata(domainObject.ContractType);
                    return metaType.QualifiedName;
                }
                else
                {
                    return null;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetValue(object target, object value)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityIdValueProvider : IValueProvider
        {
            private readonly ITypeMetadataCache _metadataCache;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityIdValueProvider(ITypeMetadataCache metadataCache)
            {
                _metadataCache = metadataCache;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetValue(object target)
            {
                var domainObject = target as IDomainObject;

                if ( domainObject != null )
                {
                    var metaType = _metadataCache.GetTypeMetadata(domainObject.ContractType);

                    if ( metaType.IsEntity )
                    {
                        return EntityId.Of(domainObject).Value.ToStringOrDefault();
                    }
                }

                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetValue(object target, object value)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ForeignKeyValueProvider : IValueProvider
        {
            private readonly ApplicationEntityService _ownerService;
            private readonly JsonProperty _relationProperty;
            private readonly ITypeMetadata _relatedMetaType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ForeignKeyValueProvider(ApplicationEntityService ownerService, IPropertyMetadata metaProperty, JsonProperty relationProperty)
            {
                _ownerService = ownerService;
                _relationProperty = relationProperty;
                _relatedMetaType = metaProperty.Relation.RelatedPartyType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetValue(object target, object value)
            {
                object relatedEntityObject;

                if ( value != null )
                {
                    var handler = _ownerService._handlerByEntityName[_relatedMetaType.QualifiedName];

                    //TODO: remove this workaround - handle entities belonging to multiple contexts
                    try
                    {
                        relatedEntityObject = handler.GetById(value.ToString());
                    }
                    catch
                    {
                        return;
                    }
                }
                else
                {
                    relatedEntityObject = null;
                }

                if ( relatedEntityObject != null )
                {
                    _relationProperty.ValueProvider.SetValue(target, relatedEntityObject);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetValue(object target)
            {
                var relatedEntity = _relationProperty.ValueProvider.GetValue(target);

                if ( relatedEntity != null )
                {
                    var relatedEntityId = EntityId.ValueOf(relatedEntity);
                    return relatedEntityId.ToStringOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ForeignKeyCollectionValueProvider<TEntity> : IValueProvider
            where TEntity : class
        {
            private readonly ApplicationEntityService _ownerService;
            private readonly JsonProperty _relationProperty;
            private readonly ITypeMetadata _relatedMetaType;
            private readonly EntityHandler _relatedEntityHandler;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ForeignKeyCollectionValueProvider(ApplicationEntityService ownerService, IPropertyMetadata metaProperty, JsonProperty relationProperty)
            {
                _ownerService = ownerService;
                _relationProperty = relationProperty;
                _relatedMetaType = metaProperty.Relation.RelatedPartyType;
                _relatedEntityHandler = _ownerService._handlerByEntityName[_relatedMetaType.QualifiedName];
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetValue(object target, object value)
            {
                var collection = (ICollection<TEntity>)_relationProperty.ValueProvider.GetValue(target);
                var entityIdSet = new HashSet<string>((string[])value);
                var itemsToRemove = new List<TEntity>();

                foreach ( var item in collection )
                {
                    var itemEntityId = item.As<IPersistableObject>().As<IEntityObject>().GetId().Value.ToString();

                    if ( entityIdSet.Contains(itemEntityId) )
                    {
                        entityIdSet.Remove(itemEntityId);
                    }
                    else
                    {
                        itemsToRemove.Add(item);
                    }
                }

                foreach ( var item in itemsToRemove )
                {
                    collection.Remove(item);
                }

                foreach ( var id in entityIdSet )
                {
                    collection.Add((TEntity)_relatedEntityHandler.GetById(id));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetValue(object target)
            {
                var collection = (ICollection<TEntity>)_relationProperty.ValueProvider.GetValue(target);

                if ( collection == null )
                {
                    return null;
                }

                var entityIds = collection.Select(obj => obj.As<IPersistableObject>().As<IEntityObject>().GetId().Value.ToStringOrDefault()).ToArray();

                return entityIds;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EmbeddedCollectionValueProvider : IValueProvider
        {
            private readonly IValueProvider _innerProvider;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EmbeddedCollectionValueProvider(IValueProvider innerProvider)
            {
                _innerProvider = innerProvider;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetValue(object target)
            {
                var value = _innerProvider.GetValue(target);
                return value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetValue(object target, object value)
            {
                _innerProvider.SetValue(target, value);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NavigationValueProvider : IValueProvider
        {
            private readonly QuerySelectItem _selectItem;
            private readonly IPropertyMetadata _firstStepMetaProperty;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NavigationValueProvider(QuerySelectItem selectItem, IPropertyMetadata firstStepMetaProperty)
            {
                _selectItem = selectItem;
                _firstStepMetaProperty = firstStepMetaProperty;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetValue(object target)
            {
                var metaType = _firstStepMetaProperty.DeclaringContract;
                var value = _selectItem.ReadValue(QueryContext.Current, metaType, target);
                return value;

                //ITypeMetadata currentMetaType = _firstStepMetaProperty.DeclaringContract;
                //object currentTarget = target;

                //for ( int stepIndex = 0 ; stepIndex < _selectItem.PropertyPath.Count ; stepIndex++ )
                //{
                //    var nextStepProperty = currentMetaType.GetPropertyByName(_selectItem.PropertyPath[stepIndex]);

                //    if ( nextStepProperty.Relation == null )
                //    {
                //        return nextStepProperty.ReadValue(currentTarget);
                //    }

                //    var leftJoin = QueryContext.Current.GetLeftJoinForNavigation(nextStepProperty);
                //    var leftSideIdProperty = nextStepProperty.DeclaringContract.EntityIdProperty;

                //    if ( !leftJoin.TryGetRightSideValue(leftSideIdProperty.ReadValue(currentTarget), out currentTarget) || currentTarget == null )
                //    {
                //        return null;
                //    }

                //    currentMetaType = nextStepProperty.Relation.RelatedPartyType;
                //}

                //return currentTarget;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetValue(object target, object value)
            {
                throw new NotSupportedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ReduceCollectionValueProvider : IValueProvider
        {
            private readonly QuerySelectItem _selectItem;
            private readonly IPropertyMetadata _firstStepMetaProperty;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ReduceCollectionValueProvider(QuerySelectItem selectItem, IPropertyMetadata firstStepMetaProperty)
            {
                _selectItem = selectItem;
                _firstStepMetaProperty = firstStepMetaProperty;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetValue(object target)
            {
                var metaType = _firstStepMetaProperty.DeclaringContract;
                var itemMetaType = _firstStepMetaProperty.Relation.RelatedPartyType;
                var collection = _selectItem.ReadValue(QueryContext.Current, metaType, target, makeSteps: 1) as IEnumerable;

                if ( collection != null )
                {
                    return string.Join(
                        ", ", 
                        collection.Cast<IDomainObject>().Select(item => 
                            _selectItem.ReadValue(QueryContext.Current, itemMetaType, item, skipSteps: 1)
                        )
                    );
                }

                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetValue(object target, object value)
            {
                throw new NotSupportedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TimeSeriesPointListConverter : JsonConverter
        {
            #region Overrides of JsonConverter

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                JArray array = new JArray();
                var points = (List<ChartData.TimeSeriesPoint>)value;

                for (int i = 0 ; i < points.Count ; i++)
                {
                    array.Add(new JValue(points[i].UnixUtcTimestamp));
                    array.Add(new JValue(points[i].Value));
                }

                array.WriteTo(writer);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(List<ChartData.TimeSeriesPoint>));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanRead
            {
                get { return false; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanWrite
            {
                get { return true; }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DomainObjectConverter : JsonConverter
        {
            private readonly ApplicationEntityService _ownerService;
            private readonly QueryOptions _queryOptions;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DomainObjectConverter(ApplicationEntityService ownerService, QueryOptions queryOptions)
            {
                _ownerService = ownerService;
                _queryOptions = queryOptions;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of JsonConverter

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

                object relatedDomainObject;

                if (LoadRelatedDomainObjectByForeignKey(reader, objectType, out relatedDomainObject))
                {
                    return relatedDomainObject;
                }

                JObject jo = JObject.Load(reader);

                var typeName = jo["$type"].Value<string>();
                var handler = _ownerService._handlerByEntityName.GetValueOrCreateDefault(typeName, _ownerService.CreateAdHocEntityHandler);
                IObject target;

                if (existingValue != null && handler.MetaType.ContractType.IsInstanceOfType(existingValue))
                {
                    target = (IObject)existingValue;
                }
                else
                {
                    target = handler.CreateNew();
                }

                JsonReader jObjectReader = jo.CreateReader();
                jObjectReader.Culture = reader.Culture;
                jObjectReader.DateParseHandling = reader.DateParseHandling;
                jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                jObjectReader.FloatParseHandling = reader.FloatParseHandling;

                serializer.Populate(jObjectReader, target);
                return target;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanConvert(Type objectType)
            {
                if (objectType.IsInterface)
                {
                    return (objectType.IsEntityContract() || objectType.IsEntityPartContract());
                }

                return objectType.GetInterfaces().Any(intf => intf.IsEntityContract() || intf.IsEntityPartContract());

                //var objectTypeFullname = objectType.FullName;
                //var result = (objectType.IsEntityContract() || objectType.IsEntityPartContract());
                //return (objectType.IsEntityContract() || objectType.IsEntityPartContract());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanRead
            {
                get { return true; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanWrite
            {
                get { return false; }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool LoadRelatedDomainObjectByForeignKey(JsonReader reader, Type objectType, out object relatedDomainObject)
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    // JSON contains object contents - proceed and populate it
                    relatedDomainObject = null;
                    return false;
                }

                var idValue = reader.Value;
                var relatedMetaType = _ownerService._metadataCache.GetTypeMetadata(objectType);
                var foreignKeyHandler = _ownerService._handlerByEntityName[relatedMetaType.QualifiedName];

                relatedDomainObject = foreignKeyHandler.GetById(idValue.ToString());
                return true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EmbeddedDomainObjectCollectionConverter<TObject> : JsonConverter
            where TObject : class
        {
            private readonly ApplicationEntityService _ownerService;
            private readonly IPropertyMetadata _metaProperty;
            private readonly ITypeMetadata _relatedMetaType;
            private readonly EntityHandler _relatedEntityHandler;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EmbeddedDomainObjectCollectionConverter(ApplicationEntityService ownerService, IPropertyMetadata metaProperty)
            {
                _ownerService = ownerService;
                _metaProperty = metaProperty;
                _relatedMetaType = _metaProperty.Relation.RelatedPartyType;
                _relatedEntityHandler = _ownerService._handlerByEntityName[_relatedMetaType.QualifiedName];
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of JsonConverter

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if ( reader.TokenType != JsonToken.StartArray )
                {
                    throw new FormatException("Expected StartArray in input JSON, but was: " + reader.TokenType);
                }

                var existingCollection = (ICollection<TObject>)existingValue;
                var removedItems = new HashSet<TObject>(existingCollection);

                while ( reader.Read() && reader.TokenType != JsonToken.EndArray )
                {
                    JObject jo = JObject.Load(reader);

                    TObject itemToPopulate = null;

                    if ( _relatedMetaType.IsEntity )
                    {
                        var idString = jo[QueryOptions.IdPropertyName].Value<string>();
                        itemToPopulate =
                            existingCollection.FirstOrDefault(obj => obj.As<IPersistableObject>().As<IEntityObject>().GetId().Value.ToString() == idString);
                    }
                    else
                    {
                        var existingItemIndexToken = jo["$index"];

                        if (existingItemIndexToken != null)
                        {
                            var itemIndex = Int32.Parse(existingItemIndexToken.Value<string>());
                            itemToPopulate = existingCollection.Skip(itemIndex).Take(1).FirstOrDefault();
                        }
                    }

                    if (itemToPopulate != null)
                    {
                        removedItems.ExceptWith(new[] { itemToPopulate });
                    }
                    else
                    {
                        itemToPopulate = (TObject)_relatedEntityHandler.CreateNew();
                        existingCollection.Add(itemToPopulate);
                    }

                    JsonReader jObjectReader = jo.CreateReader();
                    jObjectReader.Culture = reader.Culture;
                    jObjectReader.DateParseHandling = reader.DateParseHandling;
                    jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                    jObjectReader.FloatParseHandling = reader.FloatParseHandling;
                    serializer.Populate(jObjectReader, itemToPopulate);
                }

                foreach (var itemToRemove in removedItems)
                {
                    existingCollection.Remove(itemToRemove);
                }

                return existingValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanConvert(Type objectType)
            {
                Type itemType;

                return (objectType.IsCollectionType(out itemType) && (itemType.IsEntityContract() || itemType.IsEntityPartContract()));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanRead
            {
                get { return true; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanWrite
            {
                get { return false; }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool LoadRelatedDomainObjectByForeignKey(JsonReader reader, Type objectType, out object relatedDomainObject)
            {
                if ( reader.TokenType == JsonToken.StartObject )
                {
                    // JSON contains object contents - proceed and populate it
                    relatedDomainObject = null;
                    return false;
                }

                var idValue = reader.Value;
                var relatedMetaType = _ownerService._metadataCache.GetTypeMetadata(objectType);
                var foreignKeyHandler = _ownerService._handlerByEntityName[relatedMetaType.QualifiedName];

                relatedDomainObject = foreignKeyHandler.GetById(idValue.ToString());
                return true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ViewModelObjectConverter : JsonConverter
        {
            private readonly ApplicationEntityService _ownerService;
            private readonly IViewModelObjectFactory _viewModelFactory;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ViewModelObjectConverter(ApplicationEntityService ownerService, IViewModelObjectFactory viewModelFactory)
            {
                _ownerService = ownerService;
                _viewModelFactory = viewModelFactory;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of JsonConverter

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if ( reader.TokenType == JsonToken.Null )
                {
                    return null;
                }

                var target = _viewModelFactory.NewEntity(objectType);

                JObject jo = JObject.Load(reader);
                JsonReader jObjectReader = jo.CreateReader();
                jObjectReader.Culture = reader.Culture;
                jObjectReader.DateParseHandling = reader.DateParseHandling;
                jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                jObjectReader.FloatParseHandling = reader.FloatParseHandling;

                var nestedSerializer = JsonSerializer.Create(_ownerService._defaultSerializerSettings);
                nestedSerializer.Populate(jObjectReader, target);

                return target;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanConvert(Type objectType)
            {
                return objectType.HasAttribute<ViewModelContractAttribute>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanRead
            {
                get { return true; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanWrite
            {
                get { return false; }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class FormattedDocumentConverter : JsonConverter
        {
            #region Overrides of JsonConverter

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var document = (FormattedDocument)value;
                string dataString;

                if (document != null)
                {
                    dataString = string.Format("data:{0};base64,{1}", document.Metadata.Format.ContentType, Convert.ToBase64String(document.Contents));
                }
                else
                {
                    dataString = null;
                }
                
                var token = JToken.FromObject(dataString);
                token.WriteTo(writer);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(FormattedDocument);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanRead
            {
                get { return true; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanWrite
            {
                get { return true; }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MetaTypeQualifiedNameConverter : JsonConverter
        {
            private readonly ITypeMetadataCache _metadataCache;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MetaTypeQualifiedNameConverter(ITypeMetadataCache metadataCache)
            {
                _metadataCache = metadataCache;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of JsonConverter

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                var metaType = _metadataCache.GetTypeMetadata((Type)value);
                writer.WriteValue(metaType.QualifiedName);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.Value == null)
                {
                    return null;
                }

                var qualifiedName = reader.Value.ToString();
                var metaType = _metadataCache.GetTypeMetadata(qualifiedName);
 
                return metaType.ContractType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanConvert(Type objectType)
            {
                return typeof(System.Type).IsAssignableFrom(objectType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanRead
            {
                get { return true; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanWrite
            {
                get { return true; }
            }

            #endregion
        }
    }
}
