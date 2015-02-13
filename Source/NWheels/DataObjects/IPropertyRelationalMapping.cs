namespace NWheels.DataObjects
{
    public interface IPropertyRelationalMapping : IMetadataElement
    {
        string TableName { get; }
        string ColumnName { get; }
        string DataTypeName { get; }
    }
}
