using System.Collections.Generic;
using NWheels.Domain.Model;

namespace Demo.TrueTix.Domain
{
    [ValueObject]
    public class PerformanceSeatingPlan
    {
        public Performance Performance { get; set; }
        public List<PerformanceSeatingRow> Rows { get; set; }
    }

    [ValueObject]
    public class PerformanceSeatingRow
    {
        public SeatingPlanRow BaseRow { get; set; }
        public List<PerformanceSeat> Seats { get; set; }
    }

    [ValueObject]
    public class PerformanceSeat
    {
        public int Id { get; }
        public Seat BaseSeat { get; set; }
        public SeatStatus Status { get; set; }
        public SecondarySale Resale { get; set; }
    }
}
