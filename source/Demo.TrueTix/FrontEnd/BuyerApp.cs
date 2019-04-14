using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Demo.TrueTix.Api;
using Demo.TrueTix.Domain;
using NWheels.Composition.Model;
using NWheels.UI.Model;
using NWheels.UI.Model.Web;
using NWheels.UI.RestApi.Model;
using SeatingPlan = NWheels.UI.Model.SeatingPlan;
using SeatingPlanRow = NWheels.UI.Model.SeatingPlanRow;

namespace Demo.TrueTix.FrontEnd
{
    public class BuyerApp : WebApp
    {
        private readonly string _backendUrl;

        public BuyerApp(string backendUrl)
        {
            _backendUrl = backendUrl;    
        }

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
            
            SeatPlan.SeatSelected += async (seat) => {
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
            {null, SeatPlan, null},
            {null, SeatInfo, null}
        });

        [Include] 
        SeatingPlan SeatPlan => new SeatingPlan(
            rows: State.SeatingPlan.Rows.Select((r, rowIndex) => new SeatingPlanRow {
                Seats = r.Seats.Select((s, seatIndex) => new SeatingPlanSeat() {
                   Id = s.Id,
                   Type = s.Status.ToString(),
                   RowLabel = (rowIndex + 1).ToString(),
                   SeatLabel = (seatIndex + 1).ToString()
                }).ToList()
            }),
            colorByType: new Dictionary<object, Color> {
                { SeatStatus.Sale, Color.Green },
                { SeatStatus.Resale, Color.Orange },
                { SeatStatus.Sold, Color.Red }
            }
        );

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
