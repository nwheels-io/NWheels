using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hapil;
using Hapil.Writers;
using Hapil.Members;
using Hapil.Operands;
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
            context.BaseType = typeof(ConfigurationSectionBase);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            var properties = new List<PropertyMember>();

            writer.AllProperties().Implement(
                p => p.Get(pw => {
                    properties.Add(p.OwnerProperty);
                    pw.Return(p.BackingField);
                }),
                p => p.Set((pw, value) => p.BackingField.Assign(value)));
            
            writer.ImplementBase<ConfigurationSectionBase>().Method<XElement>(x => x.LoadFrom).Implement((m, xml) => {
                foreach ( var property in properties )
                {
                    using ( TT.CreateScope<TT.TProperty>(((PropertyInfo)property.MemberDeclaration).PropertyType) )
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
    }
}
