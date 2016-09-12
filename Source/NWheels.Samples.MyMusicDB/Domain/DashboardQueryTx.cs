using System;
using System.Collections.Generic;
using NWheels.DataObjects;
using NWheels.Domains.Security;
using NWheels.Processing;
using NWheels.UI;
using NWheels.UI.Factories;
using NWheels.UI.Toolbox;

namespace NWheels.Samples.MyMusicDB.Domain
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public class DashboardQueryTx : ITransactionScript<Empty.Context, Empty.Input, DashboardQueryTx.IOutput>
    {
        private readonly IViewModelObjectFactory _viewModelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DashboardQueryTx(IViewModelObjectFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ITransactionScript<Context,Input,Result>

        public Empty.Input InitializeInput(Empty.Context context)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IOutput Preview(Empty.Input input)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IOutput Execute(Empty.Input input)
        {
            var output = _viewModelFactory.NewEntity<IOutput>();

            output.Genres = 15;
            output.Artists = 2345;
            output.Albums = 34562;
            output.Tracks = 623542;
            output.Users = 1036843;
            output.ApiRequests = 76537263;

            var series = new ChartData.TimeSeriesData() {
                Label = "Requests",
                Type = ChartSeriesType.Bar,
                MillisecondStepSize = (int)TimeSpan.FromMinutes(30).TotalMilliseconds,
                Points = new List<ChartData.TimeSeriesPoint>()
            };

            var now = DateTime.Now;
            var t0 = new DateTime(now.Year, now.Month, now.Day, now.Hour, minute: 0, second: 0).AddDays(-2);
            var rnd = new Random();

            for (DateTime t = t0 ; t < now ; t = t.AddMinutes(30))
            {
                series.Points.Add(new ChartData.TimeSeriesPoint() {
                    UtcTimestamp = t,
                    Value = rnd.Next(1234, 3456)
                });
            }

            output.ApiRequestsOverTime = new ChartData() {
                Series = new List<ChartData.AbstractSeriesData>() {
                    series
                }
            };

            return output;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IOutput
        {
            int Genres { get; set; }
            int Artists { get; set; }
            int Albums { get; set; }
            int Tracks { get; set; }
            int Users { get; set; }
            int ApiRequests { get; set; }
            ChartData ApiRequestsOverTime { get; set; }
        }
    }
}