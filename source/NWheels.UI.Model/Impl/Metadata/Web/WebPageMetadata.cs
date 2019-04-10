using System.Collections.Generic;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.UI.Model.Impl.Metadata.Web
{
    public class WebPageMetadata : MetadataObject
    {
        public WebPageMetadata(MetadataObjectHeader header) : base(header)
        {
        }
        
        public string Title { get; set; }
        public List<UIComponentMetadata> Components { get; } = new List<UIComponentMetadata>();
        public List<IBackendApiMetadata> BackendApis { get; } = new List<IBackendApiMetadata>();
    }
}
