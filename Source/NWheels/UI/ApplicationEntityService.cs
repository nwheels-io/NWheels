using System;
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
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NWheels.Entities.Factories;

namespace NWheels.UI
{
    public class ApplicationEntityService
    {
        private readonly IFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IDomainContextLogger _domainContextLogger;
        private readonly Dictionary<string, EntityHandler> _handlerByEntityName;
        private readonly JsonSerializerSettings _serializerSettings;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationEntityService(
            IFramework framework, 
            ITypeMetadataCache metadataCache, 
            IDomainContextLogger domainContextLogger, 
            IEnumerable<Type> domainContextTypes)
        {
            _framework = framework;
            _metadataCache = metadataCache;
            _domainContextLogger = domainContextLogger;
            _handlerByEntityName = new Dictionary<string, EntityHandler>(StringComparer.InvariantCultureIgnoreCase);

            RegisterEntities(domainContextTypes);

            _serializerSettings = CreateSerializerSettings();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsEntityNameRegistered(string entityName)
        {
            return _handlerByEntityName.ContainsKey(entityName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string NewEntityJson(string entityName)
        {
            var handler = _handlerByEntityName[entityName];
            string json;

            using ( handler.NewUnitOfWork() )
            {
                var newEntity = handler.CreateNew();
                json = JsonConvert.SerializeObject(newEntity, _serializerSettings);
            }

            return json;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public QueryOptions ParseQueryOptions(IDictionary<string, string> parameters)
        {
            return new QueryOptions(parameters);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string QueryEntityJson(string entityName, QueryOptions options)
        {
            var handler = _handlerByEntityName[entityName];
            string json;

            using ( handler.NewUnitOfWork() )
            {
                IDomainObject[] resultSet;
                long resultCount;
                handler.Query(options, out resultSet, out resultCount);

                var results = new QueryResults() {
                    ResultSet = resultSet,
                    ResultCount = resultCount
                };

                json = JsonConvert.SerializeObject(results, _serializerSettings);
            }

            return json;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string StoreEntityJson(string entityName, EntityState entityState, string entityId, string json)
        {
            var handler = _handlerByEntityName[entityName];
            IDomainObject domainObject = null;

            using ( var context = handler.NewUnitOfWork() )
            {
                if ( entityState.IsNew() )
                {
                    domainObject = handler.CreateNew();
                    JsonConvert.PopulateObject(json, domainObject, _serializerSettings);
                    handler.Insert(domainObject);
                }
                else if ( entityState.IsModified() )
                {
                    domainObject = handler.GetById(entityId);
                    JsonConvert.PopulateObject(json, domainObject, _serializerSettings);
                    handler.Update(domainObject);
                }
                else if ( entityState.IsDeleted() )
                {
                    handler.Delete(entityId);
                    return null;
                }
                else
                {
                    throw new ArgumentException("Unexpected value of entity state: " + entityState);
                }

                context.CommitChanges();
            }

            var resultJson = (domainObject != null ? JsonConvert.SerializeObject(domainObject, _serializerSettings) : null);
            return resultJson;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DeleteEntity(string entityName, string entityId)
        {
            var handler = _handlerByEntityName[entityName];

            using ( var context = handler.NewUnitOfWork() )
            {
                handler.Delete(entityId);
                context.CommitChanges();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StoreEntityBatchJson(string json)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterEntities(IEnumerable<Type> domainContextTypes)
        {
            foreach ( var contextType in domainContextTypes )
            {
                using ( var coontext = _framework.As<ICoreFramework>().NewUnitOfWork(contextType) )
                {
                    RegisterEntitiesFromDomainContext(contextType, coontext);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterEntitiesFromDomainContext(Type contextType, IApplicationDataRepository context)
        {
            foreach ( var entityContract in context.GetEntityContractsInRepository().Where(t => t != null) )
            {
                var metaType = _metadataCache.GetTypeMetadata(entityContract);

                if ( !_handlerByEntityName.ContainsKey(metaType.QualifiedName) )
                {
                    var handler = EntityHandler.Create(this, metaType, contextType);
                    _handlerByEntityName[metaType.QualifiedName] = handler;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new JsonSerializerSettings() {
                ContractResolver = new DomainObjectContractResolver(_metadataCache, this),
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                MaxDepth = 10,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            settings.Converters.Add(new DomainObjectConverter(this));

            return settings;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Type[] GetContractTypes(Type type)
        {
            var contracts = new List<Type>();  

            if ( type.IsEntityContract() || type.IsEntityPartContract() )
            {
                contracts.Add(type);
            }

            contracts.AddRange(type.GetInterfaces().Where(intf => intf.IsEntityContract() || intf.IsEntityPartContract()));

            return contracts.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryResults
        {
            public IDomainObject[] ResultSet { get; set; }
            public long ResultCount { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryOptions
        {
            public const string OrderByParameterKey = "$orderby";
            public const string MaxCountParameterKey = "$top";
            public const string CountOnlyParameterKey = "$count";
            public const string PlusOneParameterKey = "$plus1";
            public const string AscendingParameterModifier = ":asc";
            public const string DescendingParameterModifier = ":desc";

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryOptions()
            {
                EqualityFilter = new Dictionary<string, string>();
                OrderBy = new List<QueryOrderByItem>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryOptions(IDictionary<string, string> queryParams) 
                : this()
            {
                foreach ( var parameter in queryParams )
                {
                    if ( parameter.Key.EqualsIgnoreCase(OrderByParameterKey) )
                    {
                        AddOrderBy(parameter);
                    }
                    else if ( parameter.Key.EqualsIgnoreCase(MaxCountParameterKey) )
                    {
                        MaxCount = Int32.Parse(parameter.Value);
                    }
                    else if ( parameter.Key.EqualsIgnoreCase(CountOnlyParameterKey) )
                    {
                        IsCountOnly = true;
                    }
                    else if ( parameter.Key.EqualsIgnoreCase(PlusOneParameterKey) )
                    {
                        ReturnMaxCountPlusOne = true;
                    }
                    else
                    {
                        EqualityFilter[parameter.Key] = parameter.Value;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IDictionary<string, string> EqualityFilter { get; private set; }
            public IList<QueryOrderByItem> OrderBy { get; private set; }
            public int? MaxCount { get; private set; }
            public bool IsCountOnly { get; private set; }
            public bool ReturnMaxCountPlusOne { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void AddOrderBy(KeyValuePair<string, string> parameter)
            {
                var subParams = parameter.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach ( var subParam in subParams )
                {
                    OrderBy.Add(new QueryOrderByItem(subParam));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryOrderByItem
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

            public string PropertyName { get; private set; }
            public bool Ascending { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class EntityHandler
        {
            protected EntityHandler(ApplicationEntityService owner, ITypeMetadata metaType, Type domainContextType)
            {
                this.Owner = owner;
                this.MetaType = metaType;
                this.DomainContextType = domainContextType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract IUnitOfWork NewUnitOfWork();
            public abstract void Query(QueryOptions options, out IDomainObject[] resultSet, out long resultCount);
            public abstract IDomainObject GetById(string id);
            public abstract IDomainObject CreateNew();
            public abstract void Insert(IDomainObject entity);
            public abstract void Update(IDomainObject entity);
            public abstract void Delete(string id);

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

            public static EntityHandler Create(ApplicationEntityService owner, ITypeMetadata metaType, Type domainContextType)
            {
                var concreteClosedType = typeof(EntityHandler<,>).MakeGenericType(domainContextType, metaType.ContractType);
                return (EntityHandler)Activator.CreateInstance(concreteClosedType, owner, metaType, domainContextType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EntityHandler<TContext, TEntity> : EntityHandler
            where TContext : class, IApplicationDataRepository
        {
            public EntityHandler(ApplicationEntityService owner, ITypeMetadata metaType, Type domainContextType)
                 : base(owner, metaType, domainContextType)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IUnitOfWork NewUnitOfWork()
            {
                //TODO: remove this once we are sure the bug is solved
                PerContextResourceConsumerScope<TContext> stale;
                if ( (stale = new ThreadStaticAnchor<PerContextResourceConsumerScope<TContext>>().Current) != null )
                {
                    DomainContextLogger.StaleUnitOfWorkEncountered(stale.Resource.ToString(), ((DataRepositoryBase)(object)stale.Resource).InitializerThreadText);
                }

                return Framework.NewUnitOfWork<TContext>();
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Query(QueryOptions options, out IDomainObject[] resultSet, out long resultCount)
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var repository = context.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                    IQueryable<TEntity> query = repository;

                    query = HandleEqualityFilter(options, query);
                    query = HandleOrderBy(options, query);
                    query = HandleMaxCount(options, query);

                    TEntity[] dbResultSet;
                    ExecuteQuery(query, options, out dbResultSet, out resultCount);

                    resultSet = FilterCalculatedValues(options, dbResultSet).Cast<IDomainObject>().ToArray();
                    resultCount = resultSet.Length;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IEnumerable<TEntity> FilterCalculatedValues(QueryOptions options, TEntity[] resultSet)
            {
                IQueryable<TEntity> query = null;

                foreach ( var equalityFilterItem in options.EqualityFilter )
                {
                    var metaProperty = MetaType.GetPropertyByName(equalityFilterItem.Key);

                    if ( metaProperty.IsCalculated == false )
                    {
                        continue;
                    }

                    if ( query == null )
                    {
                        query = resultSet.AsQueryable<TEntity>();
                    }

                    object parsedValue = metaProperty.ParseStringValue(equalityFilterItem.Value);
                    query = query.Where(metaProperty.MakeEqualityComparison<TEntity>(parsedValue));
                }

                if ( query == null )
                {
                    return resultSet;
                }

                return query;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IDomainObject GetById(string id)
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var repository = context.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                    var idProperty = MetaType.PrimaryKey.Properties[0];
                    IQueryable<TEntity> query = repository.Where(idProperty.MakeEqualityComparison<TEntity>(valueString: id));
                    var result = query.FirstOrDefault();

                    if ( result == null )
                    {
                        throw new ArgumentException("Specfiied entity does not exist.");
                    }

                    return result as IDomainObject;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IDomainObject CreateNew()
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var repository = context.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                    var result = repository.New();

                    return result as IDomainObject;
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
            
            private IQueryable<TEntity> HandleMaxCount(QueryOptions options, IQueryable<TEntity> query)
            {
                if ( options.MaxCount.HasValue )
                {
                    query = query.Take(options.MaxCount.Value + (options.ReturnMaxCountPlusOne ? 1 : 0));
                }
                
                return query;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IQueryable<TEntity> HandleOrderBy(QueryOptions options, IQueryable<TEntity> query)
            {
                foreach ( var orderBy in options.OrderBy )
                {
                    var metaProperty = MetaType.GetPropertyByName(orderBy.PropertyName);
                    query = metaProperty.MakeOrderBy(query, orderBy.Ascending);
                }
                
                return query;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IQueryable<TEntity> HandleEqualityFilter(QueryOptions options, IQueryable<TEntity> query)
            {
                foreach ( var equalityFilterItem in options.EqualityFilter )
                {
                    var metaProperty = MetaType.GetPropertyByName(equalityFilterItem.Key);

                    if ( metaProperty.IsCalculated )
                    {
                        continue;
                    }

                    switch ( metaProperty.Kind )
                    {
                        case PropertyKind.Scalar:
                            query = query.Where(metaProperty.MakeEqualityComparison<TEntity>(valueString: equalityFilterItem.Value));
                            break;
                        case PropertyKind.Relation:
                            query = query.Where(metaProperty.MakeForeignKeyEqualityComparison<TEntity>(valueString: equalityFilterItem.Value));
                            break;
                        default:
                            throw new NotSupportedException("Cannot filter by property: " + metaProperty.Name);
                    }
                }

                return query;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class DomainObjectContractResolver : DefaultContractResolver
        {
            private readonly ITypeMetadataCache _metadataCache;
            private readonly ApplicationEntityService _ownerService;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public DomainObjectContractResolver(ITypeMetadataCache metadataCache, ApplicationEntityService ownerService)
            {
                _metadataCache = metadataCache;
                _ownerService = ownerService;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of DefaultContractResolver

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var properties = base.CreateProperties(type, memberSerialization);
                var contractTypes = GetContractTypes(type);

                if ( contractTypes.Length > 0 )
                {
                    properties = ReplaceRelationPropertiesWithForeignKeys(contractTypes, properties);
                }

                properties.Insert(0, CreateObjectTypeProperty());
                properties.Insert(1, CreateEntityIdProperty());
                
                return properties;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IList<JsonProperty> ReplaceRelationPropertiesWithForeignKeys(Type[] contractTypes, IList<JsonProperty> properties)
            {
                var resultList = new List<JsonProperty>();
                var metaTypes = contractTypes.Select(t => _metadataCache.GetTypeMetadata(t)).ToArray();

                foreach ( var originalJsonProperty in properties )
                {
                    var metaProperty = GetPropertyByName(metaTypes, originalJsonProperty.PropertyName);

                    if ( ShouldExcludeProperty(metaProperty) )
                    {
                        continue;
                    }

                    if ( !ShouldReplacePropertyWithForeignKey(metaProperty) )
                    {
                        resultList.Add(originalJsonProperty);
                        continue;
                    }
                    
                    var replacingJsonProperty = new JsonProperty {
                        PropertyType = typeof(string),
                        DeclaringType = metaProperty.ContractPropertyInfo.DeclaringType,
                        PropertyName = metaProperty.Name,
                        ValueProvider = new ForeignKeyValueProvider(_ownerService, metaProperty, originalJsonProperty),
                        Readable = true,
                        Writable = true
                    };

                    resultList.Add(replacingJsonProperty);
                }

                return resultList;
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
                    PropertyName = "$id",
                    Readable = true,
                    Writable = false,
                    PropertyType = typeof(string),
                    ValueProvider = new EntityIdValueProvider(_metadataCache),
                    NullValueHandling = NullValueHandling.Ignore
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IPropertyMetadata GetPropertyByName(ITypeMetadata[] metaTypes, string propertyName)
            {
                foreach ( var metaType in metaTypes )
                {
                    IPropertyMetadata metaProperty;

                    if ( metaType.TryGetPropertyByName(propertyName, out metaProperty) )
                    {
                        return metaProperty;
                    }
                }

                throw new ArgumentException("Property not found: " + propertyName);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool ShouldExcludeProperty(IPropertyMetadata metaProperty)
            {
                return (ShouldReplacePropertyWithForeignKey(metaProperty) && metaProperty.IsCollection);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool ShouldReplacePropertyWithForeignKey(IPropertyMetadata metaProperty)
            {
                return (
                    metaProperty.Kind == PropertyKind.Relation &&
                    metaProperty.Relation.RelatedPartyType.IsEntity &&
                    metaProperty.RelationalMapping != null && 
                    !metaProperty.RelationalMapping.EmbeddedInParent.GetValueOrDefault(false));
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
                var domainObject = target as IDomainObject;

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
                        return EntityId.Of(domainObject).Value.ToString();
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
                    relatedEntityObject = handler.GetById(value.ToString());
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

        public class DomainObjectConverter : JsonConverter
        {
            private readonly ApplicationEntityService _ownerService;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DomainObjectConverter(ApplicationEntityService ownerService)
            {
                _ownerService = ownerService;
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
                if ( reader.TokenType == JsonToken.Null )
                {
                    return null;
                }

                JObject jo = JObject.Load(reader);

                var typeName = jo["$type"].Value<string>();
                var handler = _ownerService._handlerByEntityName[typeName];
                var target = handler.CreateNew();

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
                return (objectType.IsEntityContract() || objectType.IsEntityPartContract());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanRead 
            {
                get
                {
                    return true;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanWrite
            {
                get
                {
                    return false;
                }
            }

            #endregion
        }
    }
}
