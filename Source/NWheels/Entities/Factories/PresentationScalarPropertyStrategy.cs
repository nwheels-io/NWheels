using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class PresentationScalarPropertyStrategy : PropertyImplementationStrategy
    {
        private readonly PresentationObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PresentationScalarPropertyStrategy(
            PropertyImplementationStrategyMap ownerMap,
            PresentationObjectFactoryContext context, 
            IPropertyMetadata metaProperty)
            : base(ownerMap, context.BaseContext, context.MetadataCache, context.MetaType, metaProperty)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.Property(MetaProperty.ContractPropertyInfo).ImplementPropagate(_context.DomainObjectField);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        #endregion
    }
}
