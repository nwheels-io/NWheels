using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Extensions;
using NWheels.UI.Toolbox;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    public class VisualizationHelpers
    {
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

        public static ChartData.TimeSeriesData CreateChartSeries(TimeSpan timeboxSize, string label)
        {
            return new ChartData.TimeSeriesData() {
                Type = ChartSeriesType.Bar,
                MillisecondStepSize = (long)timeboxSize.TotalMilliseconds,
                Label = label,
                Points = new List<ChartData.TimeSeriesPoint>()
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void MoveToNextTimebox(ref DateTime value, TimeSpan timeboxSize)
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

        public static DateTime SnapPointToTimebox(DateTime point, TimeSpan timeboxSize)
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

        public static TimeSpan GetTimeboxSize(ILogTimeRangeCriteria timeRange)
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
    }
}