using System;
using System.Collections.Generic;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Processing;
using NWheels.UI;
using NWheels.UI.Toolbox;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public class LogLevelSummaryChartTx : AbstractLogLevelSummaryChartTx
    {
        #region Overrides of TransactionScript<Input,ILogTimeRangeCriteria,ChartData>

        public override ChartData Execute(ILogTimeRangeCriteria input)
        {
            var today = DateTime.UtcNow.Date;

            return new ChartData() {
                Series = new List<ChartData.AbstractSeriesData>() {
                    new ChartData.TimeSeriesData() {
                        Type = ChartSeriesType.Bar,
                        MillisecondStepSize = (long)TimeSpan.FromHours(1).TotalMilliseconds,
                        Label = "Info",
                        Points = new List<ChartData.TimeSeriesPoint>() {
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(0), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(2), Value = 20 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(3), Value = 30 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(4), Value = 20 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(5), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(6), Value = 40 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(7), Value = 60 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(8), Value = 80 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(9), Value = 120 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(10), Value = 20 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(11), Value = 100 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(12), Value = 200 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(13), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(14), Value = 20 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(15), Value = 30 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(16), Value = 40 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(17), Value = 30 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(18), Value = 20 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(19), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(20), Value = 50 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(21), Value = 100 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(22), Value = 150 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(23), Value = 80 },
                        }
                    },
                    new ChartData.TimeSeriesData() {
                        Type = ChartSeriesType.Bar,
                        Label = "Warning",
                        MillisecondStepSize = (long)TimeSpan.FromHours(1).TotalMilliseconds,
                        Points = new List<ChartData.TimeSeriesPoint>() {
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(0), Value = 5 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(2), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(3), Value = 15 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(4), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(5), Value = 5 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(6), Value = 20 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(7), Value = 30 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(8), Value = 40 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(9), Value = 60 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(10), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(11), Value = 50 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(12), Value = 100 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(13), Value = 5 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(14), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(15), Value = 15 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(16), Value = 20 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(17), Value = 15 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(18), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(19), Value = 5 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(20), Value = 25 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(21), Value = 50 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(22), Value = 75 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(23), Value = 40 },
                        }
                    },
                    new ChartData.TimeSeriesData() {
                        Type = ChartSeriesType.Bar,
                        Label = "Error",
                        MillisecondStepSize = (long)TimeSpan.FromHours(1).TotalMilliseconds,
                        Points = new List<ChartData.TimeSeriesPoint>() {
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(0), Value = 2 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(2), Value = 5 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(3), Value = 7 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(4), Value = 5 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(5), Value = 3 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(6), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(7), Value = 15 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(8), Value = 20 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(9), Value = 30 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(10), Value = 5 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(11), Value = 25 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(12), Value = 50 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(13), Value = 2 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(14), Value = 5 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(15), Value = 7 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(16), Value = 10 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(17), Value = 8 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(18), Value = 5 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(19), Value = 3 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(20), Value = 12 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(21), Value = 25 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(22), Value = 37 },
                            new ChartData.TimeSeriesPoint() { UtcTimestamp = today.AddHours(23), Value = 20 },
                        }
                    }
                }
            };
        }

        #endregion
    }
}
