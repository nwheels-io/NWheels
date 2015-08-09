using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.TypeModel.Core;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class DomainPersistableDelegationStrategy : PropertyImplementationStrategy
    {
        private readonly DomainObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainPersistableDelegationStrategy(
            DomainObjectFactoryContext context,
            IPropertyMetadata metaProperty)
            : base(context.BaseContext, context.MetadataCache, context.MetaType, metaProperty)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            var baseProperty = _context.GetBasePropertyToImplement(MetaProperty);

            using ( TT.CreateScope<TT.TInterface>(MetaType.ContractType) )
            {
                writer.Property(baseProperty).ImplementPropagate(_context.PersistableObjectField.CastTo<TT.TInterface>());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        #endregion
    }
}
