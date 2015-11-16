using System;
using System.Collections;
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
using Hapil;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NWheels.Entities.Factories;
using NWheels.TypeModel;
using NWheels.UI.Factories;

namespace NWheels.UI
{
    public class ApplicationEntityService
    {
        private readonly IFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IViewModelObjectFactory _viewModelFactory;
        private readonly IDomainContextLogger _domainContextLogger;
        private readonly Dictionary<string, EntityHandler> _handlerByEntityName;
        private readonly JsonSerializerSettings _serializerSettings;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationEntityService(
            IFramework framework, 
            ITypeMetadataCache metadataCache,
            IViewModelObjectFactory viewModelFactory,
            IDomainContextLogger domainContextLogger, 
            IEnumerable<Type> domainContextTypes)
        {
            _framework = framework;
            _metadataCache = metadataCache;
            _viewModelFactory = viewModelFactory;
            _domainContextLogger = domainContextLogger;
            _handlerByEntityName = new Dictionary<string, EntityHandler>(StringComparer.InvariantCultureIgnoreCase);

            RegisterDomainObjects(domainContextTypes);

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
                bool moreAvailable;
                handler.Query(options, out resultSet, out resultCount, out moreAvailable);

                var results = new QueryResults() {
                    ResultSet = resultSet,
                    ResultCount = resultCount,
                    MoreAvailable = moreAvailable
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
                    var traceWriter = new MemoryTraceWriter();
                    var populationSerializerSettings = CreateSerializerSettings();
                    populationSerializerSettings.TraceWriter = traceWriter;

                    domainObject = handler.GetById(entityId);
                    JsonConvert.PopulateObject(json, domainObject, populationSerializerSettings);//_serializerSettings);

                    Console.WriteLine(traceWriter.ToString());

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

        public JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new JsonSerializerSettings() {
                ContractResolver = new DomainObjectContractResolver(_metadataCache, this),
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                MaxDepth = 10,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new DomainObjectConverter(this));
            settings.Converters.Add(new ViewModelObjectConverter(this, _viewModelFactory));

            return settings;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterDomainObjects(IEnumerable<Type> domainContextTypes)
        {
            foreach ( var contextType in domainContextTypes )
            {
                using ( var coontextInstance = _framework.As<ICoreFramework>().NewUnitOfWork(contextType) )
                {
                    RegisterDomainObjectTypesFromContext(contextType, coontextInstance);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterDomainObjectTypesFromContext(Type contextType, IApplicationDataRepository contextInstance)
        {
            foreach ( var entityContract in contextInstance.GetEntityContractsInRepository().Where(t => t != null) )
            {
                var metaType = _metadataCache.GetTypeMetadata(entityContract);
                RegisterDomainObjectType(contextType, metaType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterDomainObjectType(Type contextType, ITypeMetadata metaType)
        {
            if ( _handlerByEntityName.ContainsKey(metaType.QualifiedName) )
            {
                return;
            }

            var handler = EntityHandler.Create(this, metaType, contextType);
            _handlerByEntityName[metaType.QualifiedName] = handler;

            foreach ( var property in metaType.Properties.Where(p => p.Kind.IsIn(PropertyKind.Part, PropertyKind.Relation)) )
            {
                RegisterDomainObjectType(contextType, property.Relation.RelatedPartyType);
            }

            foreach ( var derivedType in metaType.DerivedTypes )
            {
                RegisterDomainObjectType(contextType, derivedType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Type[] GetContractTypes(Type type)
        {
            var contracts = new List<Type>();  

            if ( type.IsEntityContract() || type.IsEntityPartContract() || type.IsViewModelContract() )
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
            public bool MoreAvailable { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryOptions
        {
            public const string TypeParameterKey = "$type";
            public const string SelectParameterKey = "$select";
            public const string IncludeParameterKey = "$include";
            public const string PropertyAliasModifier = ":as:";
            public const string CountParameterKey = "$count";
            public const string SkipParameterKey = "$skip";
            public const string TakeParameterKey = "$take";
            public const string OrderByParameterKey = "$orderby";
            public const string AscendingParameterModifier = ":asc";
            public const string DescendingParameterModifier = ":desc";
            public const string EqualOperator = ":eq";
            public const string NotEqualOperator = ":neq";
            public const string GreaterThanOperator = ":gt";
            public const string GreaterThanOrEqualOperator = ":gte";
            public const string LessThanOperator = ":lt";
            public const string LessThanOrEqualOperator = ":lte";
            public const string StringContainsOperator = ":strc";
            public const string StringStartsWithOperator = ":strsw";
            public const string StringEndsWithOperator = ":strew";

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryOptions()
            {
                Filter = new List<QueryFilterItem>();
                InMemoryFilter = new List<QueryFilterItem>();
                OrderBy = new List<QueryOrderByItem>();
                InMemoryOrderBy = new List<QueryOrderByItem>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryOptions(IDictionary<string, string> queryParams) 
                : this()
            {
                foreach ( var parameter in queryParams )
                {
                    if ( parameter.Key.EqualsIgnoreCase(SelectParameterKey) )
                    {
                        this.SelectPropertyNames = ParsePropertyList(parameter.Value);
                    }
                    else if ( parameter.Key.EqualsIgnoreCase(IncludeParameterKey) )
                    {
                        this.IncludePropertyNames = ParsePropertyList(parameter.Value);
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
                    else if ( parameter.Key.EqualsIgnoreCase(TypeParameterKey) )
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
            }


            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsCountOnly { get; private set; }
            public IList<QuerySelectItem> SelectPropertyNames { get; private set; }
            public IList<QuerySelectItem> IncludePropertyNames { get; private set; }
            public IList<QueryFilterItem> Filter { get; private set; }
            public IList<QueryFilterItem> InMemoryFilter { get; private set; }
            public IList<QueryOrderByItem> OrderBy { get; private set; }
            public IList<QueryOrderByItem> InMemoryOrderBy { get; private set; }
            public int? Skip { get; private set; }
            public int? Take { get; private set; }
            public string OfType { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool NeedInMemoryOperations
            {
                get
                {
                    return (InMemoryFilter.Count > 0 || InMemoryOrderBy.Count > 0);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private List<QuerySelectItem> ParsePropertyList(string parameterValue)
            {
                var destination = new List<QuerySelectItem>();
                var propertySpecifiers = parameterValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach ( var specifier in propertySpecifiers )
                {
                    destination.Add(new QuerySelectItem(specifier));
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QuerySelectItem
        {
            public QuerySelectItem(string propertySpecifier)
            {
                var aliasPosition = propertySpecifier.IndexOf(QueryOptions.PropertyAliasModifier, StringComparison.InvariantCultureIgnoreCase);
                var propertyPathString = (aliasPosition > 0 ? propertySpecifier.Substring(0, aliasPosition) : propertySpecifier);

                this.PropertyPath = propertyPathString.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                if ( aliasPosition > 0 )
                {
                    this.AliasName = propertySpecifier.Substring(aliasPosition + QueryOptions.PropertyAliasModifier.Length);
                }
                else
                {
                    this.AliasName = propertyPathString;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IReadOnlyList<string> PropertyPath { get; private set; }
            public string AliasName { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class QueryFilterItem
        {
            public QueryFilterItem(string propertyName, string @operator, string stringValue)
            {
                PropertyName = propertyName;
                Operator = @operator;
                StringValue = stringValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Expression<Func<TEntity, bool>> MakePredicateExpression<TEntity>()
            {
                if ( MetaProperty == null )
                {
                    throw new InvalidOperationException("MetaProperty must be set before calling this method.");
                }

                var expressionFactory = _s_binaryExpressionFactoryByOperator[Operator];

                if ( MetaProperty.Kind == PropertyKind.Scalar )
                {
                    return MetaProperty.MakeBinaryExpression<TEntity>(StringValue, expressionFactory);
                }
                else if ( MetaProperty.Kind == PropertyKind.Relation )
                {
                    return MetaProperty.MakeForeignKeyBinaryExpression<TEntity>(StringValue, expressionFactory);
                }

                throw new NotSupportedException("Cannot create filter expression for property of kind: " + MetaProperty.Kind);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string PropertyName { get; private set; }
            public string Operator { get; private set; }
            public string StringValue { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public IPropertyMetadata MetaProperty { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static readonly Dictionary<string, Func<Expression, Expression, Expression>> _s_binaryExpressionFactoryByOperator =
                new Dictionary<string, Func<Expression, Expression, Expression>>(StringComparer.InvariantCultureIgnoreCase) {
                    { QueryOptions.EqualOperator, Expression.Equal },
                    { QueryOptions.NotEqualOperator, Expression.NotEqual },
                    { QueryOptions.GreaterThanOperator, Expression.GreaterThan },
                    { QueryOptions.GreaterThanOrEqualOperator, Expression.GreaterThanOrEqual },
                    { QueryOptions.LessThanOperator, Expression.LessThan },
                    { QueryOptions.LessThanOrEqualOperator, Expression.LessThanOrEqual },
                    //{ QueryOptions.StringContainsOperator, (left, right) => Expression.Invoke() },
                    //{ QueryOptions.StringStartsWithOperator, Expression.Equal },
                    //{ QueryOptions.StringEndsWithOperator, Expression.Equal },
                };
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

            public IQueryable<TEntity> ApplyToQuery<TEntity>(IQueryable<TEntity> query, bool first)
            {
                if ( MetaProperty == null )
                {
                    throw new InvalidOperationException("MetaProperty must be set before calling this method.");
                }

                return MetaProperty.MakeOrderBy(query, first, ascending: this.Ascending);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string PropertyName { get; private set; }
            public bool Ascending { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IPropertyMetadata MetaProperty { get; set; }
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
            public abstract void Query(QueryOptions options, out IDomainObject[] resultSet, out long resultCount, out bool moreAvailable);
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
            where TEntity : class
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

            public override void Query(QueryOptions options, out IDomainObject[] resultSet, out long resultCount, out bool moreAvailable)
            {
                resultSet = null;
                moreAvailable = false;

                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var repository = context.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                    IQueryable<TEntity> dbQuery = repository;

                    dbQuery = HandleFilter(options, dbQuery);
                    dbQuery = HandleOrderBy(options, dbQuery);
                    dbQuery = HandlePaging(options, dbQuery);

                    if ( options.IsCountOnly && !options.NeedInMemoryOperations )
                    {
                        resultCount = dbQuery.Count();
                        return;
                    }

                    IEnumerable<TEntity> queryResults = new QueryResultEnumerable<TEntity>(dbQuery);

                    if ( options.NeedInMemoryOperations )
                    {
                        queryResults = HandleInMemoryOperations(options, queryResults);
                    }

                    if ( options.IsCountOnly )
                    {
                        resultCount = queryResults.Count();
                        return;
                    }

                    if ( options.Take.HasValue )
                    {
                        var buffer = queryResults.ToList();
                        moreAvailable = (buffer.Count > options.Take.Value);
                        resultSet = buffer.Take(options.Take.Value).Cast<IDomainObject>().ToArray();
                    }
                    else
                    {
                        resultSet = queryResults.Cast<IDomainObject>().ToArray();
                    }

                    resultCount = resultSet.Length;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IEnumerable<TEntity> HandleInMemoryOperations(QueryOptions options, IEnumerable<TEntity> dbCursor)
            {
                IQueryable<TEntity> inMemoryQuery = dbCursor.AsQueryable();

                foreach ( var filterItem in options.InMemoryFilter )
                {
                    inMemoryQuery = inMemoryQuery.Where(filterItem.MakePredicateExpression<TEntity>());
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

            public override IDomainObject GetById(string id)
            {
                using ( var context = Framework.NewUnitOfWork<TContext>() )
                {
                    var repository = context.GetEntityRepository(typeof(TEntity)).As<IEntityRepository<TEntity>>();
                    var idProperty = MetaType.PrimaryKey.Properties[0];
                    IQueryable<TEntity> query = repository.Where(idProperty.MakeBinaryExpression<TEntity>(
                        valueString: id, 
                        binaryFactory: Expression.Equal));
                    
                    var result = query.FirstOrDefault();

                    if ( result == null )
                    {
                        throw new ArgumentException("Specified entity does not exist.");
                    }

                    return result as IDomainObject;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IDomainObject CreateNew()
            {
                return Framework.NewDomainObject<TEntity>() as IDomainObject;
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

            private IQueryable<TEntity> HandleOrderBy(QueryOptions options, IQueryable<TEntity> dbQuery)
            {
                for ( int i = 0 ; i < options.OrderBy.Count ; )
                {
                    var orderItem = options.OrderBy[i];
                    var metaProperty = MetaType.GetPropertyByName(orderItem.PropertyName);

                    if ( metaProperty.IsCalculated )
                    {
                        options.Filter.RemoveAt(i);
                        options.InMemoryOrderBy.Add(orderItem);
                    }
                    else
                    {
                        dbQuery = metaProperty.MakeOrderBy(dbQuery, first: i == 0, ascending: orderItem.Ascending);
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
                    dbQuery = ApplyOfTypeFilter(dbQuery, options.OfType);
                }

                for ( int i = 0 ; i < options.Filter.Count ; )
                {
                    var filterItem = options.Filter[i];
                    filterItem.MetaProperty = MetaType.GetPropertyByName(filterItem.PropertyName);

                    if ( filterItem.MetaProperty.IsCalculated )
                    {
                        options.Filter.RemoveAt(i);
                        options.InMemoryFilter.Add(filterItem);
                    }
                    else
                    {
                        var predicateExpression = filterItem.MakePredicateExpression<TEntity>();
                        dbQuery = dbQuery.Where(predicateExpression);
                        i++;
                    }
                }

                return dbQuery;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IQueryable<TEntity> ApplyOfTypeFilter(IQueryable<TEntity> dbQuery, string typeString)
            {
                var filterMetaType = MetaType.DerivedTypes.FirstOrDefault(t => t.QualifiedName.EqualsIgnoreCase(typeString));

                if ( filterMetaType == null )
                {
                    throw new ArgumentException(string.Format(
                        "Specified entity type '{0}' is not compatible with entity '{1}'.",
                        typeString, MetaType.QualifiedName));
                }

                return filterMetaType.MakeOfType(dbQuery);
            }
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IQueryable<TEntity> HandlePaging(QueryOptions options, IQueryable<TEntity> dbQuery)
            {
                if ( options.InMemoryFilter.Count == 0 && options.InMemoryOrderBy.Count == 0 )
                {
                    if ( options.Skip.HasValue )
                    {
                        dbQuery = dbQuery.Skip(options.Skip.Value + 1);
                    }

                    if ( options.Take.HasValue )
                    {
                        dbQuery = dbQuery.Take(options.Take.Value);
                    }
                }

                return dbQuery;
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
                    var metaTypes = contractTypes.Select(t => _metadataCache.GetTypeMetadata(t)).ToArray();
                    
                    properties = ReplaceRelationPropertiesWithForeignKeys(metaTypes, properties);
                    ConfigureEmbeddedCollectionProperties(metaTypes, properties);
                }

                properties.Insert(0, CreateObjectTypeProperty());
                properties.Insert(1, CreateEntityIdProperty());
                
                return properties;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ConfigureEmbeddedCollectionProperties(ITypeMetadata[] metaTypes, IList<JsonProperty> jsonProperties)
            {
                foreach ( var jsonProperty in jsonProperties )
                {
                    var metaProperty = GetPropertyByName(metaTypes, jsonProperty.PropertyName);

                    if ( IsEmbeddedObjectCollectionProperty(metaProperty) )
                    {
                        var converterClosedType = typeof(EmbeddedDomainObjectCollectionConverter<>).MakeGenericType(metaProperty.Relation.RelatedPartyType.ContractType);
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
                    var metaProperty = GetPropertyByName(metaTypes, originalJsonProperty.PropertyName);

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

            private JsonProperty CreateReplacementForeignKeyCollectionProperty(IPropertyMetadata metaProperty, JsonProperty originalJsonProperty)
            {
                var valueProviderClosedType = 
                    typeof(ForeignKeyCollectionValueProvider<>).MakeGenericType(metaProperty.Relation.RelatedPartyType.ContractType);
                
                var valueProviderInstance = 
                    (IValueProvider)Activator.CreateInstance(valueProviderClosedType, _ownerService, metaProperty, originalJsonProperty);

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
                return (metaProperty.RelationalMapping != null && metaProperty.RelationalMapping.StorageStyle == PropertyStorageStyle.InverseForeignKey);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool ShouldReplacePropertyWithForeignKey(IPropertyMetadata metaProperty)
            {
                if ( metaProperty.Kind != PropertyKind.Relation || !metaProperty.Relation.RelatedPartyType.IsEntity )
                {
                    return false;
                }

                if ( metaProperty.DeclaringContract.IsViewModel )
                {
                    return true;
                }

                return (
                    metaProperty.RelationalMapping != null && 
                    metaProperty.RelationalMapping.StorageStyle == PropertyStorageStyle.InlineForeignKey);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool ShouldReplacePropertyWithForeignKeyCollection(IPropertyMetadata metaProperty)
            {
                if ( metaProperty.Kind != PropertyKind.Relation || !metaProperty.Relation.RelatedPartyType.IsEntity )
                {
                    return false;
                }

                if ( metaProperty.DeclaringContract.IsViewModel )
                {
                    return true;
                }

                return (
                    metaProperty.RelationalMapping != null &&
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

                object relatedDomainObject;
                
                if ( LoadRelatedDomainObjectByForeignKey(reader, objectType, out relatedDomainObject) )
                {
                    return relatedDomainObject;
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
                int arrayItemIndex = 0;

                while ( reader.Read() && reader.TokenType != JsonToken.EndArray )
                {
                    JObject jo = JObject.Load(reader);

                    TObject itemToPopulate = null;

                    if ( _relatedMetaType.IsEntity )
                    {
                        var idString = jo["$id"].Value<string>();
                        itemToPopulate = existingCollection.FirstOrDefault(obj => obj.As<IPersistableObject>().As<IEntityObject>().GetId().Value.ToString() == idString);
                    }
                    else
                    {
                        var existingItemIndexToken = jo["$index"];
                        var itemIndex = (existingItemIndexToken != null ? Int32.Parse(existingItemIndexToken.Value<string>()) : arrayItemIndex);
                        itemToPopulate = existingCollection.Skip(itemIndex).Take(1).FirstOrDefault();
                    }

                    if ( itemToPopulate == null )
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

                    arrayItemIndex++;
                }

                return existingValue;


                //if ( reader.TokenType == JsonToken.Null )
                //{
                //    return null;
                //}

                //object relatedDomainObject;

                //if ( LoadRelatedDomainObjectByForeignKey(reader, objectType, out relatedDomainObject) )
                //{
                //    return relatedDomainObject;
                //}

                //JObject jo = JObject.Load(reader);

                //var typeName = jo["$type"].Value<string>();
                //var handler = _ownerService._handlerByEntityName[typeName];
                //var target = handler.CreateNew();

                //JsonReader jObjectReader = jo.CreateReader();
                //jObjectReader.Culture = reader.Culture;
                //jObjectReader.DateParseHandling = reader.DateParseHandling;
                //jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                //jObjectReader.FloatParseHandling = reader.FloatParseHandling;

                //serializer.Populate(jObjectReader, target);
                //return target;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanConvert(Type objectType)
            {
                Type itemType;
                
                return (
                    objectType.IsCollectionType(out itemType) && 
                    (itemType.IsEntityContract() || itemType.IsEntityPartContract()));
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
                throw new NotImplementedException();
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

                var nestedSerializer = JsonSerializer.Create(_ownerService._serializerSettings);
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
