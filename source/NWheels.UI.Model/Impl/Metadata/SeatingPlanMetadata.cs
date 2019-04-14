using System.Collections.Generic;
using System.Drawing;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.UI.Model.Impl.Metadata
{
    public class SeatingPlanMetadata : MetadataObject, IMetadataOf<SeatingPlan>
    {
        public SeatingPlanMetadata(MetadataObjectHeader header) : base(header)
        {
        }
        
        public Dictionary<object, Color> ColorBySeatType { get; set; } = new Dictionary<object,Color>();
    }
}