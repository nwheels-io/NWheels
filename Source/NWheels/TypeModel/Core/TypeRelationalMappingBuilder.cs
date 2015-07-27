namespace NWheels.DataObjects.Core
{
    public class TypeRelationalMappingBuilder : MetadataElement<ITypeRelationalMapping>, ITypeRelationalMapping
    {
        #region IMetadataElement Members

        public override string ReferenceName
        {
            get { return this.PrimaryTableName; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region ITypeRelationalMapping Members

        public string PrimaryTableName { get; set; }
        public RelationalInheritanceKind? InheritanceKind { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(ITypeMetadataVisitor visitor)
        {
            PrimaryTableName = visitor.VisitAttribute("PrimaryTableName", PrimaryTableName);
            InheritanceKind = visitor.VisitAttribute("InheritanceKind", InheritanceKind);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return "TABLE(" + PrimaryTableName + ")";
        }
    }
}
