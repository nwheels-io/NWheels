using System.Collections.Generic;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.UI.Model.Impl.Metadata.Web
{
    public class WebAppMetadata : MetadataObject
    {
        public WebAppMetadata(MetadataObjectHeader header) 
            : base(header)
        {
        }
        
        public string Title { get; set; }
        public List<WebPageMetadata> Pages { get; } = new List<WebPageMetadata>();
    }
}
