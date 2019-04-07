using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.UI.Model.Impl.Metadata
{
    public class TextContentMetadata : UIComponentMetadata
    {
        public TextContentMetadata(MetadataObjectHeader header) : base(header)
        {
        }
        
        public string Text { get; set; }
    }
}