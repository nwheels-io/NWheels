using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.UI.RestApi.Model.Impl.Metadata
{
    public class BackendApiMetadata : MetadataObject, IMetadataOf<AnyBackendApiProxy>
    {
        public BackendApiMetadata(MetadataObjectHeader header) : base(header)
        {
        }
    }
}