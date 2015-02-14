using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Core.DataObjects
{
    public class KeyMetadataBuilder : MetadataElement<IKeyMetadata>, IKeyMetadata
    {
        private readonly CollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata> _propertiesAdapter;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public KeyMetadataBuilder()
        {
            this.Properties = new List<PropertyMetadataBuilder>();
            _propertiesAdapter = new CollectionAdapter<PropertyMetadataBuilder, IPropertyMetadata>(this.Properties);
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
        public KeyKind Kind{ get; set; }

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

        public override void AcceptVisitor(IMetadataElementVisitor visitor)
        {
            Name = visitor.VisitAttribute("Name", Name);
            Kind = visitor.VisitAttribute("Kind", Kind);

            visitor.VisitElementReferenceList<IPropertyMetadata, PropertyMetadataBuilder>("Properties", Properties);
        }
    }
}
