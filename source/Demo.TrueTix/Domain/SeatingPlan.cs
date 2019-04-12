using System.Collections.Generic;
using NWheels.Domain.Model;

namespace Demo.TrueTix.Domain
{
    [Entity, Aggregate]
    public class SeatingPlan
    {
        public int Id { get; set; }
        public Hall Hall { get; set; }
    }

    [Entity]    
    public class SeatingPlanRow
    {
        public int Id { get; set; }
        public SeatingPlan SeatingPlan { get; set; }
        public string Label { get; set; }
        public int Number { get; set; }
    }
}
