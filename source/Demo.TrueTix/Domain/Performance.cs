using System;
using NWheels.Domain.Model;

namespace Demo.TrueTix.Domain
{
    [Entity, Aggregate]
    public class Performance
    {
        public int Id { get; set; }
        public Show Show { get; set; }
        public DateTime Date { get; set; }
        public Hall Hall { get; set; }
        public PerformanceSeatingPlan SeatingPlan { get; set; }
    }
}
