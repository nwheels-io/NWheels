using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
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
        private ImmutableArray<string> _environmentList;
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
            ApplicationEntityService.QueryOptions options,
            CancellationToken cancellation)
        {
            var environmentFilter = TryTakePropertyFilter(options, _s_queryEnvironmentProperty);

            var dbCriteria = new List<IMongoQuery>();

            if (timeRange.From.Date != timeRange.Until.Date)
            {
                dbCriteria.Add(Query<DailySummaryRecord>.GTE(x => x.Date, timeRange.From.Date));
                dbCriteria.Add(Query<DailySummaryRecord>.LT(x => x.Date, timeRange.Until.Date));
            }
            else
            {
                dbCriteria.Add(Query<DailySummaryRecord>.EQ(x => x.Date, timeRange.From.Date));
            }

            if (options != null)
            {
                RefineDbQuery<DailySummaryRecord>(dbCriteria, options);
            }

            var dbQuery = Query.And(dbCriteria);

            return RunEnvironmentMapReduceQuery<DailySummaryRecord>(
                environmentFilter,
                queryFunc: (db, environmentName) => {
                    var collection = db.GetCollection<DailySummaryRecord>(DbNamingConvention.GetDailySummaryCollectionName(environmentName));
                    return collection.Find(dbQuery);
                },
                cancellation: cancellation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task<IEnumerable<LogMessageRecord>> QueryLogMessagesAsync(
            ILogTimeRangeCriteria timeRange,
            ApplicationEntityService.QueryOptions options,
            CancellationToken cancellation)
        {
            var environmentFilter = TryTakePropertyFilter(options, _s_queryEnvironmentProperty);

            var dbCriteria = new List<IMongoQuery>();
            dbCriteria.Add(Query<LogMessageRecord>.GTE(x => x.Timestamp, timeRange.From));
            dbCriteria.Add(Query<LogMessageRecord>.LT(x => x.Timestamp, timeRange.Until));

            if (options != null)
            {
                RefineDbQuery<LogMessageRecord>(dbCriteria, options);
            }

            var dbQuery = Query.And(dbCriteria);

            return RunEnvironmentMapReduceQuery<LogMessageRecord>(
                environmentFilter,
                queryFunc: (db, environmentName) => {
                    var collection = db.GetCollection<LogMessageRecord>(DbNamingConvention.GetLogMessageCollectionName(environmentName));
                    return collection.Find(dbQuery);
                },
                cancellation: cancellation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task<IEnumerable<ThreadLogRecord>> QueryThreadLogsAsync(
            ILogTimeRangeCriteria timeRange,
            ApplicationEntityService.QueryOptions options,
            CancellationToken cancellation)
        {
            var environmentFilter = TryTakePropertyFilter(options, _s_queryEnvironmentProperty);

            var dbCriteria = new List<IMongoQuery>();
            dbCriteria.Add(Query<ThreadLogRecord>.GTE(x => x.Timestamp, timeRange.From));
            dbCriteria.Add(Query<ThreadLogRecord>.LT(x => x.Timestamp, timeRange.Until));

            if (options != null)
            {
                RefineDbQuery<ThreadLogRecord>(dbCriteria, options);
            }

            var dbQuery = Query.And(dbCriteria);

            return RunEnvironmentMapReduceQuery<ThreadLogRecord>(
                environmentFilter,
                queryFunc: (db, environmentName) => {
                    var collection = db.GetCollection<ThreadLogRecord>(DbNamingConvention.GetThreadLogCollectionName(environmentName));
                    var cursor = collection.Find(dbQuery);
                    var fields = new FieldsBuilder();
                    fields.Exclude("Snapshot.RootActivity.SubNodes");
                    fields.Exclude("Snapshot.RootActivity.ExceptionDetails");
                    cursor.SetFields(fields);
                    return cursor;
                },
                cancellation: cancellation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task<IEnumerable<ThreadLogRecord>> QueryThreadLogsAsync(
            IThreadLogSearchCriteria criteria,
            CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(criteria.Id) && string.IsNullOrEmpty(criteria.CorrelationId))
            {
                throw new ArgumentException("Either Log ID or Correlation ID must be specified.");
            }

            var dbCriteria = new List<IMongoQuery>();

            if (!string.IsNullOrEmpty(criteria.Id))
            {
                dbCriteria.Add(Query<ThreadLogRecord>.EQ(x => x.LogId, criteria.Id));
            }
            
            if (!string.IsNullOrEmpty(criteria.CorrelationId))
            {
                dbCriteria.Add(Query<ThreadLogRecord>.EQ(x => x.CorrelationId, criteria.CorrelationId));
            }

            //RefineDbQuery<ThreadLogRecord>(dbCriteria, criteria);

            var dbQuery = Query.Or(dbCriteria);

            return RunEnvironmentMapReduceQuery<ThreadLogRecord>(
                environmentFilter: null,
                queryFunc: (db, environmentName) => {
                    var collection = db.GetCollection<ThreadLogRecord>(DbNamingConvention.GetThreadLogCollectionName(environmentName));
                    return collection.Find(dbQuery);
                },
                cancellation: cancellation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RefineDbQuery<TRecord>(List<IMongoQuery> criteria, ApplicationEntityService.QueryOptions options)
            where TRecord : LogRecordBase
        {
            RefineDbQueryProperty<TRecord>(criteria, x => x.MachineName, TryTakePropertyFilter(options, _s_queryMachineProperty));
            RefineDbQueryProperty<TRecord>(criteria, x => x.EnvironmentName, TryTakePropertyFilter(options, _s_queryEnvironmentProperty));
            RefineDbQueryProperty<TRecord>(criteria, x => x.NodeName, TryTakePropertyFilter(options, _s_queryNodeProperty));
            RefineDbQueryProperty<TRecord>(criteria, x => x.NodeInstance, TryTakePropertyFilter(options, _s_queryInstanceProperty));
            RefineDbQueryProperty<TRecord>(criteria, x => x.NodeInstanceReplica, TryTakePropertyFilter(options, _s_queryReplicaProperty));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RefineDbQueryProperty<TRecord>(
            List<IMongoQuery> criteria,
            Expression<Func<TRecord, object>> propertySelector,  
            ApplicationEntityService.QueryFilterItem filter)
            where TRecord : LogRecordBase
        {
            if (filter != null && filter.Operator.EqualsIgnoreCase(ApplicationEntityService.QueryOptions.StringContainsOperator))
            {
                criteria.Add(Query.Matches(
                    propertySelector.GetPropertyInfo().Name, 
                    BsonRegularExpression.Create(new Regex(filter.StringValue, RegexOptions.IgnoreCase))
                ));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ApplicationEntityService.QueryFilterItem TryTakePropertyFilter(
            ApplicationEntityService.QueryOptions query, 
            PropertyInfo property)
        {
            if (query != null)
            {
                var filter = query.Filter.FirstOrDefault(f => f.PropertyName.EqualsIgnoreCase(property.Name));
                
                if (filter != null)
                {
                    query.Filter.Remove(filter);
                    return filter;
                }
            }

            return null;
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
            ApplicationEntityService.QueryFilterItem environmentFilter,
            Func<MongoDatabase, string, IEnumerable<TRecord>> queryFunc,
            CancellationToken cancellation)
            where TRecord : LogRecordBase
        {
            var db = SafeGetDatabase();
            IEnumerable<string> environmentNamesToSearch = SafeGetEnvironmentList();

            if (environmentFilter != null)
            {
                environmentNamesToSearch = environmentNamesToSearch.Where(name => name.ContainsIgnoreCase(environmentFilter.StringValue));
            }

            return MapReduceTask.StartNew<string, IEnumerable<TRecord>, IEnumerable<TRecord>>(
                map: (environmentName) => {
                    var partialResult = queryFunc(db, environmentName);
                    return partialResult.Select(r => {
                        r.EnvironmentName = environmentName;
                        return r;
                    });
                },
                reduce: (partialResults) => {
                    var results = partialResults.SelectMany(r => r);
                    return results;
                },
                inputs: environmentNamesToSearch.ToArray(),
                cancellation: cancellation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ImmutableArray<string> SafeGetEnvironmentList()
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
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToImmutableArray();
                        
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
        private static readonly PropertyInfo _s_threadLogRecordSnapshotProperty =
            ExpressionUtility.GetPropertyInfoFrom<ThreadLogRecord>(x => x.Snapshot);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void NormalizeTimeRange(ILogTimeRangeCriteria timeRange)
        {
            timeRange.From = new DateTime(timeRange.From.Ticks, DateTimeKind.Utc);

            if (timeRange.Until.TimeOfDay == TimeSpan.FromHours(24).Subtract(TimeSpan.FromSeconds(1)))
            {
                timeRange.Until = new DateTime(timeRange.From.Date.AddDays(1).Ticks, DateTimeKind.Utc);
            }
            else
            {
                timeRange.Until = new DateTime(timeRange.Until.Ticks, DateTimeKind.Utc);
            }
        }
    }
}
