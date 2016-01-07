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
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.TypeModel.Core.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class DictionaryStrategy : DualValueStrategy
    {
        private readonly MongoEntityObjectFactory.ConventionContext _conventionContext;
        private Type _contractKeyType;
        private Type _contractValueType;
        private Type _storageKeyType;
        private Type _storageValueType;
        private Field<IComponentContext> _componentsField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DictionaryStrategy(
            MongoEntityObjectFactory.ConventionContext conventionContext,
            PropertyImplementationStrategyMap ownerMap,
            ObjectFactoryContext factoryContext,
            ITypeMetadataCache metadataCache,
            ITypeMetadata metaType,
            IPropertyMetadata metaProperty)
            : base(ownerMap, factoryContext, metadataCache, metaType, metaProperty, storageType: GetDictionaryStorageType(metadataCache, metaProperty, ownerMap, factoryContext))
        {
            _conventionContext = conventionContext;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBeforeImplementation(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            base.OnBeforeImplementation(writer);

            var contractGenericArguments = MetaProperty.ClrType.GetGenericArguments();
            var storageGenericArguments = base.StorageType.GetGenericArguments();

            _contractKeyType = contractGenericArguments[0];
            _contractValueType = contractGenericArguments[1];
            _storageKeyType = storageGenericArguments[0];
            _storageValueType = storageGenericArguments[1];

            _componentsField = writer.DependencyField<IComponentContext>("$components");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DualValueStrategy

        protected override void OnWritingContractToStorageConversion(
            MethodWriterBase method, 
            IOperand<TT.TProperty> contractValue, 
            MutableOperand<TT.TValue> storageValue)
        {
            using ( TT.CreateScope<TT.TContract, TT.TContract2, TT.TImpl, TT.TImpl2>(_contractKeyType, _contractValueType, _storageKeyType, _storageValueType) )
            {
                storageValue.Assign(
                    Static.Func(
                        CopyDictionaryContractToStorage<TT.TContract, TT.TContract2, TT.TImpl, TT.TImpl2>,
                        _componentsField,
                        contractValue.CastTo<Dictionary<TT.TContract, TT.TContract2>>()
                    )
                    .CastTo<TT.TValue>()
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingStorageToContractConversion(
            MethodWriterBase method, 
            MutableOperand<TT.TProperty> contractValue, 
            IOperand<TT.TValue> storageValue)
        {
            using ( TT.CreateScope<TT.TContract, TT.TContract2, TT.TImpl, TT.TImpl2>(_contractKeyType, _contractValueType, _storageKeyType, _storageValueType) )
            {
                contractValue.Assign(
                    Static.Func(
                        CopyDictionaryStorageToContract<TT.TContract, TT.TContract2, TT.TImpl, TT.TImpl2>,
                        _componentsField,
                        storageValue.CastTo<Dictionary<TT.TImpl, TT.TImpl2>>()
                    )
                    .CastTo<TT.TProperty>()
                );
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Dictionary<TSK, TSV> CopyDictionaryContractToStorage<TCK, TCV, TSK, TSV>(IComponentContext components, Dictionary<TCK, TCV> contract)
        {
            if ( contract == null )
            {
                return null;
            }

            var result = new Dictionary<TSK, TSV>(contract.Count);

            foreach ( var kvp in contract )
            {
                result.Add(
                    kvp.Key is IDomainObject ? (TSK)kvp.Key.As<IDomainObject>().As<IPersistableObject>() : (TSK)(object)kvp.Key, 
                    kvp.Value is IDomainObject ? (TSV)kvp.Value.As<IDomainObject>().As<IPersistableObject>() : (TSV)(object)kvp.Value);
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Dictionary<TCK, TCV> CopyDictionaryStorageToContract<TCK, TCV, TSK, TSV>(IComponentContext components, Dictionary<TSK, TSV> storage)
        {
            if ( storage == null )
            {
                return null;
            }

            var result = new Dictionary<TCK, TCV>(storage.Count);

            foreach ( var kvp in storage )
            {
                var persistableKey = (kvp.Key as IPersistableObject);
                var persistableValue = (kvp.Value as IPersistableObject);

                if ( persistableKey != null )
                {
                    ((IHaveDependencies)persistableKey).InjectDependencies(components);
                    persistableKey.EnsureDomainObject();
                }

                if ( persistableValue != null )
                {
                    ((IHaveDependencies)persistableValue).InjectDependencies(components);
                    persistableValue.EnsureDomainObject();
                }

                result.Add(
                    persistableKey != null ? (TCK)persistableKey.GetContainerObject() : (TCK)(object)kvp.Key,
                    persistableValue != null ? (TCV)persistableValue.GetContainerObject() : (TCV)(object)kvp.Value);
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Type GetDictionaryStorageType(
            ITypeMetadataCache metadataCache, 
            IPropertyMetadata metaProperty, 
            PropertyImplementationStrategyMap ownerMap, 
            ObjectFactoryContext factoryContext)
        {
            var contractKeyType = metaProperty.ClrType.GetGenericArguments()[0];
            var contractValueType = metaProperty.ClrType.GetGenericArguments()[1];

            var storageKeyType = (contractKeyType.IsEntityPartContract() ? GetImplementationType(contractKeyType, ownerMap, factoryContext) : contractKeyType);
            var storageValueType = (contractValueType.IsEntityPartContract() ? GetImplementationType(contractValueType, ownerMap, factoryContext) : contractValueType);

            return typeof(Dictionary<,>).MakeGenericType(storageKeyType, storageValueType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Type GetImplementationType(Type contractType, PropertyImplementationStrategyMap ownerMap, ObjectFactoryContext factoryContext)
        {
            var implementationTypeKey = ownerMap.GetImplementationTypeKey(contractType);
            return factoryContext.Factory.FindDynamicType(implementationTypeKey);
        }
    }
}
