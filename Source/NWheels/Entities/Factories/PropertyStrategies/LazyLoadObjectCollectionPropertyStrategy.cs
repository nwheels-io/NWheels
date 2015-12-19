using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using TT = Hapil.TypeTemplate;
using NWheels.Entities.Core;

namespace NWheels.Entities.Factories.PropertyStrategies
{
    public class LazyLoadObjectCollectionPropertyStrategy : PropertyImplementationStrategy
    {
        private readonly DomainObjectFactoryContext _context;
        private Type _itemImplementationType;
        private Type _itemContractType;
        private Field<IList<TT.TContract>> _backingField;
        private Field<IComponentContext> _componentsField;
        private Field<IPersistableObjectCollectionLazyLoader> _lazyLoaderField;
        //private bool _isOrderedCollection;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LazyLoadObjectCollectionPropertyStrategy(
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
            _componentsField = writer.DependencyField<IComponentContext>("$components");
            _lazyLoaderField = writer.Field < IPersistableObjectCollectionLazyLoader>("m_" + MetaProperty.Name + "$lazyLoader");

            _itemContractType = MetaProperty.Relation.RelatedPartyType.ContractType;
            _itemImplementationType = FindImplementationType(_itemContractType);

            using ( TT.CreateScope<TT.TContract>(_itemContractType) )
            {
                _backingField = writer.Field<IList<TT.TContract>>("m_" + MetaProperty.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            using ( TT.CreateScope<TT.TContract, TT.TImpl>(_itemContractType, _itemImplementationType) )
            {
                writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                    p => p.Get(w => {
                        w.Return(Static.GenericFunc(
                            (obj, objlz, lz, val) => DomainModelRuntimeHelpers.LazyLoadObjectCollectionPropertyGetter<TT.TContract, TT.TImpl>(obj, ref objlz, ref lz, ref val),
                            w.This<IDomainObject>(),
                            _context.ThisLazyLoaderField,
                            _lazyLoaderField,
                            _backingField
                        ).CastTo<TT.TProperty>());
                    })
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingImportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            using ( TT.CreateScope<TT.TContract, TT.TImpl>(_itemContractType, _itemImplementationType) )
            {
                _backingField.Assign(
                    Static.GenericFunc((r, v, f, lz) => DomainModelRuntimeHelpers.ImportDomainLazyLoadObjectCollection<TT.TContract, TT.TImpl>(r, v, f, out lz),
                        entityRepo,
                        valueVector.ItemAt(MetaProperty.PropertyIndex),
                        writer.Delegate<TT.TImpl>(w => {
                            w.Return(_context.DomainObjectFactoryField.Func<TT.TContract>(x => x.CreateDomainObjectInstance<TT.TContract>).CastTo<TT.TImpl>());                                    
                        }),
                        _lazyLoaderField
                    )
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingExportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            valueVector.ItemAt(MetaProperty.PropertyIndex).Assign(_backingField.CastTo<object>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
            using ( TT.CreateScope<TT.TContract, TT.TImpl>(_itemContractType, _itemImplementationType) )
            {
                _backingField.Assign(
                    writer.New<ConcreteToAbstractListAdapter<TT.TImpl, TT.TContract>>(
                        writer.New<List<TT.TImpl>>()
                    )
                );
            }
        }

        #endregion
    }
}
