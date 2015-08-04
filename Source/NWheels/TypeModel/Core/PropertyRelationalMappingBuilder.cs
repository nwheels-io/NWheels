namespace NWheels.DataObjects.Core
{
    public class PropertyRelationalMappingBuilder : MetadataElement<IPropertyRelationalMapping>, IPropertyRelationalMapping
    {
        #region IMetadataElement Members

        public override string ReferenceName
        {
            get
            {
                return string.Format("{0}.{1}:{2}", TableName, ColumnName, ColumnType);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IPropertyRelationalMapping Members

        public IStorageDataType StorageType { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public string RelatedColumnName { get; set; }
        public string RelatedColumnType { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(ITypeMetadataVisitor visitor)
        {
            StorageType = visitor.VisitAttribute("StorageType", StorageType);
            TableName = visitor.VisitAttribute("TableName", TableName);
            ColumnName = visitor.VisitAttribute("ColumnName", ColumnName);
            ColumnType = visitor.VisitAttribute("ColumnType", ColumnType);
            RelatedColumnName = visitor.VisitAttribute("RelatedColumnName", RelatedColumnName);
            RelatedColumnType = visitor.VisitAttribute("RelatedColumnType", RelatedColumnType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return 
                (string.IsNullOrEmpty(TableName) ? "" : "TABLE(" + TableName + ").") +
                (string.IsNullOrEmpty(ColumnName) ? "" : "COLUMN(" + ColumnName + ")") +
                (string.IsNullOrEmpty(ColumnType) ? "" : ".TYPE(" + ColumnType + ")");
        }
    }
}
