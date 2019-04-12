using System;
using System.Collections.Generic;
using System.Drawing;
using MetaPrograms.Members;
using NWheels.Composition.Model;

namespace NWheels.UI.Model
{
    public class SeatingMapData
    {
        public List<List<SeatingMapSeatData>> SeatsInRows { get; set; }
    }

    public class SeatingMapSeatData
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
    }

    public class SeatingMap : UIComponent<Empty.Props, Empty.State>
    {
        public SeatingMap(SeatingMapData data)
        {
        }
        
        public Event<SeatingMapSeatData> SeatSelected { get; set; }= new Event<SeatingMapSeatData>();
    }
}
