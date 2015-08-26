using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;
using TT2 = NWheels.Entities.Factories.DomainObjectFactory.TemplateTypes;

namespace NWheels.Entities.Factories
{
    public class PresentationObjectPropertyImplementationConvention : ImplementationConvention
    {
        private readonly PresentationObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PresentationObjectPropertyImplementationConvention(PresentationObjectFactoryContext context)
            : base(Will.ImplementPrimaryInterface)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            PropertyImplementationStrategyMap.InvokeStrategies(
                _context.PropertyMap.Strategies,
                strategy => {
                    strategy.WritePropertyImplementation(writer);
                });
        }

        #endregion
    }
}
