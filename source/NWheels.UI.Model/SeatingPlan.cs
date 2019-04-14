using System;
using System.Collections.Generic;
using System.Drawing;
using MetaPrograms.Members;
using NWheels.Composition.Model;

namespace NWheels.UI.Model
{
    public class SeatingPlanRow
    {
        public List<SeatingPlanSeat> Seats { get; set; }
    }

    public class SeatingPlanSeat
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string SeatLabel { get; set; }
        public string RowLabel { get; set; }
        public string Description { get; set; }
    }

    public class SeatingPlan : UIComponent<Empty.Props, Empty.State>
    {
        public SeatingPlan(
            IEnumerable<SeatingPlanRow> rows,
            Dictionary<object, Color> colorByType)
        {
        }
        
        public Event<SeatingPlanSeat> SeatSelected { get; set; }= new Event<SeatingPlanSeat>();
    }
}
