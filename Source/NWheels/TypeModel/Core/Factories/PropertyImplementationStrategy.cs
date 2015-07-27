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

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Can be null; if null, this instance is a prototype, otherwise this instance is a property-specific clone
        /// </summary>
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

            _factoryContext = factoryContext;
            _metadataCache = metadataCache;
            _metaType = metaType;
            _metaProperty = metaProperty;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ShouldApply(IPropertyMetadata metaProperty)
        {
            ValidateCurrentInstanceIsPrototype();
            return OnShouldApply(metaProperty);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyImplementationStrategy Clone(IPropertyMetadata metaProperty)
        {
            ValidateCurrentInstanceIsPrototype();
            return OnClone(metaProperty);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WritePropertyImplementation(ClassWriterBase classWriter)
        {
            ValidateCurrentInstanceIsClone();
            
            OnBeforeImplementation(classWriter);
            OnImplementContractProperty(classWriter);
            OnImplementStorageProperty(classWriter);
            OnAfterImplementation(classWriter);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteInitialization(MethodWriterBase initializationConstructorWriter, IOperand<IComponentContext> components)
        {
            ValidateCurrentInstanceIsClone();
            OnWritingInitializationConstructor(initializationConstructorWriter, components);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteMaterialization(MethodWriterBase materializationConstructorWriter)
        {
            ValidateCurrentInstanceIsClone();
            OnWritingMaterializationConstructor(materializationConstructorWriter);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteResolveDependencies(MethodWriterBase writer, IOperand<IComponentContext> components)
        {
            ValidateCurrentInstanceIsClone();
            OnWritingResolveDependencies(writer, components);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
            ValidateCurrentInstanceIsClone();
            OnWritingDeepListNestedObjects(writer, nestedObjects);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteSerializingCallback(MethodWriterBase callbackMethodWriter)
        {
            ValidateCurrentInstanceIsClone();
            OnWritingSerializingCallback(callbackMethodWriter);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteDeserializedCallback(MethodWriterBase callbackMethodWriter)
        {
            ValidateCurrentInstanceIsClone();
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

        public bool IsPrototype
        {
            get
            {
                return (_metaProperty == null);
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

        protected Type FindStorageType(Type contractType)
        {
            var storageTypeKey = new TypeKey(primaryInterface: contractType);
            return _factoryContext.Factory.FindDynamicType(storageTypeKey);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract bool OnShouldApply(IPropertyMetadata metaProperty);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract PropertyImplementationStrategy OnClone(IPropertyMetadata metaProperty);

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

        protected virtual void OnBeforeImplementation(ClassWriterBase writer)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected abstract void OnImplementContractProperty(ClassWriterBase writer);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnImplementStorageProperty(ClassWriterBase writer);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnAfterImplementation(ClassWriterBase writer)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingInitializationConstructor(MethodWriterBase writer, IOperand<IComponentContext> components)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingMaterializationConstructor(MethodWriterBase writer)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnWritingResolveDependencies(MethodWriterBase writer, IOperand<IComponentContext> components)
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

        private void ValidateCurrentInstanceIsPrototype()
        {
            if ( !IsPrototype )
            {
                throw new InvalidOperationException("Current instance is a clone, not a prototype. Requested operation is only allowed on prototype instances.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateCurrentInstanceIsClone()
        {
            if ( IsPrototype )
            {
                throw new InvalidOperationException("Current instance is a prototype, not a clone. Requested operation is only allowed on clone instances.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type GetConcreteCollectionType(Type abstractCollectionType, Type elementType)
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
