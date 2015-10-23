using System;
using System.Collections.Generic;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Exceptions;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.Stacks.MongoDb.Factories
{
    public abstract class LazyLoadByForeignKeyStrategyBase : PropertyImplementationStrategy
    {
        private Field<TypeTemplate.TProperty> _valueField;
        private Field<DualValueStates> _stateField;
        private Field<IComponentContext> _componentsField;
        private IPropertyMetadata _thisKeyProperty;
        private IPropertyMetadata _foreignKeyProperty;
        private Type _relatedContractType;
        private Type _relatedImplementationType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected LazyLoadByForeignKeyStrategyBase(
            PropertyImplementationStrategyMap ownerMap,
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(ownerMap, factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBeforeImplementation(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            _componentsField = writer.DependencyField<IComponentContext>("$components");
            _valueField = writer.Field<TypeTemplate.TProperty>("m_" + MetaProperty.Name);
            _stateField = writer.Field<DualValueStates>("m_" + MetaProperty.Name + "$state");

            _relatedContractType = MetaProperty.Relation.RelatedPartyType.ContractType;
            _relatedImplementationType = base.FactoryContext.Factory.FindDynamicType(_relatedContractType);

            _thisKeyProperty = MetaProperty.DeclaringContract.PrimaryKey.Properties[0];
            _foreignKeyProperty = MetaProperty.Relation.InverseProperty;

            if ( _foreignKeyProperty == null )
            {
                throw new ContractConventionException(
                    this.GetType(), 
                    MetaProperty.DeclaringContract.ContractType, 
                    MetaProperty.ContractPropertyInfo,
                    "Cannot generate lazy load by foreign key because inverse property is not defined for relation.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
            writer.If(_stateField == DualValueStates.Contract).Then(() => {
                Static.Void(RuntimeTypeModelHelpers.DeepListNestedObject, _valueField, nestedObjects);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected Field<TypeTemplate.TProperty> ValueField
        {
            get { return _valueField; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Field<DualValueStates> StateField
        {
            get { return _stateField; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Field<IComponentContext> ComponentsField
        {
            get { return _componentsField; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected IPropertyMetadata ThisKeyProperty
        {
            get { return _thisKeyProperty; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected IPropertyMetadata ForeignKeyProperty
        {
            get { return _foreignKeyProperty; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Type RelatedContractType
        {
            get { return _relatedContractType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Type RelatedImplementationType
        {
            get { return _relatedImplementationType; }
        }
    }
}