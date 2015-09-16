using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "MetaProperty", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlMetaProperty
    {
        public UidlMetaProperty(IPropertyMetadata metadata)
        {
            this.Name = metadata.Name;
            this.Kind = metadata.Kind;
            this.Role = metadata.Role;
            this.DataType = metadata.ClrType.Name;
            this.SemanticType = (metadata.SemanticType != null ? metadata.SemanticType.Name : null);
            this.AccessFlags = metadata.Access.ToString();
            this.IsSensitive = metadata.IsSensitive;
            this.IsCalculated = metadata.IsCalculated;
            this.Relation = metadata.Relation != null ? new MetaRelation(metadata.Relation) : null;
            this.Validation = metadata.Validation != null ? new MetaValidation(metadata.Validation) : null;
            this.DefaultValue = metadata.DefaultValue;
            this.DefaultDisplayName = metadata.DefaultDisplayName;
            this.DefaultDisplayFormat = metadata.DefaultDisplayFormat;
            this.DefaultSortAscending = metadata.DefaultSortAscending;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public PropertyKind Kind { get; set; }
        [DataMember]
        public PropertyRole Role { get; set; }
        [DataMember]
        public string DataType { get; set; }
        [DataMember]
        public string SemanticType { get; set; }
        [DataMember]
        public string AccessFlags { get; set; }
        [DataMember]
        public bool IsSensitive { get; set; }
        [DataMember]
        public bool IsCalculated { get; set; }
        [DataMember]
        public MetaRelation Relation { get; set; }
        [DataMember]
        public MetaValidation Validation { get; set; }
        [DataMember]
        public object DefaultValue { get; set; }
        [DataMember]
        public string DefaultDisplayName { get; set; }
        [DataMember]
        public string DefaultDisplayFormat { get; set; }
        [DataMember]
        public bool DefaultSortAscending { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Name = "MetaValidation", Namespace = UidlDocument.DataContractNamespace)]
        public class MetaValidation
        {
            public MetaValidation(IPropertyValidationMetadata metadata)
            {
                this.IsRequired = metadata.IsRequired;
                this.IsUnique = metadata.IsUnique;
                this.IsEmptyAllowed = metadata.IsEmptyAllowed;
                this.MinLength = metadata.MinLength;
                this.MaxLength = metadata.MaxLength;
                this.MinValue = metadata.MinValue;
                this.MaxValue = metadata.MaxValue;
                this.MinValueExclusive = metadata.MinValueExclusive;
                this.MaxValueExclusive = metadata.MaxValueExclusive;
                this.RegularExpression = metadata.RegularExpression;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public bool IsRequired { get; set; }
            [DataMember]
            public bool IsUnique { get; set; }
            [DataMember]
            public bool IsEmptyAllowed { get; set; }
            [DataMember]
            public int? MinLength { get; set; }
            [DataMember]
            public int? MaxLength { get; set; }
            [DataMember]
            public object MinValue { get; set; }
            [DataMember]
            public object MaxValue { get; set; }
            [DataMember]
            public bool MinValueExclusive { get; set; }
            [DataMember]
            public bool MaxValueExclusive { get; set; }
            [DataMember]
            public string RegularExpression { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Name = "MetaRelation", Namespace = UidlDocument.DataContractNamespace)]
        public class MetaRelation
        {
            public MetaRelation(IRelationMetadata metadata)
            {
                this.Kind = metadata.Kind;
                this.Multiplicity = metadata.Multiplicity;
                this.ThisPartyKind = metadata.ThisPartyKind;
                this.ThisPartyKeyName = metadata.ThisPartyKey != null ? metadata.ThisPartyKey.Name : null;
                this.RelatedPartyTypeName = metadata.RelatedPartyType != null ? metadata.RelatedPartyType.Name : null;
                this.RelatedPartyKind = metadata.RelatedPartyKind;
                this.RelatedPartyKeyName = metadata.RelatedPartyKey != null ? metadata.RelatedPartyKey.Name : null;
                this.InversePropertyName = metadata.InverseProperty != null ? metadata.InverseProperty.Name : null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public RelationKind Kind { get; set; }
            [DataMember]
            public RelationMultiplicity Multiplicity { get; set; }
            [DataMember]
            public RelationPartyKind ThisPartyKind { get; set; }
            [DataMember]
            public string ThisPartyKeyName { get; set; }
            [DataMember]
            public string RelatedPartyTypeName { get; set; }
            [DataMember]
            public RelationPartyKind RelatedPartyKind { get; set; }
            [DataMember]
            public string RelatedPartyKeyName { get; set; }
            [DataMember]
            public string InversePropertyName { get; set; }
        }
    }
}
