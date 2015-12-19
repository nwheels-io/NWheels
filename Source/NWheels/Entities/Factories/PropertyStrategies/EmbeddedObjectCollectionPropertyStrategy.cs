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
using NWheels.DataObjects.Core.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories.PropertyStrategies
{
    public class EmbeddedObjectCollectionPropertyStrategy : PropertyImplementationStrategy
    {
        private readonly DomainObjectFactoryContext _context;
        private Type _objectImplementationType;
        private Type _concreteCollectionType;
        private Type _collectionAdapterType;
        private Field<TT.TConcrete> _concreteCollectionField;
        private bool _isOrderedCollection;

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
            _objectImplementationType = FindImplementationType(MetaProperty.Relation.RelatedPartyType.ContractType);
            _concreteCollectionType = HelpGetConcreteCollectionType(MetaProperty.ClrType, _objectImplementationType);
            _collectionAdapterType = HelpGetCollectionAdapterType(
                MetaProperty.ClrType,
                MetaProperty.Relation.RelatedPartyType.ContractType,
                _objectImplementationType,
                out _isOrderedCollection);

            using ( TT.CreateScope<TT.TConcrete>(_concreteCollectionType) )
            {
                _concreteCollectionField = writer.Field<TT.TConcrete>("m_" + MetaProperty.Name + "$concrete");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.Property(MetaProperty.ContractPropertyInfo).ImplementAutomatic();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingImportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            var backingField = HelpGetPropertyBackingField(writer);

            using ( TT.CreateScope<TT.TAbstract, TT.TConcrete>(_collectionAdapterType, _concreteCollectionType) )
            {
                _concreteCollectionField.Assign(valueVector.ItemAt(MetaProperty.PropertyIndex).CastTo<TT.TConcrete>());
                backingField.Assign(writer.New<TT.TAbstract>(_concreteCollectionField).CastTo<TT.TProperty>());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingExportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            valueVector.ItemAt(MetaProperty.PropertyIndex).Assign(_concreteCollectionField.CastTo<object>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
            var backingField = HelpGetPropertyBackingField(writer);

            using ( TT.CreateScope<TT.TAbstract, TT.TConcrete>(_collectionAdapterType, _concreteCollectionType) )
            {
                _concreteCollectionField.Assign(writer.New<TT.TConcrete>());
                backingField.Assign(writer.New<TT.TAbstract>(_concreteCollectionField).CastTo<TT.TProperty>());
            }
        }

        #endregion
    }
}
