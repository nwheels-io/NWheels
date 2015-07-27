namespace NWheels.DataObjects
{
    public interface ITypeRelationalMapping : IMetadataElement
    {
        string PrimaryTableName { get; }
        RelationalInheritanceKind? InheritanceKind { get; }
    }
}
