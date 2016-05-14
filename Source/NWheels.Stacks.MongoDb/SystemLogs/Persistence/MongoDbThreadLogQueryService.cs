using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NWheels.Concurrency;
using NWheels.DataObjects;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Logging.Core;
using NWheels.UI;
using NWheels.Utilities;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
{
    public class MongoDbThreadLogQueryService
    {
        private readonly object _databaseSyncRoot = new object();
        private readonly object _environmentListSyncRoot = new object();
        private readonly TimeSpan _environmentListRefreshInterval = TimeSpan.FromMinutes(1);
        private readonly IFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IFrameworkLoggingConfiguration _loggingConfig;
        private readonly ITypeMetadata _baseMetaEntity;
        private MongoDatabase _database;
        private ImmutableHashSet<string> _environmentList;
        private long _environmentListTimestamp;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoDbThreadLogQueryService(IFramework framework, ITypeMetadataCache metadataCache, IFrameworkLoggingConfiguration loggingConfig)
        {
            _framework = framework;
            _metadataCache = metadataCache;
            _loggingConfig = loggingConfig;
            _baseMetaEntity = metadataCache.GetTypeMetadata(typeof(IBaseLogDimensionsEntity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task<IEnumerable<DailySummaryRecord>> QueryDailySummaryAsync(
            ILogTimeRangeCriteria timeRange,
            ApplicationEntityService.QueryOptions query,
            CancellationToken cancellation)
        {
            var environments = GetPropertyFilterValuesFromQuery(query, _s_queryEnvironmentProperty);

            return RunEnvironmentMapReduceQuery<DailySummaryRecord>(
                environments,
                queryFunc: (db, environmentName) => {
                    var collection = db.GetCollection<DailySummaryRecord>(DbNamingConvention.GetDailySummaryCollectionName(environmentName));
                    return collection.FindAll();
                },
                cancellation: cancellation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string[] GetPropertyFilterValuesFromQuery(ApplicationEntityService.QueryOptions query, PropertyInfo property)
        {
            if (query == null)
            {
                return null;
            }

            var filterItem = (
                query.Filter.FirstOrDefault(f => f.PropertyName.EqualsIgnoreCase(property.Name)) ??
                query.InMemoryFilter.FirstOrDefault(f => f.PropertyName.EqualsIgnoreCase(property.Name)));

            if (filterItem != null && !string.IsNullOrEmpty(filterItem.StringValue))
            {
                if (filterItem.Operator == ApplicationEntityService.QueryOptions.IsInOperator)
                {
                    return filterItem.StringValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else if (filterItem.Operator == ApplicationEntityService.QueryOptions.EqualOperator)
                {
                    return new[] { filterItem.StringValue };
                }
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Task<IEnumerable<TRecord>> RunEnvironmentMapReduceQuery<TRecord>(
            string[] environments,
            Func<MongoDatabase, string, IEnumerable<TRecord>> queryFunc,
            CancellationToken cancellation)
        {
            var db = SafeGetDatabase();
            var environmentNamesToSearch = SafeGetEnvironmentList();

            if (environments != null && environments.Length > 0)
            {
                environmentNamesToSearch = environmentNamesToSearch.Intersect(environments);
            }

            return MapReduceTask.StartNew<string, IEnumerable<TRecord>, IEnumerable<TRecord>>(
                map: (environmentName) => {
                    var partialResult = queryFunc(db, environmentName);
                    return partialResult;
                },
                reduce: (partialResults) => {
                    var results = partialResults.SelectMany(r => r);
                    return results;
                },
                inputs: environmentNamesToSearch.ToArray(),
                cancellation: cancellation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ImmutableHashSet<string> SafeGetEnvironmentList()
        {
            if (ShouldReloadEnvironmentList(_framework.UtcNow))
            {
                var db = SafeGetDatabase();

                if (!Monitor.TryEnter(_environmentListSyncRoot, 30000))
                {
                    throw new TimeoutException("Timed out waiting for environment list lock (30 sec).");
                }

                try
                {
                    var now = _framework.UtcNow;

                    if (ShouldReloadEnvironmentList(now))
                    {
                        _environmentList = db.GetCollectionNames()
                            .Where(name => name.StartsWith(DbNamingConvention.CollectionNamePrefix))
                            .Select(name => name
                                .TrimLead(DbNamingConvention.CollectionNamePrefix)
                                .TrimTail(DbNamingConvention.DailySummaryCollectionNameSuffix)
                                .TrimTail(DbNamingConvention.LogMessageCollectionNameSuffix)
                                .TrimTail(DbNamingConvention.ThreadLogCollectionNameSuffix))
                             .ToImmutableHashSet(StringComparer.CurrentCultureIgnoreCase);
                        
                        _environmentListTimestamp = Interlocked.Exchange(ref _environmentListTimestamp, now.Ticks);
                    }
                }
                finally
                {
                    Monitor.Exit(_environmentListSyncRoot);
                }
            }

            return _environmentList;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MongoDatabase SafeGetDatabase()
        {
            if (_database == null)
            {
                if (!Monitor.TryEnter(_databaseSyncRoot, 30000))
                {
                    throw new TimeoutException("Timed out waiting for database connection lock (30 sec).");
                }

                try
                {
                    if (_database == null)
                    {
                        _database = MongoDbThreadLogPersistor.ConnectToDatabase(_loggingConfig);
                    }
                }
                finally
                {
                    Monitor.Exit(_databaseSyncRoot);
                }
            }

            return _database;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldReloadEnvironmentList(DateTime now)
        {
            return (
                _environmentList == null || 
                now.Subtract(new DateTime(_environmentListTimestamp)) >= _environmentListRefreshInterval);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly PropertyInfo _s_queryMachineProperty =
            ExpressionUtility.GetPropertyInfoFrom<IBaseLogDimensionsEntity>(x => x.Machine);
        private static readonly PropertyInfo _s_queryEnvironmentProperty =
            ExpressionUtility.GetPropertyInfoFrom<IBaseLogDimensionsEntity>(x => x.Environment);
        private static readonly PropertyInfo _s_queryNodeProperty =
            ExpressionUtility.GetPropertyInfoFrom<IBaseLogDimensionsEntity>(x => x.Node);
        private static readonly PropertyInfo _s_queryInstanceProperty =
            ExpressionUtility.GetPropertyInfoFrom<IBaseLogDimensionsEntity>(x => x.Instance);
        private static readonly PropertyInfo _s_queryReplicaProperty =
            ExpressionUtility.GetPropertyInfoFrom<IBaseLogDimensionsEntity>(x => x.Replica);
    }
}
