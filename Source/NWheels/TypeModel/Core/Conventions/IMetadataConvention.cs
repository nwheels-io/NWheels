namespace NWheels.DataObjects.Core.Conventions
{
    public interface IMetadataConvention
    {
        void InjectCache(TypeMetadataCache cache);
        void Preview(TypeMetadataBuilder type);
        void Apply(TypeMetadataBuilder type);
        void Finalize(TypeMetadataBuilder type);
    }
}
