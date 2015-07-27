namespace NWheels.DataObjects
{
    public interface IRelationMetadata : IMetadataElement
    {
        RelationKind Kind { get; }
        RelationMultiplicity Multiplicity { get; }
        RelationPartyKind ThisPartyKind { get; }
        IKeyMetadata ThisPartyKey { get; }
        ITypeMetadata RelatedPartyType { get; }
        RelationPartyKind RelatedPartyKind { get; }
        IKeyMetadata RelatedPartyKey { get; }
        IPropertyMetadata InverseProperty { get; }
    }
}
