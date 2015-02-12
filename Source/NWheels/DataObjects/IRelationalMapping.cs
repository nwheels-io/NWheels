namespace NWheels.DataObjects
{
    public interface IRelationalMapping
    {
        string PrimaryTableName { get; }
        RelationalInheritanceKind? InheritanceKind { get; }
    }
}
