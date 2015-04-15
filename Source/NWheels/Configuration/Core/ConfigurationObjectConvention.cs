using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Utilities;
using TT = Hapil.TypeTemplate;

namespace NWheels.Configuration.Core
{
    public class ConfigurationObjectConvention : ImplementationConvention
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly List<Action<ConstructorWriter>> _initializers;
        private ITypeMetadata _objectMetadata;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConfigurationObjectConvention(ITypeMetadataCache metadataCache)
            : base(Will.InspectDeclaration | Will.ImplementBaseClass)
        {
            _metadataCache = metadataCache;
            _initializers = new List<Action<ConstructorWriter>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnInspectDeclaration(ObjectFactoryContext context)
        {
            _objectMetadata = _metadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);
            context.BaseType = (IsSectionObject ? typeof(ConfigurationSectionBase) : typeof(ConfigurationElementBase));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            ImplementContract(writer, _objectMetadata.ContractType);

            foreach ( var mixinType in _objectMetadata.MixinContractTypes )
            {
                ImplementContract(writer, mixinType);
            }

            ImplementLoadPropertiesMethod(writer);
            ImplementSavePropertiesMethod(writer);
            ImplementGetXmlNameMethod(writer);
            ImplementConstructors(writer);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementContract(ImplementationClassWriter<TypeTemplate.TBase> writer, Type contractType)
        {
            using ( TT.CreateScope<TT.TInterface>(contractType) )
            {
                var implementation = writer.ImplementInterfaceExplicitly<TypeTemplate.TInterface>();

                implementation.AllProperties(IsScalarProperty).ForEach(p => ImplementScalarProperty(implementation, p));
                implementation.AllProperties(IsNestedElementProperty).ForEach(p => ImplementNestedElementProperty(implementation, p));
                implementation.AllProperties(IsNestedElementCollectionProperty).ForEach(p => ImplementNestedElementCollectionProperty(implementation, p));
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementScalarProperty(
            ImplementationClassWriter<TypeTemplate.TInterface> implementation,
            PropertyInfo property)
        {
            var propertyMetadata = (PropertyMetadataBuilder)_objectMetadata.GetPropertyByDeclaration(property);

            implementation.Property(property).ImplementAutomatic();

            if ( propertyMetadata.DefaultValue != null )
            {
                ImplementPropertyDefaultValue(property, propertyMetadata);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementPropertyDefaultValue(PropertyInfo property, PropertyMetadataBuilder propertyMetadata)
        {
            _initializers.Add(cw => {
                using ( TT.CreateScope<TT.TProperty>(property.PropertyType) )
                {
                    IOperand<TT.TProperty> defaultValue;
                    if ( propertyMetadata.TryGetDefaultValueOperand(cw, out defaultValue) )
                    {
                        cw.OwnerClass.GetPropertyBackingField(property).AsOperand<TT.TProperty>().Assign(defaultValue);
                        cw.This<ConfigurationElementBase>().Void(x => x.PushPropertyOverrideHistory, cw.Const(property.Name), defaultValue.CastTo<object>());
                    }
                }
            });
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementNestedElementProperty(
            ImplementationClassWriter<TypeTemplate.TInterface> implementation,
            PropertyInfo property)
        {
            var propertyMetadata = (PropertyMetadataBuilder)_objectMetadata.GetPropertyByDeclaration(property);
            var elementTypeKey = new TypeKey(primaryInterface: property.PropertyType);
            var elementType = base.Context.Factory.FindDynamicType(elementTypeKey);

            implementation.Property(property).ImplementAutomatic();

            _initializers.Add(cw => {
                using ( TT.CreateScope<TT.TProperty, TT.TImpl>(property.PropertyType, elementType) )
                {
                    cw.OwnerClass.GetPropertyBackingField(property).AsOperand<TT.TProperty>().Assign(
                        cw.New<TT.TImpl>(cw.This<ConfigurationElementBase>())
                        .CastTo<TT.TProperty>());
                }
            });
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementNestedElementCollectionProperty(
            ImplementationClassWriter<TypeTemplate.TInterface> implementation,
            PropertyInfo property)
        {
            var propertyMetadata = _objectMetadata.GetPropertyByDeclaration(property);
            var elementContractType = propertyMetadata.Relation.RelatedPartyType.ContractType;
            var elementTypeKey = new TypeKey(primaryInterface: elementContractType);
            var elementImplementationType = base.Context.Factory.FindDynamicType(elementTypeKey);

            implementation.Property(property).ImplementAutomatic();

            _initializers.Add(cw => {
                using ( TT.CreateScope<TT.TProperty>(property.PropertyType) )
                {
                    using ( TT.CreateScope<TT.TContract, TT.TImpl>(elementContractType, elementImplementationType) )
                    {
                        var backingFieldOperand = cw.OwnerClass.GetPropertyBackingField(property).AsOperand<TT.TProperty>();

                        if ( IsNamedElementCollectionProperty(property) )
                        {
                            backingFieldOperand.Assign(cw.New<NamedObjectCollectionAdapter<TT.TImpl, TT.TContract>>(
                                cw.New<List<TT.TImpl>>(),
                                cw.MakeDelegate<ConfigurationElementBase, Func<TT.TImpl>>(
                                    cw.This<ConfigurationElementBase>(), x => x.CreateNestedElement<TT.TContract, TT.TImpl>))
                                .CastTo<TT.TProperty>());
                        }
                        else
                        {
                            backingFieldOperand.Assign(cw.New<ObjectCollectionAdapter<TT.TImpl, TT.TContract>>(
                                cw.New<List<TT.TImpl>>(), 
                                cw.MakeDelegate<ConfigurationElementBase, Func<TT.TImpl>>(
                                    cw.This<ConfigurationElementBase>(), x => x.CreateNestedElement<TT.TContract, TT.TImpl>))
                                .CastTo<TT.TProperty>());
                        }
                    }
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementConstructors(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            //TODO:bug in Hapil? when passing ConfigurationSourceInfo.UseSource() directly to Using, two calls to ConfigurationSourceInfo.UseSource are generated instead of one.

            if ( !IsSectionObject )
            {
                writer.Constructor<ConfigurationElementBase>((cw, parent) => {
                    cw.Base(parent);
                    //workaround: initialize a local and then pass the local to Using
                    var sourceUseScopeLocal = cw.Local(initialValue: Static.Func(ConfigurationSourceInfo.UseSource, Static.Prop(() => ConfigurationSourceInfo.Default)));
                    cw.Using(sourceUseScopeLocal).Do(() => { 
                        _initializers.ForEach(init => init(cw));
                    });
                });
            }

            writer.Constructor<IConfigurationObjectFactory, IConfigurationLogger, string>((cw, factory, logger, path) => {
                cw.Base(factory, logger, path);
                //workaround: initialize a local and then pass the local to Using
                var sourceUseScopeLocal = cw.Local(initialValue: Static.Func(ConfigurationSourceInfo.UseSource, Static.Prop(() => ConfigurationSourceInfo.Default)));
                cw.Using(sourceUseScopeLocal).Do(() => {
                    _initializers.ForEach(init => init(cw));
                });
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementGetXmlNameMethod(ClassWriterBase classWriter)
        {
            var xmlName = ConfigurationElementAttribute.From(Context.TypeKey.PrimaryInterface).XmlName;

            if ( string.IsNullOrEmpty(xmlName) )
            {
                xmlName = Context.TypeKey.PrimaryInterface.Name.TrimPrefix("I").TrimSuffix("Config");
            }

            classWriter.ImplementBase<ConfigurationSectionBase>().Method<string>(x => x.GetXmlName).Implement(m => m.Return(constantValue: xmlName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementLoadPropertiesMethod(ClassWriterBase classWriter)
        {
            classWriter.ImplementBase<ConfigurationSectionBase>().Method<XElement>(x => x.LoadProperties).Implement((m, xml) => {
                foreach ( var property in _objectMetadata.Properties )
                {
                    using ( TT.CreateScope<TT.TProperty>(property.ClrType) )
                    {
                        var backingField = classWriter.OwnerClass.GetPropertyBackingField(property.ContractPropertyInfo).AsOperand<TT.TProperty>();

                        if ( property.Kind == PropertyKind.Scalar )
                        {
                            m.This<ConfigurationElementBase>().Void<XElement, string, TT.TProperty>(
                                x => (a, b, c) => x.ReadScalarValue<TT.TProperty>(a, b, ref c),
                                xml,
                                m.Const(property.Name),
                                backingField);
                        }
                        else if ( property.Kind == PropertyKind.Relation && property.Relation.RelationKind.IsIn(RelationKind.OneToOne, RelationKind.ManyToOne) )
                        {
                            m.This<ConfigurationElementBase>().Void<XElement, string, IConfigurationElement>(
                                x => x.ReadNestedElement,
                                xml,
                                m.Const(property.Name),
                                backingField.CastTo<IConfigurationElement>());
                        }
                        else if ( property.Kind == PropertyKind.Relation && property.Relation.RelationKind.IsIn(RelationKind.OneToMany, RelationKind.ManyToMany) )
                        {
                            using ( TT.CreateScope<TT.TItem>(property.Relation.RelatedPartyType.ContractType) )
                            {
                                if ( property.ClrType.GetGenericTypeDefinition() == typeof(INamedObjectCollection<>) )
                                {
                                    m.This<ConfigurationElementBase>().Void<XElement, string, INamedObjectCollection<TT.TItem>>(
                                        x => (a, b, c) => x.ReadNestedNamedElementCollection<TT.TItem>(a, b, c),
                                        xml,
                                        m.Const(property.Name),
                                        backingField.CastTo<INamedObjectCollection<TT.TItem>>());
                                }
                                else
                                {
                                    m.This<ConfigurationElementBase>().Void<XElement, string, ICollection<TT.TItem>>(
                                        x => (a, b, c) => x.ReadNestedElementCollection<TT.TItem>(a, b, c),
                                        xml,
                                        m.Const(property.Name),
                                        backingField.CastTo<ICollection<TT.TItem>>());
                                }
                            }
                        }
                    }
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementSavePropertiesMethod(ClassWriterBase classWriter)
        {
            classWriter.ImplementBase<ConfigurationSectionBase>().Method<XElement>(x => x.SaveProperties).Implement((m, xml) =>
            {
                foreach ( var property in _objectMetadata.Properties )
                {
                    using ( TT.CreateScope<TT.TProperty>(property.ClrType) )
                    {
                        var backingField = classWriter.OwnerClass.GetPropertyBackingField(property.ContractPropertyInfo).AsOperand<TT.TProperty>();

                        if ( property.Kind == PropertyKind.Scalar )
                        {
                            m.This<ConfigurationElementBase>().Void<XElement, string, TT.TProperty>(
                                x => (a, b, c) => x.WriteScalarValue<TT.TProperty>(a, b, ref c),
                                xml,
                                m.Const(property.Name),
                                backingField);
                        }
                        else if ( property.Kind == PropertyKind.Relation && property.Relation.RelationKind.IsIn(RelationKind.OneToOne, RelationKind.ManyToOne) )
                        {
                            m.This<ConfigurationElementBase>().Void<XElement, string, IConfigurationElement>(
                                x => x.WriteNestedElement,
                                xml,
                                m.Const(property.Name),
                                backingField.CastTo<IConfigurationElement>());
                        }
                        else if ( property.Kind == PropertyKind.Relation && property.Relation.RelationKind.IsIn(RelationKind.OneToMany, RelationKind.ManyToMany) )
                        {
                            using ( TT.CreateScope<TT.TItem>(property.Relation.RelatedPartyType.ContractType) )
                            {
                                m.This<ConfigurationElementBase>().Void<XElement, string, INamedObjectCollection<TT.TItem>>(
                                    x => (a, b, c) => x.WriteNestedElementCollection<TT.TItem>(a, b, c),
                                    xml,
                                    m.Const(property.Name),
                                    backingField.CastTo<INamedObjectCollection<TT.TItem>>());
                            }
                        }
                    }
                }
            });
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsScalarProperty(PropertyInfo property)
        {
            return (_objectMetadata.GetPropertyByDeclaration(property).Kind == PropertyKind.Scalar);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsNestedElementProperty(PropertyInfo property)
        {
            var propertyMetadata = _objectMetadata.GetPropertyByDeclaration(property);

            return (
                propertyMetadata.Kind == PropertyKind.Relation &&
                (propertyMetadata.Relation.RelationKind == RelationKind.ManyToOne ||
                propertyMetadata.Relation.RelationKind == RelationKind.OneToOne));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsNestedElementCollectionProperty(PropertyInfo property)
        {
            var propertyMetadata = _objectMetadata.GetPropertyByDeclaration(property);

            return (
                propertyMetadata.Kind == PropertyKind.Relation &&
                (propertyMetadata.Relation.RelationKind == RelationKind.OneToMany ||
                propertyMetadata.Relation.RelationKind == RelationKind.ManyToMany));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsNamedElementCollectionProperty(PropertyInfo property)
        {
            return (
                IsNestedElementCollectionProperty(property) && 
                property.PropertyType.GetGenericTypeDefinition() == typeof(INamedObjectCollection<>));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsSectionObject
        {
            get
            {
                return ConfigurationSectionAttribute.IsConfigSection(_objectMetadata.ContractType);
            }
        }
    }
}
