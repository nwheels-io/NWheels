using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
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
    public class NestedObjectsConvention : ImplementationConvention
    {
        private readonly ITypeMetadata _metaType;
        private readonly PropertyImplementationStrategyMap _propertyStrategyMap;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NestedObjectsConvention(ITypeMetadata metaType, PropertyImplementationStrategyMap propertyStrategyMap)
            : base(Will.ImplementBaseClass)
        {
            _metaType = metaType;
            _propertyStrategyMap = propertyStrategyMap;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return _propertyStrategyMap.Strategies.Any(s => s.HasNestedObjects);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            writer.ImplementInterface<IHaveNestedObjects>()
                .Method<HashSet<object>>(x => x.DeepListNestedObjects).Implement((m, nestedObjects) => {

                    CallBaseInjectDependenciesMethod(m, nestedObjects);

                    _propertyStrategyMap.InvokeStrategies(
                        predicate: strategy => strategy.HasNestedObjects,
                        action: strategy => strategy.WriteDeepListNestedObjects(m, nestedObjects));
                });
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void CallBaseInjectDependenciesMethod(VoidMethodWriter m, Argument<HashSet<object>> nestedObjects)
        {
            if (_metaType.BaseType != null && typeof(IHaveNestedObjects).IsAssignableFrom(m.OwnerClass.BaseType))
            {
                var baseDeepListNestedObjectsMethod = TypeMemberCache.Of(m.OwnerClass.BaseType).Methods.FirstOrDefault(IsDeepListNestedObjectsMethod);

                if (baseDeepListNestedObjectsMethod != null)
                {
                    using (TT.CreateScope<TT.TBase>(m.OwnerClass.BaseType))
                    {
                        m.This<TT.TBase>().Void<HashSet<object>>(baseDeepListNestedObjectsMethod, nestedObjects);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static MethodInfo _s_deepListNestedObjectsMethod =
            ExpressionUtility.GetMethodInfo<Expression<Action<IHaveNestedObjects, HashSet<object>>>>((x, o) => x.DeepListNestedObjects(o));

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsDeepListNestedObjectsMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return (
                method.DeclaringType != null &&
                method.DeclaringType.IsClass &&
                !method.IsAbstract &&
                !method.IsGenericMethod &&
                parameters.Length == 1 &&
                parameters[0].ParameterType == typeof(HashSet<object>) &&
                method.Name == _s_deepListNestedObjectsMethod.Name);
        }

    }
}
