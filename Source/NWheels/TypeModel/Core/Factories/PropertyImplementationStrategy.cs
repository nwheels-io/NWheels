using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.TypeModel;
using TT = Hapil.TypeTemplate;

namespace NWheels.DataObjects.Core.Factories
{
    public abstract class PropertyImplementationStrategy : IPropertyImplementationStrategy
    {
        private readonly PropertyImplementationStrategyMap _ownerMap;
        private readonly ObjectFactoryContext _factoryContext;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metaType;
        private readonly IPropertyMetadata _metaProperty;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected PropertyImplementationStrategy(
            PropertyImplementationStrategyMap ownerMap,
            ObjectFactoryContext factoryContext,
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
        {
            if ( ownerMap == null )
            {
                throw new ArgumentNullException("ownerMap");
            }
            if ( factoryContext == null )
            {
                throw new ArgumentNullException("factoryContext");
            }
            if ( metadataCache == null )
            {
                throw new ArgumentNullException("metadataCache");
            }
            if ( metaType == null )
            {
                throw new ArgumentNullException("metaType");
            }
            if ( metaProperty == null )
            {
                throw new ArgumentNullException("metaProperty");
            }

            _ownerMap = ownerMap;
            _factoryContext = factoryContext;
            _metadataCache = metadataCache;
            _metaType = metaType;
            _metaProperty = metaProperty;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WritePropertyImplementation(ImplementationClassWriter<TT.TInterface> implementationWriter)
        {
            OnBeforeImplementation(implementationWriter);
            OnImplementContractProperty(implementationWriter);
            OnImplementStorageProperty(implementationWriter);
            OnAfterImplementation(implementationWriter);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteInitialization(MethodWriterBase initializationConstructorWriter, Operand<IComponentContext> components, params IOperand[] args)
        {
            OnWritingInitializationConstructor(initializationConstructorWriter, components, args);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteMaterialization(MethodWriterBase materializationConstructorWriter)
        {
            OnWritingMaterializationConstructor(materializationConstructorWriter);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteResolveDependencies(ClassWriterBase classWriter, MethodWriterBase methodWriter, Operand<IComponentContext> components)
        {
            OnWritingResolveDependencies(classWriter, methodWriter, components);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteDeepListNestedObjects(MethodWriterBase writer, Operand<HashSet<object>> nestedObjects)
        {
            OnWritingDeepListNestedObjects(writer, nestedObjects);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteSerializingCallback(MethodWriterBase callbackMethodWriter)
        {
            OnWritingSerializingCallback(callbackMethodWriter);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteDeserializedCallback(MethodWriterBase callbackMethodWriter)
        {
            OnWritingDeserializedCallback(callbackMethodWriter);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteReturnTrueIfModified(FunctionMethodWriter<bool> functionWriter)
        {
            OnWritingReturnTrueIfModified(functionWriter);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteValidation(MethodWriterBase writer)
        {
            OnWritingValidation(writer);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteExportStorageValue(MethodWriterBase methodWriter, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            OnWritingExportStorageValue(methodWriter, entityRepo, valueVector);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteImportStorageValue(MethodWriterBase methodWriter, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            OnWritingImportStorageValue(methodWriter, entityRepo, valueVector);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectFactoryContext FactoryContext
        {
            get
            {
                return _factoryContext;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadataCache MetadataCache
        {
            get
            {
                return _metadataCache;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadata MetaType
        {
            get
            {
                return _metaType;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public IPropertyMetadata MetaProperty
        {
            get
            {
                return _metaProperty;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool HasDependencies
        {
            get
            {
                return OnHasDependencies();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool HasNestedObjects
        {
            get
            {
                return OnHasNestedObjects();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyInfo ImplementedContractProperty { get; protected set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyInfo ImplementedStorageProperty { get; protected set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyInfo ImplementedStorageOrContractProperty
        {
            get
            {
                return ImplementedStorageProperty ?? ImplementedContractProperty;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected Type FindImplementationType(Type contractType)
        {
            var implementationTypeKey = _ownerMap.GetImplementationTypeKey(contractType);
            return _factoryContext.Factory.FindDynamicType(implementationTypeKey);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual bool OnHasDependencies()
        {
            return false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual bool OnHasNestedObjects()
        {
            return false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnBeforeImplementation(ImplementationClassWriter<TT.TInterface> writer)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnImplementContractProperty(ImplementationClassWriter<TT.TInterface> writer);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnImplementStorageProperty(ImplementationClassWriter<TT.TInterface> writer);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnAfterImplementation(ImplementationClassWriter<TT.TInterface> writer)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingMaterializationConstructor(MethodWriterBase writer, params IOperand[] args)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingResolveDependencies(ClassWriterBase classWriter, MethodWriterBase methodWriter, Operand<IComponentContext> components)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingSerializingCallback(MethodWriterBase writer)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingDeserializedCallback(MethodWriterBase writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingReturnTrueIfModified(FunctionMethodWriter<bool> functionWriter)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingValidation(MethodWriterBase writer)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingExportStorageValue(MethodWriterBase methodWriter, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingImportStorageValue(MethodWriterBase methodWriter, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void HelpInitializeDefaultValue(MethodWriterBase constructorWriter, Operand<IComponentContext> components)
        {
            if ( MetaProperty.DefaultValue != null )
            {
                WriteInitializeWithDefaultValue(constructorWriter, components);
            }
            else if ( MetaProperty.DefaultValueGeneratorType != null )
            {
                WriteInitializeWithGeneratedValue(constructorWriter, components);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Field<TypeTemplate.TProperty> HelpGetPropertyBackingField(MethodWriterBase writer)
        {
            return writer.OwnerClass.GetPropertyBackingField(MetaProperty.ContractPropertyInfo).AsOperand<TT.TProperty>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Field<TypeTemplate.TProperty> HelpGetPropertyBackingFieldByName(MethodWriterBase writer)
        {
            return writer.OwnerClass.GetMemberByName<PropertyMember>(MetaProperty.Name).BackingField.AsOperand<TT.TProperty>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void SetMetaPropertyStorageStyle(PropertyStorageStyle storageStyle)
        {
            var writableMetaProperty = (PropertyMetadataBuilder)this.MetaProperty;
            writableMetaProperty.SafeGetRelationalMapping().StorageStyle = storageStyle;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void WriteInitializeWithDefaultValue(MethodWriterBase constructorWriter, Operand<IComponentContext> components)
        {
            var cw = constructorWriter;

            IOperand<TT.TProperty> defaultValue;

            if ( MetaProperty.TryGetDefaultValueOperand(cw, out defaultValue))
            {
                var backingField = cw.OwnerClass.GetPropertyBackingField(MetaProperty.ContractPropertyInfo).AsOperand<TT.TProperty>();
                backingField.Assign(defaultValue);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteInitializeWithGeneratedValue(MethodWriterBase constructorWriter, Operand<IComponentContext> components)
        {
            var cw = constructorWriter;
            var generator = MetadataCache.As<TypeMetadataCache>().GetPropertyValueGeneratorInstance(MetaProperty.DefaultValueGeneratorType);
            var generatorWriter = (generator as IPropertyValueGeneratorWriter);

            var backingField = cw.OwnerClass.GetPropertyBackingField(MetaProperty.ContractPropertyInfo).AsOperand<TT.TProperty>();

            if ( generatorWriter != null )
            {
                // inline value generation code for better performance
                generatorWriter.WriteGenerateValue(MetaProperty.ContractQualifiedName, constructorWriter, backingField);
            }
            else 
            {
                // fall back to resolving generator object from container at runtime, and calling its GenerateValue method
                using ( TT.CreateScope<TT.TService>(MetaProperty.DefaultValueGeneratorType) )
                {
                    backingField.Assign(
                        Static.Func(ResolutionExtensions.Resolve<TT.TService>, components)
                        .CastTo<IPropertyValueGenerator<TT.TProperty>>()
                        .Func<string, TT.TProperty>(x => x.GenerateValue, cw.Const(MetaProperty.ContractQualifiedName)));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type HelpGetConcreteCollectionOpenType(Type abstractCollectionType)
        {
            if ( !abstractCollectionType.IsConstructedGenericType || !abstractCollectionType.IsInterface ) 
            {
                throw new ArgumentException("Expected generic collection interface", "abstractCollectionType");
            }

            var definition = abstractCollectionType.GetGenericTypeDefinition();

            if ( definition == typeof(IList<>) )
            {
                return typeof(List<>);
            }
            else
            {
                return typeof(HashSet<>);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type HelpGetConcreteCollectionType(Type abstractCollectionType, Type elementType)
        {
            if ( !abstractCollectionType.IsConstructedGenericType || !abstractCollectionType.IsInterface )
            {
                throw new ArgumentException("Expected generic collection interface", "abstractCollectionType");
            }

            var definition = abstractCollectionType.GetGenericTypeDefinition();

            if ( definition == typeof(IList<>) )
            {
                return typeof(List<>).MakeGenericType(elementType);
            }
            else
            {
                return typeof(HashSet<>).MakeGenericType(elementType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type HelpGetCollectionAdapterType(
            Type abstractCollectionType, 
            Type abstractElementType, 
            Type concreteElementType, 
            out bool isOrderedCollection)
        {
            if ( !abstractCollectionType.IsConstructedGenericType || !abstractCollectionType.IsInterface )
            {
                throw new ArgumentException("Expected generic collection interface", "abstractCollectionType");
            }

            var definition = abstractCollectionType.GetGenericTypeDefinition();

            if ( definition == typeof(IList<>) )
            {
                isOrderedCollection = true;
                return typeof(ConcreteToAbstractListAdapter<,>).MakeGenericType(concreteElementType, abstractElementType);
            }
            else
            {
                isOrderedCollection = false;
                return typeof(ConcreteToAbstractCollectionAdapter<,>).MakeGenericType(concreteElementType, abstractElementType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetReadAccessorMethodName(IPropertyMetadata property)
        {
            return "ReadPropertyValue_" + property.Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetWriteAccessorMethodName(IPropertyMetadata property)
        {
            return "WritePropertyValue_" + property.Name;
        }
    }
}
