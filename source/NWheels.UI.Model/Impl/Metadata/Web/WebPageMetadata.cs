using System.Collections.Generic;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.UI.Model.Impl.Metadata.Web
{
    public class WebPageMetadata : MetadataObject
    {
        public WebPageMetadata(MetadataObjectHeader header) : base(header)
        {
        }
        
        public List<UIComponentMetadata> Components { get; } = new List<UIComponentMetadata>();
    }
}