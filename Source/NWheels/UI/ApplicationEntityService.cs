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

namespace NWheels.UI
{
    public class ApplicationEntityService
    {
        private readonly IFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IDomainContextLogger _domainContextLogger;
        private readonly Dictionary<string, EntityHandler> _handlerByEntityName;

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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsEntityNameRegistered(string entityName)
        {
            return _handlerByEntityName.ContainsKey(entityName);
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

                json = JsonConvert.SerializeObject(results, new JsonSerializerSettings() {
                    MaxDepth = 1, 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            }

            return json;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StoreEntity(string entityName, EntityState entityState, string entityId, string json)
        {
            var handler = _handlerByEntityName[entityName];

            using ( var context = handler.NewUnitOfWork() )
            {
                if ( entityState.IsNew() )
                {
                    var newEntity = handler.CreateNew();
                    JsonConvert.PopulateObject(json, newEntity);
                    handler.Insert(newEntity);
                }
                else if ( entityState.IsModified() )
                {
                    var existingEntity = handler.GetById(entityId);
                    JsonConvert.PopulateObject(json, existingEntity);
                    handler.Update(existingEntity);
                }
                else if ( entityState.IsDeleted() )
                {
                    handler.Delete(entityId);
                }
                else
                {
                    throw new ArgumentException("Unexpected value of entity state: " + entityState);
                }

                context.CommitChanges();
            }
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

            private IEnumerable<TEntity> FilterCalculatedValues(QueryOptions options, TEntity[] resultSet)
            {
                IQueryable<TEntity> query = null;

                foreach (var equalityFilterItem in options.EqualityFilter)
                {
                    var metaProperty = MetaType.GetPropertyByName(equalityFilterItem.Key);

                    if (metaProperty.IsCalculated == false)
                    {
                        continue;
                    }

                    if (query == null)
                    {
                        query = resultSet.AsQueryable<TEntity>();
                    }

                    object parsedValue = metaProperty.ParseStringValue(equalityFilterItem.Value);
                    query = query.Where(metaProperty.MakeEqualityComparison<TEntity>(parsedValue));
                }

                if (query == null)
                    return resultSet;

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
    }
}
