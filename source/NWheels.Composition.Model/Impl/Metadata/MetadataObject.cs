namespace NWheels.Composition.Model.Impl.Metadata
{
    public interface IMetadataObject
    {
        MetadataObjectHeader Header { get; }
    }
    
    public abstract class MetadataObject : IMetadataObject
    {
        protected MetadataObject(MetadataObjectHeader header)
        {
            this.Header = header;
        }

        public MetadataObjectHeader Header { get; }
    }
}
