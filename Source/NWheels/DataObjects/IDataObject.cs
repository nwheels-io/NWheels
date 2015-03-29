namespace NWheels.DataObjects
{
    public interface IDataObject
    {
        ITypeMetadata GetTypeMetadata();
        IDataObjectKey GetKey();
    }
}
