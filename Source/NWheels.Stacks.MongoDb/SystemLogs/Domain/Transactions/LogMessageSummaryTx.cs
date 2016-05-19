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
using RecordKey = System.Tuple<string, string, string, string, string, NWheels.Logging.LogLevel?, System.Tuple<string, string, string>>;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    public class LogMessageSummaryTx : AbstractLogMessageSummaryTx
    {
        private readonly IFramework _framework;
        private readonly MongoDbThreadLogQueryService _queryService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogMessageSummaryTx(IFramework framework, IViewModelObjectFactory viewModelFactory, MongoDbThreadLogQueryService queryService)
            : base(framework, viewModelFactory)
        {
            _framework = framework;
            _queryService = queryService;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Input,ILogTimeRangeCriteria,VisualizedQueryable<ILogLevelSummaryEntity>>

        public override IQueryable<ILogMessageSummaryEntity> Execute(ILogTimeRangeCriteria input)
        {
            MongoDbThreadLogQueryService.NormalizeTimeRange(input);

            var query = (UIOperationContext.Current != null ? UIOperationContext.Current.Query : null);
            var task = _queryService.QueryDailySummaryAsync(input, query, CancellationToken.None);
            task.Wait();

            var resultSetBuilder = new ResultSetBuilder(_framework, input);
            var chartBuilder = new ChartBuilder(input);

            if (task.Result != null)
            {
                resultSetBuilder.AddRecords(task.Result);
                chartBuilder.AddRecords(task.Result);
            }

            return new VisualizedQueryable<ILogMessageSummaryEntity>(
                resultSetBuilder.GetResultRows().AsQueryable(),
                chartBuilder.GetChart());
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ResultSetBuilder : SummaryResultSetBuilderBase<RecordKey, LogMessageSummaryEntity>
        {
            public ResultSetBuilder(IFramework framework, ILogTimeRangeCriteria input)
                : base(framework, input)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SummaryResultSetBuilderBase<Tuple<string,string,string,string,string>,LogLevelSummaryEntity>

            protected override RecordKey GetRecordKey(DailySummaryRecord record)
            {
                return new RecordKey(
                    record.EnvironmentName, 
                    record.MachineName, 
                    record.NodeName, 
                    record.NodeInstance, 
                    record.NodeInstanceReplica,
                    record.Level,
                    new Tuple<string, string, string>(record.Logger, record.MessageId, record.ExceptionType));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override LogMessageSummaryEntity CreateResult(RecordKey key)
            {
                var result = Framework.NewDomainObject<ILogMessageSummaryEntity>().As<LogMessageSummaryEntity>();

                result.SetKey(
                    environment: key.Item1,
                    machine: key.Item2,
                    node: key.Item3,
                    instance: key.Item4,
                    replica: key.Item5, 
                    level: key.Item6,
                    logger: key.Item7.Item1,
                    messageId: key.Item7.Item2,
                    exceptionType: key.Item7.Item3);

                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void IncrementResult(LogMessageSummaryEntity result, DailySummaryRecord record, int count)
            {
                result.Increment(record.Level, count);
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ChartBuilder : ChartBuilderBase<DailySummaryRecord, LogLevel>
        {
            public ChartBuilder(ILogTimeRangeCriteria input) 
                : base(input)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            protected override IEnumerable<LogLevel> GetSeries()
            {
                yield return LogLevel.Info;
                yield return LogLevel.Warning;
                yield return LogLevel.Error;
                yield return LogLevel.Critical;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void Accumulate(DailySummaryRecord record)
            {
                foreach (var hourKvp in record.Minute)
                {
                    var hour = Int32.Parse(hourKvp.Key);
                    var hourTimestamp = record.Date.Add(TimeSpan.FromHours(hour));

                    foreach (var minuteKvp in hourKvp.Value.Where(minuteKvp => minuteKvp.Value > 0))
                    {
                        var minute = Int32.Parse(minuteKvp.Key);
                        var timestamp = hourTimestamp.Add(TimeSpan.FromMinutes(minute));
                        IncrementSeriesTimebox(record.Level, timestamp, minuteKvp.Value);
                    }
                }
            }
        }
    }
}
