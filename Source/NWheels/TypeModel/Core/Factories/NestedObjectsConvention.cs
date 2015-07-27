using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;

namespace NWheels.TypeModel.Core.Factories
{
    public class NestedObjectsConvention : ImplementationConvention
    {
        private readonly PropertyImplementationStrategyMap _propertyStrategyMap;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NestedObjectsConvention(PropertyImplementationStrategyMap propertyStrategyMap)
            : base(Will.ImplementBaseClass)
        {
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
                    _propertyStrategyMap.InvokeStrategies(
                        predicate: strategy => strategy.HasNestedObjects,
                        action: strategy => strategy.WriteDeepListNestedObjects(m, nestedObjects));
                });
        }

        #endregion
    }
}
