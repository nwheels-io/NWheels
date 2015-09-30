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
        public UidlObjectMetaType(Type type, ITypeMetadata metadata, HashSet<Type> relatedTypes)
            : base(type)
        {
            base.TypeKind = UidlMetaTypeKind.Object;
            this.BaseTypeName = metadata.BaseType != null ? metadata.BaseType.Name : null;
            this.DerivedTypeNames = metadata.DerivedTypes.Select(t => t.Name).ToList();
            this.Properties = metadata.Properties.ToDictionary(p => p.Name, p => new UidlMetaProperty(p, relatedTypes));
            this.PrimaryKey = metadata.PrimaryKey != null ? new MetaKey(metadata.PrimaryKey) : null;
            this.AllKeys = metadata.AllKeys.ToDictionary(k => k.Name, k => new MetaKey(k));
            this.DefaultDisplayFormat = metadata.DefaultDisplayFormat;
            this.DefaultDisplayPropertyNames = metadata.DefaultDisplayProperties.Select(p => p.Name).ToList();
            this.DefaultSortPropertyNames = metadata.DefaultSortProperties.Select(p => p.Name).ToList();

            SetRestType(metadata);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string BaseTypeName { get; set; }
        [DataMember]
        public List<string> DerivedTypeNames { get; set; }
        [DataMember]
        public string RestTypeNamespace { get; set; }
        [DataMember]
        public string RestTypeName { get; set; }
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

        private void SetRestType(ITypeMetadata metadata)
        {
            var implementations = metadata.GetAllImplementations().ToArray();

            if ( implementations.Any() )
            {
                var implementationType = implementations.Single().Value; //TODO: get rid of this limitation - introduce REST entity factory

                this.RestTypeName = implementationType.Name;
                this.RestTypeNamespace = implementationType.Namespace;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Name = "MetaKey", Namespace = UidlDocument.DataContractNamespace)]
        public class MetaKey
        {
            public MetaKey(IKeyMetadata metadata)
            {
                this.Name = metadata.Name;
                this.Kind = metadata.Kind;
                this.PropertyNames = metadata.Properties.Select(p => p.Name).ToList();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public KeyKind Kind { get; set; }
            [DataMember]
            public List<string> PropertyNames { get; set; }
        }
    }
}
