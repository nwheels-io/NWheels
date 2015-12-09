using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace NWheels.DataObjects.Core
{
    public class TypeMetadataCache : ITypeMetadataCache
    {
        private readonly IComponentContext _components;
        private readonly ConcurrentDictionary<Type, TypeMetadataBuilder> _metadataByContractType = new ConcurrentDictionary<Type, TypeMetadataBuilder>();
        private readonly ConcurrentDictionary<string, TypeMetadataBuilder> _metadataByQualifiedName = new ConcurrentDictionary<string, TypeMetadataBuilder>();
        private readonly ConcurrentDictionary<Type, TypeMetadataBuilder> _metadataByImplementationType = new ConcurrentDictionary<Type, TypeMetadataBuilder>();
        private readonly ConcurrentDictionary<Type, ISemanticDataType> _semanticDataTypes;
        private readonly ConcurrentDictionary<Type, IStorageDataType> _storageDataTypes;
        private MetadataConventionSet _conventions;
        private Dictionary<Type, MixinRegistration[]> _mixinsByPrimaryContract = null;
        private Dictionary<Type, ConcretizationRegistration> _concretizationsByPrimaryContract = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataCache(IComponentContext components, MetadataConventionSet conventions)
        {
            _components = components;

            _conventions = conventions;
            _conventions.InjectCache(this);

            _semanticDataTypes = new ConcurrentDictionary<Type, ISemanticDataType>();
            _storageDataTypes = new ConcurrentDictionary<Type, IStorageDataType>();

            EnsureExtensibilityRegistrations();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadata GetMetaTypeByImplementation(Type implementationType)
        {
            ITypeMetadata metaType;

            if ( TryGetMetaTypeByImplementation(implementationType, out metaType) )
            {
                return metaType;
            }

            throw new KeyNotFoundException(string.Format("Implementation type '{0}' is not registered in the metadata cache.", implementationType.FullName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ContainsTypeMetadata(Type primaryContract)
        {
            return _metadataByContractType.ContainsKey(primaryContract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadata GetTypeMetadata(Type primaryContract)
        {
            return _metadataByContractType.GetOrAdd(primaryContract, BuildTypeMetadata);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadata GetTypeMetadata(string qualifiedName)
        {
            return _metadataByQualifiedName[qualifiedName];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetTypeMetadata(Type primaryContract, out ITypeMetadata metadata)
        {
            TypeMetadataBuilder metadataBuilder;
            var result = _metadataByContractType.TryGetValue(primaryContract, out metadataBuilder);
            metadata = metadataBuilder;
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetMetaTypeByImplementation(Type implementationType, out ITypeMetadata metadata)
        {
            TypeMetadataBuilder metaTypeBuilder;

            if ( _metadataByImplementationType.TryGetValue(implementationType, out metaTypeBuilder) )
            {
                metadata = metaTypeBuilder;
                return true;
            }
            else
            {
                metadata = null;
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IPropertyMetadata> GetIncomingRelations(ITypeMetadata targetType, Func<IPropertyMetadata, bool> sourcePredicate = null)
        {
            return _metadataByContractType.Values
                .SelectMany(type => type.Properties.Where(p => p.Kind == PropertyKind.Relation && p.Relation != null && p.Relation.RelatedPartyType == targetType))
                .Distinct()
                .Where(p => sourcePredicate == null || sourcePredicate(p));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureRelationalMapping(ITypeMetadata type)
        {
            var metadata = (TypeMetadataBuilder)type;
            metadata.EnsureRelationalMapping(_conventions);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type Concretize(Type contract)
        {
            ConcretizationRegistration concretization;

            if ( _concretizationsByPrimaryContract.TryGetValue(contract, out concretization) )
            {
                return concretization.ConcreteContract;
            }
            else
            {
                return contract;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ISemanticDataType GetSemanticTypeInstance(Type semanticDataType, Type propertyClrType)
        {
            Type closedSemanticDataType = (
                semanticDataType.IsGenericType && semanticDataType.IsGenericTypeDefinition ?
                semanticDataType.MakeGenericType(propertyClrType) :
                semanticDataType);

            return _semanticDataTypes.GetOrAdd(closedSemanticDataType, key => (ISemanticDataType)Activator.CreateInstance(key));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IStorageDataType GetStorageTypeInstance(Type storageDataType, Type propertyClrType)
        {
            Type closedStorageDataType = (
                storageDataType.IsGenericType && storageDataType.IsGenericTypeDefinition ?
                storageDataType.MakeGenericType(propertyClrType) :
                storageDataType);

            return _storageDataTypes.GetOrAdd(closedStorageDataType, key => (IStorageDataType)Activator.CreateInstance(key));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IPropertyValueGenerator GetPropertyValueGeneratorInstance(Type valueGeneratorType)
        {
            return (IPropertyValueGenerator)_components.Resolve(valueGeneratorType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IPropertyValueGenerator<T> GetPropertyValueGeneratorInstance<T>(Type valueGeneratorType)
        {
            return (IPropertyValueGenerator<T>)_components.Resolve(valueGeneratorType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AcceptVisitor(ITypeMetadataVisitor visitor, Func<TypeMetadataBuilder, bool> predicate = null)
        {
            AcceptVisitor(() => visitor, predicate);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AcceptVisitor(Func<ITypeMetadataVisitor> visitorFactory, Func<TypeMetadataBuilder, bool> predicate = null)
        {
            var snapshotOfTypesInCache = _metadataByContractType.ToArray().Select(kvp => kvp.Value).ToArray();

            foreach ( var type in snapshotOfTypesInCache.Where(type => predicate == null || predicate(type)) )
            {
                type.AcceptVisitor(visitorFactory());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InvalidateExtensibilityRegistrations()
        {
            _mixinsByPrimaryContract = null;
            _concretizationsByPrimaryContract = null;
            
            _conventions = _components.Resolve<MetadataConventionSet>();
            _conventions.InjectCache(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MetadataConventionSet Conventions
        {
            get { return _conventions; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal Snapshot TakeSnapshot()
        {
            return Snapshot.Create(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal TypeMetadataBuilder FindTypeMetadataAllowIncomplete(Type contract)
        {
            TypeMetadataBuilder metadata;
            var entriesBeingBuilt = _s_entriesBeingBuilt;

            if ( entriesBeingBuilt != null && entriesBeingBuilt.TryGetValue(contract, out metadata) )
            {
                return metadata;
            }

            return (TypeMetadataBuilder)GetTypeMetadata(contract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void UpdateMetaTypeImplementation(TypeMetadataBuilder metaType, Type implementationType)
        {
            _metadataByImplementationType.TryAdd(implementationType, metaType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnsureExtensibilityRegistrations()
        {
            if ( _mixinsByPrimaryContract == null || _concretizationsByPrimaryContract == null )
            {
                var mixinRegistrations = _components.Resolve<IEnumerable<MixinRegistration>>();
                var concretizationRegistrations = _components.Resolve<IEnumerable<ConcretizationRegistration>>();

                _mixinsByPrimaryContract = mixinRegistrations.GroupBy(r => r.TargetContract).ToDictionary(g => g.Key, g => g.ToArray());

                BuildMergedConcretizations(concretizationRegistrations);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BuildMergedConcretizations(IEnumerable<ConcretizationRegistration> concretizationRegistrations)
        {
            _concretizationsByPrimaryContract = new Dictionary<Type, ConcretizationRegistration>();
            var concretizationGroupsByGeneralContract = concretizationRegistrations.GroupBy(r => r.GeneralContract).ToList();

            foreach ( var concretizationGroup in concretizationGroupsByGeneralContract )
            {
                ConcretizationRegistration mergedConcretization = null;

                foreach ( var concretization in concretizationGroup )
                {
                    mergedConcretization = (mergedConcretization == null ? concretization : mergedConcretization.Merge(concretization));
                }

                _concretizationsByPrimaryContract.Add(concretizationGroup.Key, mergedConcretization);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMetadataBuilder BuildTypeMetadata(Type primaryContract)
        {
            EnsureExtensibilityRegistrations();

            var mixinContracts = GetRegisteredMixinContracts(primaryContract);
            var metaType = BuildTypeMetadata(primaryContract, mixinContracts);

            _metadataByQualifiedName[metaType.QualifiedName] = metaType;

            return metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMetadataBuilder BuildTypeMetadata(Type primaryContract, Type[] mixinContracts)
        {
            var entriesBeingBuilt = _s_entriesBeingBuilt;
            var ownEntriesBeingBuilt = (entriesBeingBuilt == null);
            var newEntryInserted = false;

            if ( ownEntriesBeingBuilt )
            {
                entriesBeingBuilt = new Dictionary<Type, TypeMetadataBuilder>();
                _s_entriesBeingBuilt = entriesBeingBuilt;
            }

            try
            {
                var builder = new TypeMetadataBuilder(this);
                newEntryInserted = !entriesBeingBuilt.ContainsKey(primaryContract);
                entriesBeingBuilt[primaryContract] = builder;

                return BuildTypeMetadata(primaryContract, mixinContracts, builder);
            }
            finally
            {
                if ( ownEntriesBeingBuilt )
                {
                    _s_entriesBeingBuilt = null;
                }
                else if ( newEntryInserted )
                {
                    entriesBeingBuilt.Remove(primaryContract);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMetadataBuilder BuildTypeMetadata(Type primaryContract, Type[] mixinContracts, TypeMetadataBuilder builder)
        {
            TypeMetadataBuilder metaType;
            ConcretizationRegistration concretization;

            if ( _concretizationsByPrimaryContract.TryGetValue(primaryContract, out concretization) && concretization.ConcreteContract != primaryContract )
            {
                var concretizationMixinContracts = GetRegisteredMixinContracts(concretization.ConcreteContract);
                
                metaType = BuildTypeMetadata(
                    concretization.ConcreteContract, 
                    mixinContracts.Union(concretizationMixinContracts).ToArray());
            }
            else
            {
                var constructor = new TypeMetadataBuilderConstructor(_conventions);
                Type[] addedMixinContracts;

                if ( constructor.ConstructMetadata(primaryContract, mixinContracts, builder, this, out addedMixinContracts) && addedMixinContracts.Length == 0 )
                {
                    metaType = builder;
                }
                else
                {
                    metaType = BuildTypeMetadata(primaryContract, mixinContracts.Union(addedMixinContracts).ToArray());
                }
            }

            if ( concretization != null && concretization.DomainObject != null && metaType.DomainObjectType == null )
            {
                metaType.DomainObjectType = concretization.DomainObject;
            }

            return metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type[] GetRegisteredMixinContracts(Type primaryContract)
        {
            MixinRegistration[] mixinRegistrations;

            if ( _mixinsByPrimaryContract.TryGetValue(primaryContract, out mixinRegistrations) )
            {
                return mixinRegistrations.Select(r => r.MixinContract).ToArray();
            }
            else
            {
                return Type.EmptyTypes;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ThreadStatic]
        private static Dictionary<Type, TypeMetadataBuilder> _s_entriesBeingBuilt;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class Snapshot
        {
            public Dictionary<string, object> Contracts { get; set; }
            public List<string> SemanticDataTypes { get; set; }
            public List<string> StorageDataTypes { get; set; }
            public List<string> Concretizations { get; set; }
            public List<string> Mixins { get; set; }
            public List<string> MetadataConventions { get; set; }
            public List<string> RelationalMappingConventions { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static Snapshot Create(TypeMetadataCache cache)
            {
                return new Snapshot() {
                    Contracts = 
                        cache._metadataByContractType.ToDictionary(kvp => kvp.Key.FullName, kvp => (object)kvp.Value),
                    SemanticDataTypes = 
                        cache._semanticDataTypes.Values.Select(v => v.GetType().FullName).OrderBy(s => s).ToList(),
                    StorageDataTypes = 
                        cache._storageDataTypes.Values.Select(v => v.GetType().FullName).OrderBy(s => s).ToList(),
                    Concretizations = 
                        cache._concretizationsByPrimaryContract.Values
                        .Select(v => string.Format("{0} --|> {1}", v.ConcreteContract.FullName, v.GeneralContract.FullName))
                        .ToList(),
                    Mixins = 
                        cache._mixinsByPrimaryContract.Values
                        .SelectMany(r => r).Select(r => string.Format("{0} += {1}", r.TargetContract.FullName, r.MixinContract.FullName))
                        .ToList(),
                    MetadataConventions = 
                        cache._conventions.MetadataConventions.Select(c => c.GetType().FullName).OrderBy(s => s).ToList(),
                    RelationalMappingConventions = 
                        cache._conventions.RelationalMappingConventions.Select(c => c.GetType().FullName).OrderBy(s => s).ToList()
                };
            }
        }
    }
}
