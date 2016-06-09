using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using NWheels.Logging;

namespace NWheels.Stacks.MongoDb
{
    public interface IMongoDbLogger : IApplicationEventLogger
    {
        [LogActivity(LogLevel.Verbose, CollectStats = true)]
        ILogActivity Query(
            [Detail(StatsOption = LogStatsOption.GroupBy, IncludeInSingleLineText = true)] string entity, 
            [Detail(IncludeInSingleLineText = true)] string collection, 
            [Detail(Mutable = true, StatsOption = LogStatsOption.Sum)] int resultCount);

        [LogDebug]
        void QueryPlanExplained([Detail(MaxStringLength = 1024)] string queryPlan);

        [LogDebug]
        void QueryResult(string entity, int rowNumber);

        [LogVerbose]
        void EndOfQueryResults(int rowCount);

        [LogDebug]
        void DisposingQueryResultEnumerator(int rowsRead);

        [LogActivity]
        ILogActivity ExecutingInsert(string entityType);

        [LogActivity]
        ILogActivity ExecutingSave(string entityType);

        [LogActivity]
        ILogActivity ExecutingUpdate(string entityType);

        [LogActivity]
        ILogActivity ExecutingDelete(string entityType);

        [LogVerbose]
        void WritingEntityBatch(string entity, string operation, int size);

        [LogError]
        void MongoDbWriteError(string message);

        [LogDebug]
        void MongoDbWriteResult(long documentsAffected, BsonValue upserted, bool updatedExisting);

        [LogDebug]
        void BulkWriteResult(string entity, string operation, int size, long inserted, long deleted, long modified, long matched);

        [LogActivity]
        ILogActivity InitializingDatabaseIndexes(string name);

        [LogActivity]
        ILogActivity ExecutingMigrationCollection(Type collectionType);

        [LogActivity]
        ILogActivity ExecutingMigration(int version, string name);

        [LogInfo]
        void MigratingDatabaseSchema(string name, int dbVersion, int appVersion);

        [LogInfo]
        void DatabaseMigrationCompleted(string name, int newVersion);

        [LogInfo]
        void DatabaseIsUpToDate(string name, int currentVersion);
    }
}
