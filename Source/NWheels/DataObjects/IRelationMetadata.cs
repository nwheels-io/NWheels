namespace NWheels.DataObjects
{
    public interface IRelationMetadata
    {
        RelationKind RelationKind { get; }
        RelationPartyKind ThisPartyKind { get; }
        IKeyMetadata ThisPartyKey { get; }
        ITypeMetadata RelatedPartyType { get; }
        RelationPartyKind RelatedPartyKind { get; }
        IKeyMetadata RelatedPartyKey { get; }
    }
}
