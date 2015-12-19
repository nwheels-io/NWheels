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

namespace NWheels.Entities.Factories.PropertyStrategies
{
    public class __LazyObjectCollectionPropertyStrategy : PropertyImplementationStrategy
    {
        private readonly DomainObjectFactoryContext _context;
        //private Field<TT.TProperty> _backingField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public __LazyObjectCollectionPropertyStrategy(
            PropertyImplementationStrategyMap ownerMap, 
            DomainObjectFactoryContext context, 
            IPropertyMetadata metaProperty)
            : base(ownerMap, context.BaseContext, context.MetadataCache, context.MetaType, metaProperty)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            //_backingField = writer.Field<TT.TProperty>("m_" + MetaProperty.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.Property(MetaProperty.ContractPropertyInfo).ImplementAutomatic();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        #endregion
    }
}
