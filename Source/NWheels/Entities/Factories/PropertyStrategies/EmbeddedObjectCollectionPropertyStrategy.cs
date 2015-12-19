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
    public class EmbeddedObjectCollectionPropertyStrategy : PropertyImplementationStrategy
    {
        private readonly DomainObjectFactoryContext _context;
        private Type _itemImplementationType;
        private Type _itemContractType;
        //private Type _collectionAdapterType;
        private Field<TT.TProperty> _backingField;
        private Field<IComponentContext> _componentsField;
        //private bool _isOrderedCollection;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EmbeddedObjectCollectionPropertyStrategy(
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

            _itemContractType = MetaProperty.Relation.RelatedPartyType.ContractType;
            _itemImplementationType = FindImplementationType(_itemContractType);
            //_collectionAdapterType = HelpGetCollectionAdapterType(
            //    MetaProperty.ClrType,
            //    MetaProperty.Relation.RelatedPartyType.ContractType,
            //    _itemImplementationType,
            //    out _isOrderedCollection);

            _backingField = writer.Field<TT.TProperty>("m_" + MetaProperty.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                p => p.Get(w => {
                    w.Return(Static.GenericFunc((obj, lz, val) => DomainModelRuntimeHelpers.PropertyGetter<TT.TProperty>(obj, ref lz, ref val),
                        w.This<IDomainObject>(),
                        _context.ThisLazyLoaderField,
                        _backingField
                    ));
                })
            );
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
                    Static.GenericFunc((r, v, f) => DomainModelRuntimeHelpers.ImportEmbeddedDomainCollection<TT.TContract, TT.TImpl>(r, v, f),
                        entityRepo,
                        valueVector.ItemAt(MetaProperty.PropertyIndex),
                        writer.Delegate<TT.TImpl>(w => {
                            w.Return(_context.DomainObjectFactoryField.Func<TT.TContract>(x => x.CreateDomainObjectInstance<TT.TContract>).CastTo<TT.TImpl>());
                        })
                    )
                    .CastTo<TT.TProperty>()
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
                    .CastTo<TT.TProperty>()
                );
            }
        }

        #endregion
    }
}
