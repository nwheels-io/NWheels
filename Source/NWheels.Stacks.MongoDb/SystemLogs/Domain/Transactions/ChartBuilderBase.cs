using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;
using NWheels.UI;
using NWheels.UI.Toolbox;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    public abstract class ChartBuilderBase<TRecord, TDiscriminator>
        where TRecord : LogRecordBase
    {
        private readonly ILogTimeRangeCriteria _input;
        private readonly TimeSpan _timeboxSize;
        private readonly ApplicationEntityService.QueryOptions _query;
        private readonly ImmutableArray<DateTime> _timeboxGrid;
        private Dictionary<TDiscriminator, ChartData.TimeSeriesData> _seriesByDiesciminator;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected ChartBuilderBase(ILogTimeRangeCriteria input)
        {
            _input = input;
            _query = (UIOperationContext.Current != null ? UIOperationContext.Current.Query : null);
            
            _timeboxSize = VisualizationHelpers.GetTimeboxSize(_input);
            _timeboxGrid = BuildTimeboxGrid();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddRecords(IEnumerable<TRecord> records)
        {
            EnsureSeries();

            foreach (var record in records)
            {
                Accumulate(record);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ChartData GetChart()
        {
            EnsureSeries();

            return new ChartData() {
                Series = _seriesByDiesciminator.Values.Cast<ChartData.AbstractSeriesData>().ToList(),
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual IEnumerable<TDiscriminator> GetSeries()
        {
            yield return (TDiscriminator)(object)SummaryLogLevel.Positive;
            yield return (TDiscriminator)(object)SummaryLogLevel.Negative;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void Accumulate(TRecord record);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected void IncrementSeriesTimebox(TDiscriminator discriminator, DateTime timestamp, int count)
        {
            ChartData.TimeSeriesData series;

            if (_seriesByDiesciminator.TryGetValue(discriminator, out series))
            {
                var timeboxIndex = TryGetTimeboxIndex(timestamp);

                if (timeboxIndex >= 0)
                {
                    series.Points[timeboxIndex].Value += count;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ILogTimeRangeCriteria Input
        {
            get { return _input; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TimeSpan TimeboxSize
        {
            get { return _timeboxSize; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ApplicationEntityService.QueryOptions Query
        {
            get { return _query; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ImmutableArray<DateTime> TimeboxGrid
        {
            get { return _timeboxGrid; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Dictionary<TDiscriminator, ChartData.TimeSeriesData> SeriesByDiscriminator
        {
            get { return _seriesByDiesciminator; }
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
            }

            return timeboxGridBuilder.ToImmutable();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private int TryGetTimeboxIndex(DateTime timestamp)
        {
            var timeboxIndex = TimeboxGrid.BinarySearch(timestamp);

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

        private void EnsureSeries()
        {
            if (_seriesByDiesciminator == null)
            {
                var discriminatorIndex = GetSeries().ToArray();

                _seriesByDiesciminator = discriminatorIndex.ToDictionary(
                    discriminator => discriminator,
                    discriminator => VisualizationHelpers.CreateChartSeries(_timeboxSize, label: discriminator.ToString()));

                foreach (var discriminator in discriminatorIndex)
                {
                    var series = _seriesByDiesciminator[discriminator];

                    for (int i = 0 ; i < _timeboxGrid.Length ; i++)
                    {
                        series.Points.Add(new ChartData.TimeSeriesPoint() {
                            UtcTimestamp = _timeboxGrid[i],
                            Value = 0
                        });
                    }
                }
            }
        }
    }
}
