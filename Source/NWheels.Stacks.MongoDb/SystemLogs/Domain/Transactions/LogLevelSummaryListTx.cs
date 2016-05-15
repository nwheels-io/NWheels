using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;
using NWheels.UI;
using NWheels.UI.Factories;
using RecordKey = System.Tuple<string, string, string, string, string>;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    public class LogLevelSummaryListTx : AbstractLogLevelSummaryListTx
    {
        private readonly IFramework _framework;
        private readonly MongoDbThreadLogQueryService _queryService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogLevelSummaryListTx(IFramework framework, IViewModelObjectFactory viewModelFactory, MongoDbThreadLogQueryService queryService)
            : base(framework, viewModelFactory)
        {
            _framework = framework;
            _queryService = queryService;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Input,ILogTimeRangeCriteria,IQueryable<ILogLevelSummaryEntity>>

        public override IQueryable<ILogLevelSummaryEntity> Execute(ILogTimeRangeCriteria input)
        {
            MongoDbThreadLogQueryService.NormalizeTimeRange(input);

            var query = (UIOperationContext.Current != null ? UIOperationContext.Current.Query : null);
            var task = _queryService.QueryDailySummaryAsync(input, query, CancellationToken.None);
            task.Wait();

            var resultByRecordKey = new Dictionary<RecordKey, LogLevelSummaryEntity>();

            foreach (var record in task.Result)
            {
                var result = resultByRecordKey.GetOrAdd(GetRecordKey(record), CreateResultRow);

                if (record.Date >= input.From && record.Date.AddDays(1) <= input.Until)
                {
                    IncrementByWholeRecord(result, record);
                }
                else if (record.Date == input.From.Date || record.Date == input.Until.Date)
                {
                    IncrementByPartialRecord(result, record, input);
                }
            }

            return resultByRecordKey.Values.AsQueryable();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void IncrementByWholeRecord(LogLevelSummaryEntity result, DailySummaryRecord record)
        {
            for (int hour = 0 ; hour < 24 ; hour++)
            {
                var count = record.Hour.GetValueOrDefault(key: hour.ToString(), defaultValue: 0);
                result.Increment(record.Level, count);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void IncrementByPartialRecord(LogLevelSummaryEntity result, DailySummaryRecord record, ILogTimeRangeCriteria input)
        {
            int firstWholeHour = (record.Date == input.From.Date ? input.From.Hour + 1 : 0);
            int lastWholeHour = (record.Date == input.Until.Date ? input.Until.Hour - 1 : 23);

            if (firstWholeHour > 0)
            {
                IncrementByPartialHour(result, record, input, firstWholeHour - 1);
            }

            for (var hour = firstWholeHour ; hour <= lastWholeHour ; hour++)
            {
                var count = record.Hour.GetValueOrDefault(key: hour.ToString(), defaultValue: 0);
                result.Increment(record.Level, count);
            }

            if (lastWholeHour < 23)
            {
                IncrementByPartialHour(result, record, input, lastWholeHour + 1);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void IncrementByPartialHour(LogLevelSummaryEntity result, DailySummaryRecord record, ILogTimeRangeCriteria input, int hour)
        {
            Dictionary<string, int> countByMinute;

            if (!record.Minute.TryGetValue(hour.ToString(), out countByMinute))
            {
                return;
            }

            var hourTimestamp = record.Date.AddHours(hour);

            for (int minute = 0 ; minute < 60 ; minute++)
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private RecordKey GetRecordKey(DailySummaryRecord record)
        {
            return new RecordKey(record.EnvironmentName, record.MachineName, record.NodeName, record.NodeInstance, record.NodeInstanceReplica);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
}
