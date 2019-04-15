using System.Collections.Generic;
using MetaPrograms.Expressions;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.UI.Model.Impl.Metadata
{
    public abstract class UIComponentMetadata : MetadataObject
    {
        protected UIComponentMetadata(MetadataObjectHeader header) : base(header)
        {
        }
        
        public int TopY { get; set; }
        
        public AbstractExpression MapStateToValue { get; set; }
        
        public Dictionary<string, UIEventMetadata> EventByName { get; set; } = new Dictionary<string, UIEventMetadata>(); 
    }
}
