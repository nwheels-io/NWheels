using System;
using System.Linq;
using System.Reflection;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;
using TT2 = NWheels.Entities.Factories.DomainObjectFactory.TemplateTypes;

namespace NWheels.Entities.Factories
{
    public class DomainObjectFactoryContext
    {
        public DomainObjectFactoryContext(
            ObjectFactoryContext baseContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            Type persistableFactoryType,
            PropertyImplementationStrategyMap propertyMap)
        {
            this.BaseContext = baseContext;
            this.MetadataCache = metadataCache;
            this.MetaType = metaType;
            this.PersistableFactoryType = persistableFactoryType;
            this.PersistableObjectType = metaType.GetImplementationBy(persistableFactoryType);
            this.DomainObjectMembers = TypeMemberCache.Of(metaType.DomainObjectType ?? typeof(object));
            this.PersistableObjectMembers = TypeMemberCache.Of(this.PersistableObjectType);
            this.PropertyMap = propertyMap;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyInfo GetBasePropertyToImplement(IPropertyMetadata metaProperty)
        {
            var baseProperty = (
                MetaType.DomainObjectType != null
                ? DomainObjectMembers.ImplementableProperties.Single(p => p.Name == metaProperty.Name)
                : metaProperty.ContractPropertyInfo);

            return baseProperty;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable CreatePropertyTypeTemplateScope(IPropertyMetadata metaProperty)
        {
            if ( metaProperty.ClrType.IsEntityContract() || metaProperty.ClrType.IsEntityPartContract() )
            {
                return TypeTemplate.CreateScope<TT2.TDomainItem, TT2.TPersistableItem>(
                    metaProperty.ClrType,
                    metaProperty.Relation.RelatedPartyType.ContractType);
            }

            Type collectionItemType;

            if ( metaProperty.ClrType.IsCollectionType(out collectionItemType) &&
                (collectionItemType.IsEntityContract() || collectionItemType.IsEntityPartContract()) )
            {
                return TypeTemplate.CreateScope<TT2.TDomainItem, TT2.TPersistableItem>(
                    collectionItemType,
                    metaProperty.Relation.RelatedPartyType.ContractType);
            }

            return null;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectFactoryContext BaseContext { get; private set; }
        public ITypeMetadataCache MetadataCache { get; private set; }
        public ITypeMetadata MetaType { get; private set; }
        public Type PersistableFactoryType { get; private set; }
        public Type PersistableObjectType { get; private set; }
        public Field<TT2.TPersistable> PersistableObjectField { get; set; }
        public Field<IDomainObjectFactory> DomainObjectFactoryField { get; set; }
        public TypeMemberCache DomainObjectMembers { get; private set; }
        public TypeMemberCache PersistableObjectMembers { get; private set; }
        public PropertyImplementationStrategyMap PropertyMap { get; private set; }
    }
}