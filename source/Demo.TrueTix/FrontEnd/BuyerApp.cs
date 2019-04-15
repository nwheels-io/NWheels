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
        [Include]
        SeatingPage Seating => new SeatingPage(performanceId: 12345);
    }

    public class SeatingPage : WebPage<Empty.Props, SeatingPage.PageState>
    {
        public SeatingPage(int performanceId)
            : base(new Empty.Props())
        {
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
        ISeatingApi SeatingApi => new BackendApiProxy<ISeatingApi>().Api;

        [Include]
        public override ILayoutComponent PageLayout => new GridLayout(new UIComponent[,] {
            {null, PerformanceInfo, null},
            {null, SeatPlan, null},
            {null, SeatInfo, null}
        });

        [Include] 
        SeatingPlan SeatPlan => new SeatingPlan(
            data: State.SeatingPlan.Rows,
            colorByType: new Dictionary<object, Color> {
                { SeatStatus.Sale, Color.Green },
                { SeatStatus.Resale, Color.Orange },
                { SeatStatus.Sold, Color.Red }
            }
        );

        [Include]
        TextContent PerformanceInfo => new TextContent(State.SeatingPlan.Performance);

        [Include] 
        TextContent SeatInfo => new TextContent(State.SelectedSeatInfo);

        public class PageState
        {
            public PerformanceSeatingPlan SeatingPlan { get; set; }
            public int? SelectedSeatId { get; set; }
            public PerformanceSeat SelectedSeatInfo { get; set; }
        }
    }
}
