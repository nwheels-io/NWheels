using NWheels.TypeModel;

namespace NWheels.DataObjects
{
    public interface IPropertyRelationalMapping : IMetadataElement
    {
        IStorageDataType StorageType { get; }
        PropertyStorageStyle StorageStyle { get; }
        bool IsEmbeddedInParent { get; }
        bool IsForeignKeyEmbeddedInParent { get; }
        string TableName { get; }
        string ColumnName { get; }
        string ColumnType { get; }
        string RelatedColumnName { get; }
        string RelatedColumnType { get; }
    }
}
