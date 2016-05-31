using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Autofac;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Factories;
using NWheels.Extensions;
using NWheels.TypeModel;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.UI.Factories
{
    public interface IViewModelObjectFactory : IEntityObjectFactory
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ViewModelObjectFactory : EntityObjectFactory, IViewModelObjectFactory
    {
        public ViewModelObjectFactory(IComponentContext components, DynamicModule module, TypeMetadataCache metadataCache)
            : base(components, module, metadataCache)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var metaType = MetadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);
            var propertyMap = BuildPropertyStrategyMap(context, metaType);

            propertyMap.NeedImplementationTypeKey += (sender, args) => {
                args.TypeKeyToUse = CreateImplementationTypeKey(args.ContractType);
            };

            return new IObjectFactoryConvention[] {
                new BaseTypeConvention(MetadataCache, metaType), 
                new PropertyImplementationConvention(metaType, propertyMap),
                new EntityContractMethodsNotSupportedConvention(),
                new MaterializationConstructorConvention(metaType, propertyMap),
                new InitializationConstructorConvention(metaType, propertyMap),
                new ImplementIObjectConvention(), 
                new NeverModifiedConvention(),
                new JsonExtensionDataConvention(propertyMap)
                //new ImplementIEntityObjectConvention(metaType, propertyMap), 
                //new ImplementIEntityPartObjectConvention(metaType), 
                //new EnsureDomainObjectConvention(metaType), 
                //new DependencyInjectionConvention(metaType, propertyMap, forceApply: true), 
                //new NestedObjectsConvention(propertyMap), 
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PropertyImplementationStrategyMap BuildPropertyStrategyMap(ObjectFactoryContext context, ITypeMetadata metaType)
        {
            var builder = new PropertyImplementationStrategyMap.Builder();
            Type collectionItemType;

            builder.AddRule(
                p => p.ClrType.IsCollectionType(out collectionItemType) && (collectionItemType.IsEntityPartContract() || collectionItemType.IsEntityContract()),
                p => new CollectionAdapterStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.ClrType.IsEntityPartContract(),
                p => new RelationTypecastStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => !(p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite),
                p => new PublicAccessorWrapperStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite,
                p => new AutomaticPropertyStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            return builder.Build(MetadataCache, metaType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override TypeKey CreateImplementationTypeKey(Type entityContractInterface)
        {
            return new TypeKey(
                primaryInterface: entityContractInterface,
                baseType: BaseTypeConvention.GetBaseType(this, base.MetadataCache.GetTypeMetadata(entityContractInterface)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Dictionary<string, string> ExtensionDataToStringDictionary(Dictionary<string, JToken> extensionData)
        {
            if (extensionData != null)
            { 
                return extensionData.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToStringOrDefault("null"));
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NeverModifiedConvention : ImplementationConvention
        {
            public NeverModifiedConvention()
                : base(Will.ImplementBaseClass)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                writer.ImplementInterfaceExplicitly<IObject>()
                    .Property(x => x.IsModified).Implement(p =>
                        p.Get(gw => gw.Return(false)
                    )
                );
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        /*
        public class JsonExtensionDataConvention : DecorationConvention
        {
            private readonly ITypeMetadataCache _metadataCache;
            private readonly ITypeMetadata _metaType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public JsonExtensionDataConvention(ITypeMetadataCache metadataCache, ITypeMetadata metaType)
                : base(Will.DecorateProperties)
            {
                _metadataCache = metadataCache;
                _metaType = metaType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnProperty(PropertyMember property, Func<PropertyDecorationBuilder> decorate)
            {
                IPropertyMetadata metaProperty = null;

                if (property.PropertyDeclaration != null)
                {
                    _metaType.TryGetPropertyByDeclaration(property.PropertyDeclaration, out metaProperty);
                }

                if (metaProperty == null)
                {
                    _metaType.TryGetPropertyByName(property.Name, out metaProperty);
                }

                if (metaProperty != null && 
                    metaProperty.ClrType == typeof(Dictionary<string, string>) &&
                    metaProperty.SemanticType != null && 
                    metaProperty.SemanticType.WellKnownSemantic == WellKnownSemanticType.ExtensionData)
                {
                    decorate().Attribute<JsonExtensionDataAttribute>();
                }
            }
        }
        */
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IHaveDeserializedCallback
        {
            void OnDeserialized();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class JsonExtensionDataConvention : ImplementationConvention
        {
            private readonly PropertyImplementationStrategyMap _propertyStrategyMap;
            private readonly IPropertyMetadata _extensionDataProperty;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public JsonExtensionDataConvention(PropertyImplementationStrategyMap propertyStrategyMap)
                : base(Will.ImplementBaseClass)
            {
                _propertyStrategyMap = propertyStrategyMap;
                _extensionDataProperty = _propertyStrategyMap.Properties.FirstOrDefault(IsExtensionDataProperty);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override bool ShouldApply(ObjectFactoryContext context)
            {
                return (_extensionDataProperty != null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                var contractExtensionBackingField = writer.OwnerClass
                    .GetPropertyBackingField(_extensionDataProperty.ContractPropertyInfo)
                    .AsOperand<Dictionary<string, string>>();

                Field<Dictionary<string, JToken>> jsonExtensionBackingField = null;

                writer.NewVirtualWritableProperty<Dictionary<string, JToken>>("JsonExtensionData").Implement(
                    Attributes.Set<JsonExtensionDataAttribute>(),
                    p => p.Get(gw => {
                        jsonExtensionBackingField = p.BackingField;
                        gw.Return(p.BackingField);
                    }),
                    p => p.Set((sw, value) => {
                        p.BackingField.Assign(value);
                    })
                );

                writer.ImplementInterfaceExplicitly<IHaveDeserializedCallback>()
                    .Method(x => x.OnDeserialized).Implement(w => {
                        contractExtensionBackingField.Assign(Static.Func(ExtensionDataToStringDictionary, jsonExtensionBackingField));
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static bool IsExtensionDataProperty(IPropertyMetadata metaProperty)
            {
                return (
                    metaProperty.ClrType == typeof(Dictionary<string, string>) &&
                    metaProperty.SemanticType != null &&
                    metaProperty.SemanticType.WellKnownSemantic == WellKnownSemanticType.ExtensionData);
            }
        }
    }
}
