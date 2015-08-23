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
using NWheels.TypeModel.Core.Factories;
using TT = Hapil.TypeTemplate;
using TT2 = NWheels.Entities.Factories.DomainObjectFactory.TemplateTypes;

namespace NWheels.Entities.Factories
{
    public class DomainPersistableCollectionCastStrategy : PropertyImplementationStrategy
    {
        private readonly DomainObjectFactoryContext _context;
        private Field<TT.TProperty> _adapterBackingField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainPersistableCollectionCastStrategy(
            DomainObjectFactoryContext context,
            IPropertyMetadata metaProperty)
            : base(context.BaseContext, context.MetadataCache, context.MetaType, metaProperty)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            _adapterBackingField = writer.Field<TT.TProperty>("m_" + MetaProperty.Name + "$adapter");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            var baseProperty = _context.GetBasePropertyToImplement(MetaProperty);
            
            writer.Property(baseProperty).Implement(p => 
                p.Get(gw => {
                    this.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;
                    gw.Return(_adapterBackingField);
                })
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnAfterImplementation(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
            if ( TT.Resolve<TT.TProperty>().GetGenericTypeDefinition() == typeof(IList<>) )
            {
                _adapterBackingField.Assign(
                    Static.Func(RuntimeEntityModelHelpers.CreateContainmentListAdapter<IPersistableObject, IDomainObject, TT2.TPersistableItem, TT2.TDomainItem>,
                        args[0].CastTo<TT.TContract>().Prop<IList<TT2.TPersistableItem>>(MetaProperty.ContractPropertyInfo),
                        _context.DomainObjectFactoryField
                    )
                    .CastTo<TT.TProperty>());
            }
            else
            {
                _adapterBackingField.Assign(
                    Static.Func(RuntimeEntityModelHelpers.CreateContainmentCollectionAdapter<IPersistableObject, IDomainObject, TT2.TPersistableItem, TT2.TDomainItem>,
                        args[0].CastTo<TT.TContract>().Prop<ICollection<TT2.TPersistableItem>>(MetaProperty.ContractPropertyInfo),
                        _context.DomainObjectFactoryField
                    )
                    .CastTo<TT.TProperty>());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool OnHasNestedObjects()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
            var w = writer;
            
            Static.Void(RuntimeTypeModelHelpers.DeepListNestedObjectCollection, 
                w.This<TT.TBase>().Prop<TT.TProperty>(this.ImplementedContractProperty).CastTo<System.Collections.IEnumerable>(), 
                nestedObjects);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingReturnTrueIfModified(FunctionMethodWriter<bool> functionWriter)
        {
            using ( TT.CreateScope<TT.TItem>(MetaProperty.Relation.RelatedPartyType.ContractType) )
            {
                functionWriter.If(_adapterBackingField.CastTo<IChangeTrackingCollection<TT.TItem>>().Prop(x => x.IsChanged)).Then(() => {
                    functionWriter.Return(true);                        
                });
            }
        }

        #endregion
    }
}
