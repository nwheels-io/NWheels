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
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;

namespace NWheels.TypeModel.Core.Factories
{
    public class DependencyInjectionConvention : CompositeConvention
    {
        private readonly PropertyImplementationStrategyMap _propertyStrategyMap;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DependencyInjectionConvention(PropertyImplementationStrategyMap propertyStrategyMap)
            : base(
                new ImplementIHaveDependencies(propertyStrategyMap), 
                new CallInjectDependenciesFromInitializationConstructor())
        {
            _propertyStrategyMap = propertyStrategyMap;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of CompositeConvention

        public override bool ShouldApply(ObjectFactoryContext context)
        {
            return _propertyStrategyMap.Strategies.Any(strategy => strategy.HasDependencies);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ImplementIHaveDependencies : ImplementationConvention
        {
            private readonly PropertyImplementationStrategyMap _propertyStrategyMap;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ImplementIHaveDependencies(PropertyImplementationStrategyMap propertyStrategyMap)
                : base(Will.ImplementBaseClass)
            {
                _propertyStrategyMap = propertyStrategyMap;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                writer.ImplementInterface<IHaveDependencies>()
                    .Method<IComponentContext>(x => x.InjectDependencies).Implement((m, components) => {
                        writer.DependencyField<IComponentContext>("$components").Assign(components);
                        
                        _propertyStrategyMap.InvokeStrategies(
                            predicate: strategy => strategy.HasDependencies,
                            action: strategy => strategy.WriteResolveDependencies(writer, m, components));
                    });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class CallInjectDependenciesFromInitializationConstructor : DecorationConvention
        {
            private Field<IComponentContext> _componentsField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CallInjectDependenciesFromInitializationConstructor()
                : base(Will.DecorateClass | Will.DecorateConstructors)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
            {
                _componentsField = classWriter.DependencyField<IComponentContext>("$components");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnConstructor(MethodMember member, Func<ConstructorDecorationBuilder> decorate)
            {
                if ( IsInitializationConstructor(member) )
                {
                    decorate().OnBefore(
                        m => {
                            m.This<IHaveDependencies>().Void<IComponentContext>(x => x.InjectDependencies, _componentsField);
                        });
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsInitializationConstructor(MethodMember method)
            {
                return (method.Signature.ArgumentCount > 0);
            }
        }
    }
}
