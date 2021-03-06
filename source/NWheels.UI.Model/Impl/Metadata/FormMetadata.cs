using NWheels.Composition.Model;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.UI.Model.Impl.Metadata
{
    public class FormMetadata : UIComponentMetadata, IMetadataOf<IForm>
    {
        public FormMetadata(MetadataObjectHeader header) : base(header)
        {
        }
    }
}