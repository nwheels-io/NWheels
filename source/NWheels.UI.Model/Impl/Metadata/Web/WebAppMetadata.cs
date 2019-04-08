using System.Collections.Generic;
using MetaPrograms;
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
        public List<PageItem> Pages { get; } = new List<PageItem>();

        public class PageItem
        {
            public IdentifierName Name { get; set; }
            public WebPageMetadata Metadata { get; set; }
            public bool IsIndex { get; set; }
        }
    }
}
