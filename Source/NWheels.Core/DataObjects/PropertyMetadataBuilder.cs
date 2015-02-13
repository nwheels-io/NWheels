using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Core.DataObjects
{
    public class PropertyMetadataBuilder : MetadataElement<IPropertyMetadata>, IPropertyMetadata
    {
        #region IPropertyMetadata Members

        public string Name { get; set; }
        public PropertyKind Kind { get; set; }
        public Type ClrType { get; set; }
        public System.ComponentModel.DataAnnotations.DataType SemanticDataType { get; set; }
        public System.Reflection.PropertyInfo ContractPropertyInfo { get; set; }
        public System.Reflection.PropertyInfo ImplementationPropertyInfo { get; set; }
        public string DefaultDisplayName { get; set; }
        public string DefaultDisplayFormat { get; set; }
        public bool DefaultSortAscending { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IRelationMetadata IPropertyMetadata.Relation
        {
            get { return this.Relation; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPropertyValidationMetadata IPropertyMetadata.Validation
        {
            get { return this.Validation; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPropertyRelationalMapping IPropertyMetadata.RelationalMapping
        {
            get { return this.RelationalMapping; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public PropertyRelationalMappingBuilder RelationalMapping { get; set; }
        public RelationMetadataBuilder Relation { get; set; }
        public PropertyValidationMetadataBuilder Validation { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(IMetadataElementVisitor visitor)
        {
            Name = visitor.VisitAttribute("Name", Name);
            Kind = visitor.VisitAttribute("Kind", Kind);
            ClrType = visitor.VisitAttribute("ClrType", ClrType);
            SemanticDataType = visitor.VisitAttribute("SemanticDataType", SemanticDataType);
            ContractPropertyInfo = visitor.VisitAttribute("ContractPropertyInfo", ContractPropertyInfo);
            ImplementationPropertyInfo = visitor.VisitAttribute("ImplementationPropertyInfo", ImplementationPropertyInfo);
            DefaultDisplayName = visitor.VisitAttribute("DefaultDisplayName", DefaultDisplayName);
            DefaultDisplayFormat = visitor.VisitAttribute("DefaultDisplayFormat", DefaultDisplayFormat);
            DefaultSortAscending = visitor.VisitAttribute("DefaultSortAscending", DefaultSortAscending);

            Relation = visitor.VisitElement<IRelationMetadata, RelationMetadataBuilder>(Relation);
            Validation = visitor.VisitElement<IPropertyValidationMetadata, PropertyValidationMetadataBuilder>(Validation);
            RelationalMapping = visitor.VisitElement<IPropertyRelationalMapping, PropertyRelationalMappingBuilder>(RelationalMapping);
        }
    }
}
