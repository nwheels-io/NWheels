namespace NWheels.Composition.Model.Impl.Metadata
{
    public interface IMetadataObject
    {
        MetadataObjectHeader Header { get; } 
    }
    
    public abstract class MetadataObject : IMetadataObject
    {
        private readonly MetadataObjectHeader _header;

        protected MetadataObject(MetadataObjectHeader header)
        {
            _header = header;
        }

        MetadataObjectHeader IMetadataObject.Header => _header;

        public static MetadataObjectHeader Header(IMetadataObject obj) => obj.Header;
    }
}
