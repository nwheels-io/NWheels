using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;

namespace NWheels.Testing.Entities.Impl
{
    public class TestIdGeneratorConvention : DecorationConvention
    {
        private readonly ITypeMetadata _metaType;
        private readonly IPropertyMetadata _targetProperty;
        private Field<IComponentContext> _componentsField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestIdGeneratorConvention(ITypeMetadata metaType)
            : base(Will.DecorateClass | Will.DecorateConstructors)
        {
            _metaType = metaType;
            _targetProperty = (metaType.PrimaryKey != null ? metaType.PrimaryKey.Properties.FirstOrDefault(IsIntPropertyWithNoGenerator) : null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return (_targetProperty != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DecorationConvention

        protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
        {
            _componentsField = classWriter.DependencyField<IComponentContext>("$components");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnConstructor(MethodMember member, Func<ConstructorDecorationBuilder> decorate)
        {
            if ( IsInitializationConstructor(member) )
            {
                var backingField = member.OwnerClass.GetPropertyBackingField(_targetProperty.ContractPropertyInfo);

                decorate().OnSuccess(w =>
                    backingField.AsOperand<int>().Assign(
                        Static.Func(ResolutionExtensions.Resolve<TestIntIdValueGenerator>, _componentsField)
                        .Func<string, int>(x => x.GenerateValue, w.Const(_metaType.Name + "." + _targetProperty.Name))
                    )
                );
            }
        }


        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsInitializationConstructor(MethodMember member)
        {
            return member.Signature.ArgumentCount > 0;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsIntPropertyWithNoGenerator(IPropertyMetadata metaProperty)
        {
            return (metaProperty.ClrType == typeof(int) && metaProperty.DefaultValueGeneratorType == null);
        }
    }
}
