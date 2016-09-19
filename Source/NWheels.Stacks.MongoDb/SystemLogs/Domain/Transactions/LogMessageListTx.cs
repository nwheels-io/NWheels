using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;
using NWheels.Logging;
using NWheels.UI;
using NWheels.UI.Factories;
using NWheels.UI.Toolbox;
using RecordKey = System.Tuple<string, string, string, string, string>;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    public class LogMessageListTx : AbstractLogMessageListTx
    {
        private readonly IFramework _framework;
        private readonly MongoDbThreadLogQueryService _queryService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogMessageListTx(IFramework framework, IViewModelObjectFactory viewModelFactory, MongoDbThreadLogQueryService queryService)
            : base(framework, viewModelFactory)
        {
            _framework = framework;
            _queryService = queryService;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Input,ILogTimeRangeCriteria,IQueryable<ILogMessageEntity>>

        public override IQueryable<ILogMessageEntity> Execute(ILogTimeRangeCriteria input)
        {
            MongoDbThreadLogQueryService.NormalizeTimeRange(input);

            var query = (UIOperationContext.Current != null ? UIOperationContext.Current.Query : null);
            var task = _queryService.QueryLogMessagesAsync(input, query, CancellationToken.None);
            task.Wait();

            var resultSetBuilder = new ResultSetBuilder(_framework);
            var chartBuilder = new ChartBuilder(input);

            if (task.Result != null)
            {
                resultSetBuilder.AddRecords(task.Result);
                chartBuilder.AddRecords(task.Result);
            }

            return new VisualizedQueryable<ILogMessageEntity>(
                resultSetBuilder.GetResultRows().AsQueryable(),
                chartBuilder.GetChart());
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ResultSetBuilder
        {
            private readonly List<ILogMessageEntity> _resultSet = new List<ILogMessageEntity>();
            private readonly IFramework _framework;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ResultSetBuilder(IFramework framework)
            {
                _framework = framework;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddRecords(IEnumerable<LogMessageRecord> records)
            {
                _resultSet.AddRange(records.Select(CreateResult));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<ILogMessageEntity> GetResultRows()
            {
                return _resultSet;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private ILogMessageEntity CreateResult(LogMessageRecord record)
            {
                var result = _framework.NewDomainObject<ILogMessageEntity>().As<LogMessageEntity>();
                result.CopyFromRecord(record);
                return result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ChartBuilder : ChartBuilderBase<LogMessageRecord, SummaryLogLevel>
        {
            public ChartBuilder(ILogTimeRangeCriteria input)
                : base(input)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            //protected override IEnumerable<LogLevel> GetSeries()
            //{
            //    yield return LogLevel.Info;
            //    yield return LogLevel.Warning;
            //    yield return LogLevel.Error;
            //    yield return LogLevel.Critical;
            //}

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void Accumulate(LogMessageRecord record)
            {
                IncrementSeriesTimebox(
                    record.Level.ToSummaryLogLevel(), 
                    record.Timestamp, 
                    count: 1 * record.Level.SignFactor());
            }
        }
    }
}
