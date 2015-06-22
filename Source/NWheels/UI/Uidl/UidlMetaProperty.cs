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
            [DataMember]
            public RelationKind RelationKind { get; set; }
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
