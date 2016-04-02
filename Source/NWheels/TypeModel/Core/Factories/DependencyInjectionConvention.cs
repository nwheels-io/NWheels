using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
using NWheels.Utilities;
using TT = Hapil.TypeTemplate;

namespace NWheels.TypeModel.Core.Factories
{
    public class DependencyInjectionConvention : CompositeConvention
    {
        private readonly PropertyImplementationStrategyMap _propertyStrategyMap;
        private readonly bool _forceApply;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DependencyInjectionConvention(ITypeMetadata metaType, PropertyImplementationStrategyMap propertyStrategyMap, bool forceApply = false)
            : base(
                new ImplementIHaveDependencies(metaType, propertyStrategyMap), 
                new CallInjectDependenciesFromInitializationConstructor())
        {
            _propertyStrategyMap = propertyStrategyMap;
            _forceApply = forceApply;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of CompositeConvention

        public override bool ShouldApply(ObjectFactoryContext context)
        {
            return (_forceApply || _propertyStrategyMap.Strategies.Any(strategy => strategy.HasDependencies));
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static MethodInfo _s_injectDependenciesMethod =
            ExpressionUtility.GetMethodInfo<Expression<Action<IHaveDependencies, IComponentContext>>>((x, c) => x.InjectDependencies(c));

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsInjectDependenciesMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return (
                method.DeclaringType != null &&
                method.DeclaringType.IsClass &&
                !method.IsAbstract &&
                !method.IsGenericMethod &&
                parameters.Length == 1 &&
                parameters[0].ParameterType == typeof(IComponentContext) &&
                method.Name == _s_injectDependenciesMethod.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ImplementIHaveDependencies : ImplementationConvention
        {
            private readonly PropertyImplementationStrategyMap _propertyStrategyMap;
            private readonly ITypeMetadata _metaType;
            private Type _factoryType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ImplementIHaveDependencies(ITypeMetadata metaType, PropertyImplementationStrategyMap propertyStrategyMap)
                : base(Will.InspectDeclaration | Will.ImplementBaseClass)
            {
                _propertyStrategyMap = propertyStrategyMap;
                _metaType = metaType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                _factoryType = context.Factory.GetType();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                writer.ImplementInterfaceVirtual<IHaveDependencies>().Method<IComponentContext>(x => x.InjectDependencies).Implement((m, components) => {
                    //Static.Void(DebugEx.WriteLine,
                    //    m.Const<string>("{0}->InjectDependencies(components={1})"),
                    //    m.NewArray<object>(
                    //        m.This<object>().FuncToString(),
                    //        m.Iif(components.IsNull(), m.Const<string>("NULL"), components.FuncToString()).CastTo<object>()
                    //    )
                    //);
                    
                    writer.DependencyField<IComponentContext>("$components").Assign(components);

                    _propertyStrategyMap.InvokeStrategies(
                        predicate: strategy => strategy.HasDependencies,
                        action: strategy => strategy.WriteResolveDependencies(writer, m, components));

                    CallBaseInjectDependenciesMethod(m, components);

                    _propertyStrategyMap.InvokeStrategies(
                        strategy => {
                            strategy.WriteDeserializedCallback(m);
                        });
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CallBaseInjectDependenciesMethod(VoidMethodWriter m, Argument<IComponentContext> components)
            {
                if ( _metaType.BaseType != null && typeof(IHaveDependencies).IsAssignableFrom(m.OwnerClass.BaseType) )
                {
                    var baseInjectDependenciesMethod = TypeMemberCache.Of(m.OwnerClass.BaseType).Methods.FirstOrDefault(IsInjectDependenciesMethod);

                    if ( baseInjectDependenciesMethod != null )
                    {
                        using ( TT.CreateScope<TT.TBase>(m.OwnerClass.BaseType) )
                        {
                            m.This<TT.TBase>().Void<IComponentContext>(baseInjectDependenciesMethod, components);
                        }
                    }
                }
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
                    decorate().OnSuccess(
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
