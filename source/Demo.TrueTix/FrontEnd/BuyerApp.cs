using System.Linq;
using Demo.TrueTix.Api;
using Demo.TrueTix.Domain;
using NWheels.Composition.Model;
using NWheels.UI.Model;
using NWheels.UI.Model.Web;
using NWheels.UI.RestApi.Model;
using SeatingMap = NWheels.UI.Model.SeatingMap;

namespace Demo.TrueTix.FrontEnd
{
    public class BuyerApp : WebApp
    {
        private readonly string _backendUrl;

        public BuyerApp(string backendUrl) => _backendUrl = backendUrl;

        [Include]
        SeatingPage Seating => new SeatingPage(performanceId: 12345, backendUrl: _backendUrl);
    }

    public class SeatingPage : WebPage<Empty.Props, SeatingPage.PageState>
    {
        private readonly string _backendUrl;

        public SeatingPage(int performanceId, string backendUrl)
            : base(new Empty.Props())
        {
            _backendUrl = backendUrl;
            
            PageReady += async _ => {
                State.SeatingPlan = await SeatingApi.GetSeatingMap(performanceId);
            };
            
            SeatMap.SeatSelected += async (seat) => {
                State.SelectedSeatId = seat.Id;
                State.SelectedSeatInfo = null;
                State.SelectedSeatInfo = await SeatingApi.GetSeat(performanceId, seat.Id);
            };
        }

        public override string Title => "Seats";

        [Include]
        ISeatingApi SeatingApi => new BackendApiProxy<ISeatingApi>(_backendUrl).Api;

        [Include]
        public override ILayoutComponent PageLayout => new GridLayout(new UIComponent[,] {
            {null, PerformanceInfo, null},
            {null, SeatMap, null},
            {null, SeatInfo, null}
        });

        [Include] 
        SeatingMap SeatMap => new SeatingMap(new SeatingMapData {
            SeatsInRows = State.SeatingPlan.Rows
                .Select(row => row.Seats
                    .Select(seat => new SeatingMapSeatData {
                        //
                    }).ToList()
                ).ToList()
        });

        [Include]
        TextContent PerformanceInfo => new TextContent(FormatPerformance(State.SeatingPlan.Performance)); 
        
        [Include]
        TextContent SeatInfo => new TextContent(FormatSeat(State.SelectedSeatInfo));

        string FormatPerformance(Performance perf) => $"{perf}";

        string FormatSeat(PerformanceSeat seat) => $"{seat}";

        public class PageState
        {
            public PerformanceSeatingPlan SeatingPlan { get; set; }
            public int? SelectedSeatId { get; set; }
            public PerformanceSeat SelectedSeatInfo { get; set; }
        }
    }
}
