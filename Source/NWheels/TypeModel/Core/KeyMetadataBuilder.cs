using System.Collections.Generic;

namespace NWheels.DataObjects.Core
{
    public class KeyMetadataBuilder : MetadataElement<IKeyMetadata>, IKeyMetadata
    {
        private readonly ConcreteToAbstractCollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata> _propertiesAdapter;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public KeyMetadataBuilder()
        {
            this.Properties = new List<PropertyMetadataBuilder>();
            _propertiesAdapter = new ConcreteToAbstractCollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.Properties);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IMetadataElement Members

        public override string ReferenceName
        {
            get { return this.Name; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IKeyMetadata Members

        public string Name { get; set; }
        public KeyKind Kind { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<IPropertyMetadata> IKeyMetadata.Properties
        {
            get
            {
                return _propertiesAdapter;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<PropertyMetadataBuilder> Properties { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(ITypeMetadataVisitor visitor)
        {
            Name = visitor.VisitAttribute("Name", Name);
            Kind = visitor.VisitAttribute("Kind", Kind);

            visitor.VisitElementReferenceList<IPropertyMetadata, PropertyMetadataBuilder>("Properties", Properties);
        }
    }
}
