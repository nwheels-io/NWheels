namespace NWheels.DataObjects
{
    public interface ITypeRelationalMapping
    {
        string PrimaryTableName { get; }
        RelationalInheritanceKind? InheritanceKind { get; }
    }
}
