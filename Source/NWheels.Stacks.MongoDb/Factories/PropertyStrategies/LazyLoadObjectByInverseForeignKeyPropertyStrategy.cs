using System;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;
using TT = Hapil.TypeTemplate;
using TT2 = NWheels.Stacks.MongoDb.Factories.PropertyStrategies.PersistableTypeTemplates;
using NWheels.Stacks.MongoDb.LazyLoaders;

namespace NWheels.Stacks.MongoDb.Factories.PropertyStrategies
{
    public class LazyLoadObjectByInverseForeignKeyPropertyStrategy : PropertyImplementationStrategy
    {
        private Type _relatedContractType;
        private IPropertyMetadata _inverseForeignKeyProperty;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LazyLoadObjectByInverseForeignKeyPropertyStrategy(
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
            _inverseForeignKeyProperty = MetaProperty.Relation.InverseProperty;
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingExportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            valueVector.ItemAt(MetaProperty.PropertyIndex).Assign(
                writer.New<ObjectLazyLoaderByForeignKey>(
                    entityRepo.Prop(x => x.OwnerContext).Func<Type, IEntityRepository>(x => x.GetEntityRepository, writer.Const(_relatedContractType)),
                    writer.Const(_inverseForeignKeyProperty.Name),
                    writer.This<IPersistableObject>().Prop(x => x.EntityId)
                )
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
        }

        #endregion
    }
}
