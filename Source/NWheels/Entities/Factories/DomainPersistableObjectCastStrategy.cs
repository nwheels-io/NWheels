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
using NWheels.Entities.Core;
using NWheels.TypeModel.Core;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class DomainPersistableObjectCastStrategy : PropertyImplementationStrategy
    {
        private readonly DomainObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainPersistableObjectCastStrategy(
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
            
            writer.Property(baseProperty).Implement( 
                p => baseProperty.CanRead
                    ? p.Get(gw => {
                        base.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;
                        gw.Return(
                            _context.PersistableObjectField
                            .Prop<TT.TProperty>(MetaProperty.ContractPropertyInfo)
                            .CastTo<IContainedIn<IDomainObject>>()
                            .Func<IDomainObject>(x => x.GetContainerObject)
                            .CastTo<TT.TProperty>());
                    })
                    : null,
                p => baseProperty.CanWrite
                    ? p.Set((sw, value) => {
                        base.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;
                        _context.PersistableObjectField.Prop<TT.TProperty>(MetaProperty.ContractPropertyInfo).Assign(
                            value
                            .CastTo<IContain<IPersistableObject>>()
                            .Func<IPersistableObject>(x => x.GetContainedObject)
                            .CastTo<TT.TProperty>());
                    })
                    : null
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        #endregion
    }
}
