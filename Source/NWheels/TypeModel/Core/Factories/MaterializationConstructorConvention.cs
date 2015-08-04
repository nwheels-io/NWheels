using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;

namespace NWheels.TypeModel.Core.Factories
{
    public class MaterializationConstructorConvention : ImplementationConvention
    {
        private readonly ITypeMetadata _metaType;
        private readonly PropertyImplementationStrategyMap _propertyStrategyMap;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MaterializationConstructorConvention(
            ITypeMetadata metaType,
            PropertyImplementationStrategyMap propertyStrategyMap)
            : base(Will.ImplementBaseClass)
        {
            _metaType = metaType;
            _propertyStrategyMap = propertyStrategyMap;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            writer.DefaultConstructor(cw => {
                cw.Base();
                _propertyStrategyMap.InvokeStrategies(
                    strategy => {
                        strategy.WriteMaterialization(cw);
                    });
            });
        }

        #endregion
    }
}
