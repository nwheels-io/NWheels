using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;
using NWheels.Logging;
using NWheels.UI;
using NWheels.UI.Factories;
using NWheels.UI.Toolbox;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    public class LogLevelSummaryChartTx : AbstractLogLevelSummaryChartTx
    {
        private readonly MongoDbThreadLogQueryService _queryService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogLevelSummaryChartTx(MongoDbThreadLogQueryService queryService)
        {
            _queryService = queryService;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Input,ILogTimeRangeCriteria,ChartData>

        public override ChartData Execute(ILogTimeRangeCriteria input)
        {
            var operation = new QueryOperation(_queryService, input);
            return operation.Execute();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly ImmutableArray<TimeSpan> _s_timeboxGrid = ImmutableArray.Create<TimeSpan>(new[] {
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromHours(1),
            TimeSpan.FromHours(2),
            TimeSpan.FromHours(3),
            TimeSpan.FromHours(4),
            TimeSpan.FromHours(6),
            TimeSpan.FromHours(12),
            TimeSpan.FromDays(1),
            TimeSpan.FromDays(2),
            TimeSpan.FromDays(3),
            TimeSpan.FromDays(7),
            TimeSpan.FromDays(14),
            TimeSpan.FromDays(30),
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ChartData.TimeSeriesData CreateChartSeries(TimeSpan timeboxSize, string label)
        {
            return new ChartData.TimeSeriesData() {
                Type = ChartSeriesType.Bar,
                MillisecondStepSize = (long)timeboxSize.TotalMilliseconds,
                Label = label,
                Points = new List<ChartData.TimeSeriesPoint>()
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void MoveToNextTimebox(ref DateTime value, TimeSpan timeboxSize)
        {
            if (timeboxSize == TimeSpan.FromDays(30))
            {
                value = value.AddMonths(1);
            }
            else
            {
                value = value.Add(timeboxSize);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static DateTime SnapPointToTimebox(DateTime point, TimeSpan timeboxSize)
        {
            if (timeboxSize == TimeSpan.FromDays(1))
            {
                return point.Date;
            }
            else if (timeboxSize == TimeSpan.FromDays(30))
            {
                return point.Date.StartOfMonth();
            }
            else if (timeboxSize.TotalDays < 1)
            {
                long timeboxIndexInDay = point.TimeOfDay.Ticks / timeboxSize.Ticks;
                return point.Date.Add(TimeSpan.FromTicks(timeboxIndexInDay * timeboxSize.Ticks));
            }
            else
            {
                long timeboxIndexInYear = point.Subtract(point.StartOfYear()).Ticks / timeboxSize.Ticks;
                return point.StartOfYear().Add(TimeSpan.FromTicks(timeboxIndexInYear * timeboxSize.Ticks));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static TimeSpan GetTimeboxSize(ILogTimeRangeCriteria timeRange)
        {
            var rangeLength = timeRange.Until.Subtract(timeRange.From);
            var timebox = TimeSpan.FromTicks(rangeLength.Ticks / 24);

            return SnapTimeboxToGrid(timebox);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static TimeSpan SnapTimeboxToGrid(TimeSpan timebox)
        {
            if (timebox < _s_timeboxGrid[0])
            {
                return _s_timeboxGrid[0];
            }

            if (timebox > _s_timeboxGrid[_s_timeboxGrid.Length - 1])
            {
                return _s_timeboxGrid[_s_timeboxGrid.Length - 1];
            }

            TimeSpan snappedTimebox = TimeSpan.Zero;

            for (int i = 0 ; i < _s_timeboxGrid.Length - 1 ; i++)
            {
                if (timebox >= _s_timeboxGrid[i] && timebox <= _s_timeboxGrid[i + 1])
                {
                    var ticks = timebox.Ticks;
                    var middleTicks = _s_timeboxGrid[i].Ticks + (_s_timeboxGrid[i + 1].Ticks - _s_timeboxGrid[i].Ticks) / 2;
                    var snappedTicks = (ticks < middleTicks ? _s_timeboxGrid[i].Ticks : _s_timeboxGrid[i + 1].Ticks);
                    snappedTimebox = TimeSpan.FromTicks(snappedTicks);
                }
            }

            return snappedTimebox;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class QueryOperation
        {
            private readonly MongoDbThreadLogQueryService _queryService;
            private readonly ILogTimeRangeCriteria _input;
            private readonly TimeSpan _timeboxSize;
            private readonly ApplicationEntityService.QueryOptions _query;
            private readonly Dictionary<LogLevel, ChartData.TimeSeriesData> _seriesByLogLevel;
            private readonly ImmutableArray<DateTime> _timeboxGrid;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public QueryOperation(MongoDbThreadLogQueryService queryService, ILogTimeRangeCriteria input)
            {
                _queryService = queryService;
                _input = input;
                
                _query = (UIOperationContext.Current != null ? UIOperationContext.Current.Query : null);
                _timeboxSize = GetTimeboxSize(_input);
                _seriesByLogLevel = new Dictionary<LogLevel, ChartData.TimeSeriesData>() {
                    { LogLevel.Info, CreateChartSeries(_timeboxSize, "Info") },
                    { LogLevel.Warning, CreateChartSeries(_timeboxSize, "Warning") },
                    { LogLevel.Error, CreateChartSeries(_timeboxSize, "Error") },
                    { LogLevel.Critical, CreateChartSeries(_timeboxSize, "Critical") }
                };
                _timeboxGrid = BuildTimeboxGrid();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ChartData Execute()
            {
                var task = _queryService.QueryDailySummaryAsync(_input, _query, CancellationToken.None);
                task.Wait();

                BuildTimeboxGrid();
                AccumulateTimeboxValues(task.Result);

                return new ChartData() {
                    Series = _seriesByLogLevel.Values.Cast<ChartData.AbstractSeriesData>().ToList()
                };
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private void AccumulateTimeboxValues(IEnumerable<DailySummaryRecord> records)
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
                var timeboxGridStart = SnapPointToTimebox(_input.From, _timeboxSize);

                for (var timebox = timeboxGridStart ; timebox < _input.Until ; MoveToNextTimebox(ref timebox, _timeboxSize))
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
