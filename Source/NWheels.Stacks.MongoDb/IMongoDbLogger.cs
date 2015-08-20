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
        [LogActivity]
        ILogActivity ExecutingQuery(Expression expression, string nativeQuery);

        [LogDebug]
        void QueryPlanExplained([Detail(MaxStringLength = 1024)] string queryPlan);

        [LogDebug]
        void QueryResult(string entity, int rowNumber);

        [LogVerbose]
        void EndOfQueryResults(int rowCount);

        [LogDebug]
        void DisposingQueryResultEnumerator();

        [LogActivity]
        ILogActivity ExecutingInsert(string entityType, int entityCount);

        [LogActivity]
        ILogActivity ExecutingUpdate(string entityType, int entityCount);

        [LogActivity]
        ILogActivity ExecutingDelete(string entityType, int entityCount);

        [LogError]
        void MongoDbWriteError(string message);

        [LogDebug]
        void MongoDbWriteResult(long documentsAffected, BsonValue upserted, bool updatedExisting);
    }
}
