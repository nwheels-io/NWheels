namespace NWheels.DataObjects
{
    public interface IPropertyRelationalMapping : IMetadataElement
    {
        IStorageDataType StorageType { get; }
        string TableName { get; }
        string ColumnName { get; }
        string ColumnType { get; }
    }
}
