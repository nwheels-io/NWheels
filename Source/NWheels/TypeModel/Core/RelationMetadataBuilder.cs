namespace NWheels.DataObjects.Core
{
    public class RelationMetadataBuilder : MetadataElement<IRelationMetadata>, IRelationMetadata
    {
        #region IMetadataElement Members

        public override string ReferenceName
        {
            get { return RelatedPartyType.Name; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IRelationMetadata Members

        public RelationKind Kind { get; set; }
        public RelationPartyKind ThisPartyKind { get; set; }
        public RelationPartyKind RelatedPartyKind { get; set; }

        private RelationMultiplicity _multiplicity;
        public RelationMultiplicity Multiplicity
        {
            get
            {
                return _multiplicity;
            }
            set
            {
                _multiplicity = value;
            }
        }




        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IKeyMetadata IRelationMetadata.ThisPartyKey
        {
            get { return this.ThisPartyKey; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypeMetadata IRelationMetadata.RelatedPartyType
        {
            get { return this.RelatedPartyType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IKeyMetadata IRelationMetadata.RelatedPartyKey
        {
            get { return this.RelatedPartyKey; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPropertyMetadata IRelationMetadata.InverseProperty
        {
            get { return this.InverseProperty; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public KeyMetadataBuilder ThisPartyKey { get; set; }
        public TypeMetadataBuilder RelatedPartyType { get; set; }
        public KeyMetadataBuilder RelatedPartyKey { get; set; }
        public PropertyMetadataBuilder InverseProperty { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(ITypeMetadataVisitor visitor)
        {
            Kind = visitor.VisitAttribute("Kind", Kind);
            Multiplicity = visitor.VisitAttribute("RelationKind", Multiplicity);
            ThisPartyKind = visitor.VisitAttribute("ThisPartyKind", ThisPartyKind);
            ThisPartyKey = visitor.VisitElementReference<IKeyMetadata, KeyMetadataBuilder>("ThisPartyKey", ThisPartyKey);

            RelatedPartyType = visitor.VisitElementReference<ITypeMetadata, TypeMetadataBuilder>("RelatedPartyType", RelatedPartyType);
            RelatedPartyKind = visitor.VisitAttribute("RelatedPartyKind", RelatedPartyKind);
            RelatedPartyKey = visitor.VisitElementReference<IKeyMetadata, KeyMetadataBuilder>("RelatedPartyKey", RelatedPartyKey);

            InverseProperty = visitor.VisitElementReference<IPropertyMetadata, PropertyMetadataBuilder>("InverseProperty", InverseProperty);
        }
    }
}
