using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.DataObjects.Core.Factories
{
    public abstract class PropertyImplementationStrategy
    {
        private readonly ObjectFactoryContext _factoryContext;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metaType;
        private readonly IPropertyMetadata _metaProperty;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected PropertyImplementationStrategy(
            ObjectFactoryContext factoryContext,
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
        {
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

        public void WriteInitialization(MethodWriterBase initializationConstructorWriter, Operand<IComponentContext> components)
        {
            OnWritingInitializationConstructor(initializationConstructorWriter, components);
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

        protected Type FindImpementationType(Type contractType)
        {
            var storageTypeKey = new TypeKey(primaryInterface: contractType);
            return _factoryContext.Factory.FindDynamicType(storageTypeKey);
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

        protected virtual void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingMaterializationConstructor(MethodWriterBase writer)
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

            using ( TT.CreateScope<TT.TService>(MetaProperty.DefaultValueGeneratorType) )
            {
                var backingField = cw.OwnerClass.GetPropertyBackingField(MetaProperty.ContractPropertyInfo).AsOperand<TT.TProperty>();
                backingField.Assign(
                    components.Func<TT.TService>(x => x.Resolve<TT.TService>)
                    .CastTo<IPropertyValueGenerator<TT.TProperty>>()
                    .Func<string, TT.TProperty>(x => x.GenerateValue, cw.Const(MetaProperty.ContractQualifiedName)));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type HelpGetConcreteCollectionOpenType(Type abstractCollectionType)
        {
            if ( !abstractCollectionType.IsConstructedGenericType || !abstractCollectionType.IsInterface) 
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
    }
}
