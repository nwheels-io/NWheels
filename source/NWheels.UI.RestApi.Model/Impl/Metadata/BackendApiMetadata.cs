using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata;

namespace NWheels.UI.RestApi.Model.Impl.Metadata
{
    public class BackendApiMetadata : MetadataObject, IBackendApiMetadata, IMetadataOf<AnyBackendApiProxy>
    {
        public BackendApiMetadata(MetadataObjectHeader header) : base(header)
        {
        }

        public string Url { get; set; }
        public string Protocol { get; set; } = "GraphQL";
    }
}
