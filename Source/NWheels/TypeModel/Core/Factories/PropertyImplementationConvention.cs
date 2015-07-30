using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.Exceptions;
using TT = Hapil.TypeTemplate;

namespace NWheels.DataObjects.Core.Factories
{
    public class PropertyImplementationConvention : ImplementationConvention
    {
        private readonly ITypeMetadata _metaType;
        private readonly PropertyImplementationStrategyMap _propertyStrategyMap;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyImplementationConvention(
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
            var strategyGroupsByDeclaringInterface = 
                _propertyStrategyMap.Strategies.GroupBy(strategy => strategy.MetaProperty.ContractPropertyInfo.DeclaringType);

            foreach ( var group in strategyGroupsByDeclaringInterface )
            {
                var interfaceType = group.Key;

                using ( TT.CreateScope<TT.TInterface>(interfaceType) )
                {
                    var explicitImplementation = writer.ImplementInterfaceExplicitly<TT.TInterface>();

                    PropertyImplementationStrategyMap.InvokeStrategies(group, strategy => {
                        strategy.WritePropertyImplementation(explicitImplementation);
                    });
                }
            }
        }

        #endregion
    }
}
