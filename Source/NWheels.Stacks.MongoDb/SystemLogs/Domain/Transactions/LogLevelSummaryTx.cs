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
    public class LogLevelSummaryTx : AbstractLogLevelSummaryTx
    {
        private readonly IFramework _framework;
        private readonly MongoDbThreadLogQueryService _queryService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogLevelSummaryTx(IFramework framework, IViewModelObjectFactory viewModelFactory, MongoDbThreadLogQueryService queryService)
            : base(framework, viewModelFactory)
        {
            _framework = framework;
            _queryService = queryService;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Input,ILogTimeRangeCriteria,VisualizedQueryable<ILogLevelSummaryEntity>>

        public override IQueryable<ILogLevelSummaryEntity> Execute(ILogTimeRangeCriteria input)
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

            return new VisualizedQueryable<ILogLevelSummaryEntity>(
                resultSetBuilder.GetResultRows().AsQueryable(),
                chartBuilder.GetChart());
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ResultSetBuilder : SummaryResultSetBuilderBase<RecordKey, LogLevelSummaryEntity>
        {
            public ResultSetBuilder(IFramework framework, ILogTimeRangeCriteria input)
                : base(framework, input)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SummaryResultSetBuilderBase<Tuple<string,string,string,string,string>,LogLevelSummaryEntity>

            protected override RecordKey GetRecordKey(DailySummaryRecord record)
            {
                return new RecordKey(record.EnvironmentName, record.MachineName, record.NodeName, record.NodeInstance, record.NodeInstanceReplica);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override LogLevelSummaryEntity CreateResult(RecordKey key)
            {
                var result = Framework.NewDomainObject<ILogLevelSummaryEntity>().As<LogLevelSummaryEntity>();

                result.SetKey(
                    environment: key.Item1,
                    machine: key.Item2,
                    node: key.Item3,
                    instance: key.Item4,
                    replica: key.Item5);

                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void IncrementResult(LogLevelSummaryEntity result, DailySummaryRecord record, int count)
            {
                result.Increment(record.Level, count);
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

#if false
        private class OldResultSetBuilder
        {
            private readonly Dictionary<RecordKey, LogLevelSummaryEntity> _resultByRecordKey = new Dictionary<RecordKey, LogLevelSummaryEntity>();
            private readonly IFramework _framework;
            private readonly ILogTimeRangeCriteria _input;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public OldResultSetBuilder(IFramework framework, ILogTimeRangeCriteria input)
            {
                _framework = framework;
                _input = input;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddRecords(IEnumerable<DailySummaryRecord> records)
            {
                foreach (var record in records)
                {
                    var result = _resultByRecordKey.GetOrAdd(GetRecordKey(record), CreateResultRow);

                    if (record.Date >= _input.From && record.Date.AddDays(1) <= _input.Until)
                    {
                        IncrementByWholeRecord(result, record);
                    }
                    else if (record.Date == _input.From.Date || record.Date == _input.Until.Date)
                    {
                        IncrementByPartialRecord(result, record, _input);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<ILogLevelSummaryEntity> GetResultRows()
            {
                return _resultByRecordKey.Values;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void IncrementByWholeRecord(LogLevelSummaryEntity result, DailySummaryRecord record)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    var count = record.Hour.GetValueOrDefault(key: hour.ToString(), defaultValue: 0);
                    result.Increment(record.Level, count);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void IncrementByPartialRecord(LogLevelSummaryEntity result, DailySummaryRecord record, ILogTimeRangeCriteria input)
            {
                int firstWholeHour = (record.Date == input.From.Date ? input.From.Hour + 1 : 0);
                int lastWholeHour = (record.Date == input.Until.Date ? input.Until.Hour - 1 : 23);

                if (firstWholeHour > 0)
                {
                    IncrementByPartialHour(result, record, input, firstWholeHour - 1);
                }

                for (var hour = firstWholeHour; hour <= lastWholeHour; hour++)
                {
                    var count = record.Hour.GetValueOrDefault(key: hour.ToString(), defaultValue: 0);
                    result.Increment(record.Level, count);
                }

                if (lastWholeHour < 23)
                {
                    IncrementByPartialHour(result, record, input, lastWholeHour + 1);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void IncrementByPartialHour(LogLevelSummaryEntity result, DailySummaryRecord record, ILogTimeRangeCriteria input, int hour)
            {
                Dictionary<string, int> countByMinute;

                if (!record.Minute.TryGetValue(hour.ToString(), out countByMinute))
                {
                    return;
                }

                var hourTimestamp = record.Date.AddHours(hour);

                for (int minute = 0; minute < 60; minute++)
                {
                    var minuteTimestamp = hourTimestamp.AddMinutes(minute);

                    if (minuteTimestamp >= input.From && minuteTimestamp < input.Until)
                    {
                        var value = countByMinute.GetValueOrDefault(key: minute.ToString(), defaultValue: 0);

                        if (value > 0)
                        {
                            result.Increment(record.Level, value);
                        }
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private RecordKey GetRecordKey(DailySummaryRecord record)
            {
                return new RecordKey(record.EnvironmentName, record.MachineName, record.NodeName, record.NodeInstance, record.NodeInstanceReplica);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private LogLevelSummaryEntity CreateResultRow(RecordKey key)
            {
                var result = _framework.NewDomainObject<ILogLevelSummaryEntity>().As<LogLevelSummaryEntity>();

                result.SetKey(
                    environment: key.Item1,
                    machine: key.Item2,
                    node: key.Item3,
                    instance: key.Item4,
                    replica: key.Item5);

                return result;
            }
        }
#endif

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ChartBuilder : ChartBuilderBase<DailySummaryRecord, SummaryLogLevel>
        {
            public ChartBuilder(ILogTimeRangeCriteria input) 
                : base(input)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            //protected override IEnumerable<LogLevel> GetSeries()
            //{
            //    yield return LogLevel.Critical;
            //    yield return LogLevel.Error;
            //    yield return LogLevel.Warning;
            //    yield return LogLevel.Info;
            //}

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
                        var summaryLevel = (record.Level < LogLevel.Warning ? SummaryLogLevel.Positive : SummaryLogLevel.Negative);

                        //var testFactor = (record.Level > LogLevel.Info ? 10 : 1);

                        IncrementSeriesTimebox(
                            summaryLevel, 
                            timestamp, 
                            count: minuteKvp.Value * record.Level.SignFactor());
                    }
                }
            }
        }
    }
}
