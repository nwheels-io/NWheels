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
            [Detail(GroupStats = true, IncludeInSingleLineText = true)] string entity, 
            [Detail(IncludeInSingleLineText = true)] string collection, 
            [Detail(Mutable = true)] int resultCount);

        [LogDebug]
        void QueryPlanExplained([Detail(MaxStringLength = 1024)] string queryPlan);

        [LogDebug]
        void QueryResult(string entity, int rowNumber);

        [LogVerbose]
        void EndOfQueryResults(int rowCount);

        [LogDebug]
        void DisposingQueryResultEnumerator();

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
        ILogActivity MigratingDatabase(string name);
    }
}
