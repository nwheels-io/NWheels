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
using TT = Hapil.TypeTemplate;
using TT2 = NWheels.Stacks.MongoDb.Factories.PropertyStrategies.PersistableTypeTemplates;

namespace NWheels.Stacks.MongoDb.Factories.PropertyStrategies
{
    public class EmbeddedObjectCollectionPropertyStrategy : PropertyImplementationStrategy
    {
        private Field<TT2.TPersistable[]> _backingField;
        private Type _itemContractType;
        private Type _itemImplementationType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EmbeddedObjectCollectionPropertyStrategy(
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
            //_componentsField = writer.DependencyField<IComponentContext>("$components");

            _itemContractType = MetaProperty.Relation.RelatedPartyType.ContractType;
            _itemImplementationType = FindImplementationType(_itemContractType);

            using ( TT.CreateScope<TT2.TPersistable>(_itemImplementationType) )
            {
                _backingField = writer.Field<TT2.TPersistable[]>("m_" + MetaProperty.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.NewVirtualWritableProperty<TT2.TPersistable[]>(MetaProperty.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingImportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            using ( TT.CreateScope<TT2.TPersistable>(_itemImplementationType) )
            {
                _backingField.Assign(
                    Static.GenericFunc((repo, val) => DomainModelRuntimeHelpers.ImportEmbeddedPersistableCollection<TT2.TPersistable>(repo, val),
                        entityRepo,
                        valueVector.ItemAt(MetaProperty.PropertyIndex)
                    )
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingExportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            using ( TT.CreateScope<TT2.TPersistable>(_itemImplementationType) )
            {
                valueVector.ItemAt(MetaProperty.PropertyIndex).Assign(_backingField.CastTo<object>());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
        }

        #endregion
    }
}
