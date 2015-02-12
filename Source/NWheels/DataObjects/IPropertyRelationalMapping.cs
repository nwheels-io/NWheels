namespace NWheels.DataObjects
{
    public interface IPropertyRelationalMapping
    {
        string TableName { get; }
        string ColumnName { get; }
        string DataTypeName { get; }
    }
}
