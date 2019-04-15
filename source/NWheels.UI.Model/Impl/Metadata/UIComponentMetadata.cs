using System.Collections.Generic;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.UI.Model.Impl.Metadata
{
    public abstract class UIComponentMetadata : MetadataObject
    {
        protected UIComponentMetadata(MetadataObjectHeader header) : base(header)
        {
        }
        
        public Dictionary<string, UIEventMetadata> EventByName { get; set; } = new Dictionary<string, UIEventMetadata>(); 
    }
}
