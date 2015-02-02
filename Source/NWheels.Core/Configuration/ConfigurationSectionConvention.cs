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
using NWheels.Exceptions;
using NWheels.Utilities;
using TT = Hapil.TypeTemplate;
using System.Reflection;

namespace NWheels.Core.Configuration
{
    public class ConfigurationSectionConvention : ImplementationConvention
    {
        public ConfigurationSectionConvention()
            : base(Will.InspectDeclaration | Will.ImplementPrimaryInterface)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnInspectDeclaration(ObjectFactoryContext context)
        {
            if ( ConfigurationSectionAttribute.From(context.TypeKey.PrimaryInterface) == null )
            {
                throw new ContractConventionException(this.GetType(), context.TypeKey.PrimaryInterface, "Missing ConfigurationSection attribute.");
            }

            context.BaseType = typeof(ConfigurationSectionBase);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            var properties = new List<PropertyMember>();

            ImplementProperties(writer, properties);
            ImplementLoadPropertiesMethod(writer, properties);
            ImplementGetXmlNameMethod(writer);
            ImplementConstructor(writer, properties);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementConstructor(ImplementationClassWriter<TypeTemplate.TInterface> writer, List<PropertyMember> properties)
        {
            writer.Constructor<Auto<IConfigurationLogger>, string>((cw, logger, path) => {
                cw.Base(logger, path);

                foreach ( var property in properties )
                {
                    using ( TT.CreateScope<TT.TProperty>(property.PropertyDeclaration.PropertyType) )
                    {
                        IOperand<TT.TProperty> defaultValue;

                        if ( TryGetPropertyDefaultValue(cw, property.PropertyDeclaration, out defaultValue) )
                        {
                            property.BackingField.AsOperand<TT.TProperty>().Assign(defaultValue);
                        }
                    }
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementGetXmlNameMethod(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            var xmlName = ConfigurationSectionAttribute.From(Context.TypeKey.PrimaryInterface).XmlName;

            if ( string.IsNullOrEmpty(xmlName) )
            {
                xmlName = Context.TypeKey.PrimaryInterface.Name.TrimPrefix("I").TrimSuffix("Config");
            }

            writer.ImplementBase<ConfigurationSectionBase>().Method<string>(x => x.GetXmlName).Implement(m => m.Return(constantValue: xmlName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementProperties(ImplementationClassWriter<TypeTemplate.TInterface> writer, List<PropertyMember> properties)
        {
            writer.ReadOnlyProperties().Implement(
                p => p.Get(pw => {
                    properties.Add(p.OwnerProperty);
                    pw.Return(p.BackingField);
                })
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementLoadPropertiesMethod(ImplementationClassWriter<TypeTemplate.TInterface> writer, List<PropertyMember> properties)
        {
            writer.ImplementBase<ConfigurationSectionBase>().Method<XElement>(x => x.LoadProperties).Implement((m, xml) => {
                foreach ( var property in properties )
                {
                    using ( TT.CreateScope<TT.TProperty>(property.PropertyDeclaration.PropertyType) )
                    {
                        m.This<ConfigurationSectionBase>().Void<XElement, string, TT.TProperty>(
                            x => (a, b, c) => x.TryReadScalarValue<TT.TProperty>(a, b, ref c),
                            xml,
                            m.Const(property.Name),
                            property.BackingField.AsOperand<TT.TProperty>());
                    }
                }
            });
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
    }
}
