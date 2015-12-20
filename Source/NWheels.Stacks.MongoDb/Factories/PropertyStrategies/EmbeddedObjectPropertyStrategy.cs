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

namespace NWheels.Stacks.MongoDb.Factories.PropertyStrategies
{
    public class EmbeddedObjectPropertyStrategy : PropertyImplementationStrategy
    {
        private Type _objectImplementationType;
        private Field<TT2.TPersistable> _backingField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EmbeddedObjectPropertyStrategy(
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
            _objectImplementationType = FindImplementationType(MetaProperty.ClrType);

            using ( TT.CreateScope<TT2.TPersistable>(_objectImplementationType) )
            {
                _backingField = writer.Field<TT2.TPersistable>("m_" + MetaProperty.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            using ( TT.CreateScope<TT2.TPersistable>(_objectImplementationType) )
            {
                writer.NewVirtualWritableProperty<TT2.TPersistable>(MetaProperty.Name).ImplementAutomatic(_backingField);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingImportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            using ( TT.CreateScope<TT2.TPersistable>(_objectImplementationType) )
            {
                _backingField.Assign(
                    Static.GenericFunc((r, v) => DomainModelRuntimeHelpers.ImportEmbeddedPersistableObject<TT2.TPersistable>(r, v), 
                        entityRepo, 
                        valueVector.ItemAt(MetaProperty.PropertyIndex))
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingExportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            using ( TT.CreateScope<TT2.TPersistable>(_objectImplementationType) )
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
