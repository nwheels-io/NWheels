using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Extensions;
using NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    public abstract class SummaryResultSetBuilderBase<TRecordKey, TResultEntity>
    {
        private readonly Dictionary<TRecordKey, TResultEntity> _resultByRecordKey = new Dictionary<TRecordKey, TResultEntity>();
        private readonly IFramework _framework;
        private readonly ILogTimeRangeCriteria _input;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected SummaryResultSetBuilderBase(IFramework framework, ILogTimeRangeCriteria input)
        {
            _framework = framework;
            _input = input;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddRecords(IEnumerable<DailySummaryRecord> records)
        {
            foreach (var record in records)
            {
                var result = _resultByRecordKey.GetOrAdd(GetRecordKey(record), CreateResult);

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

        public IEnumerable<TResultEntity> GetResultRows()
        {
            return _resultByRecordKey.Values;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract TRecordKey GetRecordKey(DailySummaryRecord record);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract TResultEntity CreateResult(TRecordKey key);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void IncrementResult(TResultEntity result, DailySummaryRecord record, int count);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected IFramework Framework
        {
            get { return _framework; }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void IncrementByWholeRecord(TResultEntity result, DailySummaryRecord record)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var count = record.Hour.GetValueOrDefault(key: hour.ToString(), defaultValue: 0);
                IncrementResult(result, record, count);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void IncrementByPartialRecord(TResultEntity result, DailySummaryRecord record, ILogTimeRangeCriteria input)
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
                IncrementResult(result, record, count);
            }

            if (lastWholeHour < 23)
            {
                IncrementByPartialHour(result, record, input, lastWholeHour + 1);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void IncrementByPartialHour(TResultEntity result, DailySummaryRecord record, ILogTimeRangeCriteria input, int hour)
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
                        IncrementResult(result, record, value);
                    }
                }
            }
        }
    }
}
