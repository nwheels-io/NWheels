using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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
        void ObjectReadFromQueryResult(int rowNumber);

        [LogVerbose]
        void EndOfQueryResults(int rowCount);

        [LogDebug]
        void DisposingQueryResultEnumerator();
    }
}
