using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Core;
using NWheels.TypeModel.Core;
using TT = Hapil.TypeTemplate;
using TT2 = NWheels.Entities.Factories.DomainObjectFactory.TemplateTypes;

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
                        
                        var domainObjectLocal = gw.Local<TT.TProperty>();
                        domainObjectLocal.Assign(
                            _context.PersistableObjectField
                            .Prop<TT.TProperty>(MetaProperty.ContractPropertyInfo)
                            .CastTo<IPersistableObject>()
                            .Func<IDomainObject>(x => x.GetContainerObject)
                            .CastTo<TT.TProperty>());

                        gw.If(domainObjectLocal.IsNotNull()).Then(() => {
                            gw.Return(domainObjectLocal);
                        }).Else(() => {
                            gw.Return(
                                _context.DomainObjectFactoryField.Func<TT.TProperty, TT.TProperty>(
                                    x => x.CreateDomainObjectInstance,
                                    _context.PersistableObjectField.Prop<TT.TProperty>(MetaProperty.ContractPropertyInfo))
                            );
                        });

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
        }

        #endregion
    }
}
