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
            resultSetBuilder.AddRecords(task.Result);

            var chartBuilder = new ChartBuilder(input);
            chartBuilder.AddRecords(task.Result);

            return new VisualizedQueryable<ILogLevelSummaryEntity>(
                resultSetBuilder.GetResultRows().AsQueryable(),
                chartBuilder.GetChart());
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ResultSetBuilder
        {
            private readonly Dictionary<RecordKey, LogLevelSummaryEntity> _resultByRecordKey = new Dictionary<RecordKey, LogLevelSummaryEntity>();
            private readonly IFramework _framework;
            private readonly ILogTimeRangeCriteria _input;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ResultSetBuilder(IFramework framework, ILogTimeRangeCriteria input)
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ChartBuilder
        {
            private readonly ILogTimeRangeCriteria _input;
            private readonly TimeSpan _timeboxSize;
            private readonly ApplicationEntityService.QueryOptions _query;
            private readonly Dictionary<LogLevel, ChartData.TimeSeriesData> _seriesByLogLevel;
            private readonly ImmutableArray<DateTime> _timeboxGrid;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ChartBuilder(ILogTimeRangeCriteria input)
            {
                _input = input;

                _query = (UIOperationContext.Current != null ? UIOperationContext.Current.Query : null);
                _timeboxSize = VisualizationHelpers.GetTimeboxSize(_input);
                _seriesByLogLevel = new Dictionary<LogLevel, ChartData.TimeSeriesData>() {
                    { LogLevel.Info, VisualizationHelpers.CreateChartSeries(_timeboxSize, "Info") },
                    { LogLevel.Warning, VisualizationHelpers.CreateChartSeries(_timeboxSize, "Warning") },
                    { LogLevel.Error, VisualizationHelpers.CreateChartSeries(_timeboxSize, "Error") },
                    { LogLevel.Critical, VisualizationHelpers.CreateChartSeries(_timeboxSize, "Critical") }
                };

                _timeboxGrid = BuildTimeboxGrid();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddRecords(IEnumerable<DailySummaryRecord> records)
            {
                Action<DailySummaryRecord> accumulator;

                if (_timeboxSize.TotalHours < 1)
                {
                    accumulator = AccumulateWithMinutePrecision;
                }
                else
                {
                    accumulator = AccumulateWithHourPrecision;
                }

                foreach (var record in records)
                {
                    accumulator(record);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ChartData GetChart()
            {
                return new ChartData() {
                    Series = _seriesByLogLevel.Values.Cast<ChartData.AbstractSeriesData>().ToList()
                };
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private void AccumulateWithHourPrecision(DailySummaryRecord record)
            {
                foreach (var kvp in record.Hour.Where(kvp => kvp.Value > 0))
                {
                    var hour = Int32.Parse(kvp.Key);
                    var timestamp = record.Date.Add(TimeSpan.FromHours(hour));
                    IncrementSeriesTimebox(record.Level, timestamp, kvp.Value);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private void AccumulateWithMinutePrecision(DailySummaryRecord record)
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

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void IncrementSeriesTimebox(LogLevel level, DateTime timestamp, int count)
            {
                ChartData.TimeSeriesData series;

                if (_seriesByLogLevel.TryGetValue(level, out series))
                {
                    var timeboxIndex = TryGetTimeboxIndex(timestamp);

                    if (timeboxIndex >= 0)
                    {
                        series.Points[timeboxIndex].Value += count;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private int TryGetTimeboxIndex(DateTime timestamp)
            {
                var timeboxIndex = _timeboxGrid.BinarySearch(timestamp);

                if (timeboxIndex < 0)
                {
                    timeboxIndex = ~timeboxIndex - 1;
                }

                if (timeboxIndex < 0 || timeboxIndex >= _timeboxGrid.Length)
                {
                    return -1;
                }

                return timeboxIndex;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private ImmutableArray<DateTime> BuildTimeboxGrid()
            {
                var timeboxGridBuilder = ImmutableArray.CreateBuilder<DateTime>();
                var timeboxGridStart = VisualizationHelpers.SnapPointToTimebox(_input.From, _timeboxSize);

                for (var timebox = timeboxGridStart; timebox < _input.Until; VisualizationHelpers.MoveToNextTimebox(ref timebox, _timeboxSize))
                {
                    var utcTimestamp = timebox;
                    timeboxGridBuilder.Add(utcTimestamp);

                    _seriesByLogLevel.Values.ForEach((series, index) => series.Points.Add(new ChartData.TimeSeriesPoint() {
                        UtcTimestamp = utcTimestamp,
                        Value = 0
                    }));
                }

                return timeboxGridBuilder.ToImmutable();
            }
        }
    }
}
