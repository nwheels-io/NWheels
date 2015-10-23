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

namespace NWheels.Entities.Factories
{
    public class PresentationNestedCollectionPropertyStrategy : PropertyImplementationStrategy
    {
        private readonly PresentationObjectFactoryContext _context;
        private Type _collectionItemType;
        private Field<TT.TProperty> _adapterBackingField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PresentationNestedCollectionPropertyStrategy(
            PropertyImplementationStrategyMap ownerMap,
            PresentationObjectFactoryContext context,
            IPropertyMetadata metaProperty)
            : base(ownerMap, context.BaseContext, context.MetadataCache, context.MetaType, metaProperty)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            MetaProperty.ClrType.IsCollectionType(out _collectionItemType);
            _adapterBackingField = writer.Field<TT.TProperty>("m_" + MetaProperty.Name + "$adapter");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.Property(base.MetaProperty.ContractPropertyInfo).Implement(p => 
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
            using ( TT.CreateScope<TT.TItem>(_collectionItemType) )
            {
                if ( TT.Resolve<TT.TProperty>().GetGenericTypeDefinition() == typeof(IList<>) )
                {
                    //_adapterBackingField.Assign(
                    //    Static.Func(RuntimeEntityModelHelpers.CreatePresentationCollectionAdapter<TT.TInterface>,
                    //        args[0].CastTo<TT.TContract>().Prop<IList<TT2.TPersistableItem>>(MetaProperty.ContractPropertyInfo),
                    //        _context.DomainObjectFactoryField
                    //    )
                    //    .CastTo<TT.TProperty>());
                    throw new NotSupportedException();
                }
                else
                {
                    _adapterBackingField.Assign(
                        Static.Func(
                            RuntimeEntityModelHelpers.CreatePresentationCollectionAdapter<TT.TItem>,
                            args[0].CastTo<TT.TContract>().Prop<ICollection<TT.TItem>>(MetaProperty.ContractPropertyInfo),
                            _context.PresentationFactoryField).CastTo<TT.TProperty>());
                }
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
        }

        #endregion
    }
}
