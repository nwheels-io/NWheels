using NWheels.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public abstract class UidlMetaType
    {
        protected UidlMetaType(Type type)
        {
            this.TypeName = type.AssemblyQualifiedNameNonVersioned();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string TypeName { get; set; }
        [DataMember]
        public UidlMetaTypeKind TypeKind { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Name = "KeyMetaType", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlKeyMetaType : UidlMetaType
    {
        public UidlKeyMetaType(Type type)
            : base(type)
        {
            base.TypeKind = UidlMetaTypeKind.Key;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityTypeName { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Name = "ValueMetaType", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlValueMetaType : UidlMetaType
    {
        public UidlValueMetaType(Type type)
            : base(type)
        {
            base.TypeKind = UidlMetaTypeKind.Value;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Name = "ObjectMetaType", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlObjectMetaType : UidlMetaType
    {
        public UidlObjectMetaType(Type type, ITypeMetadata metadata)
            : base(type)
        {
            base.TypeKind = UidlMetaTypeKind.Object;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public bool BaseTypeName { get; set; }
        [DataMember]
        public List<string> DerivedTypeNames { get; set; }
        [DataMember]
        public Dictionary<string, UidlMetaProperty> Properties { get; set; }
        [DataMember]
        public MetaKey PrimaryKey { get; set; }
        [DataMember]
        public Dictionary<string, MetaKey> AllKeys { get; set; }
        [DataMember]
        public string DefaultDisplayFormat { get; set; }
        [DataMember]
        public List<string> DefaultDisplayPropertyNames { get; set; }
        [DataMember]
        public List<string> DefaultSortPropertyNames { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Name = "MetaKey", Namespace = UidlDocument.DataContractNamespace)]
        public class MetaKey
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public KeyKind Kind { get; set; }
            [DataMember]
            public List<string> PropertyNames { get; set; }
        }
    }
}
