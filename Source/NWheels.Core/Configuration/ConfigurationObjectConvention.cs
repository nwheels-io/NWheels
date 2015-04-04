using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hapil;
using Hapil.Writers;
using Hapil.Members;
using Hapil.Operands;
using NWheels.Configuration;
using NWheels.Core.DataObjects;
using NWheels.DataObjects;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Utilities;
using TT = Hapil.TypeTemplate;
using System.Reflection;
using NWheels.Core.Conventions;

namespace NWheels.Core.Configuration
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
                _initializers.Add(cw => {
                    using ( TT.CreateScope<TT.TProperty>(property.PropertyType) )
                    {
                        IOperand<TT.TProperty> defaultValue;
                        if ( propertyMetadata.TryGetDefaultValueOperand(cw, out defaultValue) )
                        {
                            cw.OwnerClass.GetPropertyBackingField(property).AsOperand<TT.TProperty>().Assign(defaultValue);
                        }
                    }
                });
            }
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
                        cw.OwnerClass.GetPropertyBackingField(property)
                            .AsOperand<TT.TProperty>()
                            .Assign(cw.New<CollectionAdapter<TT.TImpl, TT.TContract>>(cw.New<List<TT.TImpl>>()).CastTo<TT.TProperty>());
                    }
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementConstructors(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            if ( !IsSectionObject )
            {
                writer.Constructor<ConfigurationElementBase>((cw, parent) => {
                    cw.Base(parent);
                    _initializers.ForEach(init => init(cw));
                });
            }

            writer.Constructor<IConfigurationObjectFactory, Auto<IConfigurationLogger>, string>((cw, factory, logger, path) => {
                cw.Base(factory, logger, path);
                _initializers.ForEach(init => init(cw));
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
                                m.This<ConfigurationElementBase>().Void<XElement, string, ICollection<TT.TItem>>(
                                    x => (a, b, c) => x.ReadNestedElementCollection<TT.TItem>(a, b, c),
                                    xml,
                                    m.Const(property.Name),
                                    backingField.CastTo<ICollection<TT.TItem>>());
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryGetPropertyDefaultValue(MethodWriterBase writer, PropertyInfo property, out IOperand<TT.TProperty> valueOperand)
        {
            var attribute = property.GetCustomAttribute<DefaultValueAttribute>(inherit: true);

            if ( attribute != null && attribute.Value != null )
            {
                if ( property.PropertyType.IsInstanceOfType(attribute.Value) )
                {
                    if ( attribute.Value is System.Type )
                    {
                        valueOperand = Static.Func<string, bool, Type>(
                            Type.GetType,
                            writer.Const(((Type)attribute.Value).AssemblyQualifiedName),
                            writer.Const(true)).CastTo<TT.TProperty>();
                    }
                    else
                    {
                        var valueOperandType = typeof(Constant<>).MakeGenericType(attribute.Value.GetType());
                        valueOperand = ((IOperand)Activator.CreateInstance(valueOperandType, attribute.Value)).CastTo<TT.TProperty>();
                    }
                }
                else if ( attribute.Value is string )
                {
                    valueOperand = Static.Func(ParseUtility.Parse<TT.TProperty>, writer.Const((string)attribute.Value));
                }
                else
                {
                    throw new ContractConventionException(
                        this.GetType(), Context.TypeKey.PrimaryInterface, property, "Specified default value could not be parsed");
                }

                return true;
            }

            valueOperand = null;
            return false;
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
