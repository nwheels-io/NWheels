using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.UI.Model.Impl.Metadata.Web
{
    public class DataGridMetadata : UIComponentMetadata, IMetadataOf<IDataGrid>
    {
        public DataGridMetadata(MetadataObjectHeader header) : base(header)
        {
        }
    }
}