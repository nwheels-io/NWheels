using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.Exceptions;

namespace NWheels.DataObjects.Core.Factories
{
    public class PropertyImplementationConvention : ImplementationConvention
    {
        private readonly ITypeMetadata _metaType;
        private readonly PropertyImplementationStrategyMap _propertyStrategyMap;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PropertyImplementationConvention(
            ITypeMetadata metaType, 
            PropertyImplementationStrategyMap propertyStrategyMap)
            : base(Will.ImplementPrimaryInterface)
        {
            _metaType = metaType;
            _propertyStrategyMap = propertyStrategyMap;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            _propertyStrategyMap.InvokeStrategies(
                strategy => {
                    strategy.WritePropertyImplementation(writer);
                });
        }

        #endregion
    }
}
