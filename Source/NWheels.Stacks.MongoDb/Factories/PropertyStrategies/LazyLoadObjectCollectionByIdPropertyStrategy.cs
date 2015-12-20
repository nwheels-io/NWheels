using System;
using System.Collections.Generic;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;
using NWheels.Stacks.MongoDb.LazyLoaders;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.MongoDb.Factories.PropertyStrategies
{
    public class LazyLoadObjectCollectionByIdPropertyStrategy : PropertyImplementationStrategy
    {
        private Type _relatedContractType;
        private Type _idType;
        private Field<TT.TKey[]> _backingField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LazyLoadObjectCollectionByIdPropertyStrategy(
            PropertyImplementationStrategyMap ownerMap,
            ObjectFactoryContext factoryContext,
            ITypeMetadataCache metadataCache,
            ITypeMetadata metaType,
            IPropertyMetadata metaProperty)
            : base(ownerMap, factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            _relatedContractType = MetaProperty.Relation.RelatedPartyType.ContractType;
            _idType = MetaProperty.Relation.RelatedPartyType.EntityIdProperty.ClrType;

            using ( TT.CreateScope<TT.TKey>(_idType) )
            {
                _backingField = writer.Field<TT.TKey[]>("m_" + MetaProperty.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingImportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            using ( TT.CreateScope<TT.TKey>(_idType) )
            {
                _backingField.Assign(
                    Static.GenericFunc((v) => PersistableObjectRuntimeHelpers.ImportPersistableLazyLoadObjectCollection<TT.TKey>(v),
                        valueVector.ItemAt(MetaProperty.PropertyIndex)
                    )
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingExportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            using ( TT.CreateScope<TT.TKey>(_idType) )
            {
                valueVector.ItemAt(MetaProperty.PropertyIndex).Assign(
                    writer.New<ObjectCollectionLazyLoaderById<TT.TKey>>(
                        entityRepo.Prop(x => x.OwnerContext).Func<Type, IEntityRepository>(x => x.GetEntityRepository, writer.Const(_relatedContractType)),
                        _backingField
                    )
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
        }

        #endregion
    }
}
