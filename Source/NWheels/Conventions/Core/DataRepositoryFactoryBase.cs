using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Exceptions;
using NWheels.Extensions;
using System.Reflection;
using Hapil.Members;
using NWheels.Conventions;
using NWheels.DataObjects.Core;
using System.Data;
using System.Linq.Expressions;
using NWheels.Authorization.Core;
using NWheels.Concurrency;
using NWheels.Core;
using NWheels.Entities.Core;
using NWheels.Entities.Impl;
using TT = Hapil.TypeTemplate;
using NWheels.Authorization;
using NWheels.TypeModel;

// ReSharper disable ConvertToLambdaExpression

namespace NWheels.Conventions.Core
{
    public abstract class DataRepositoryFactoryBase : ConventionObjectFactory, IDataRepositoryFactory, IAutoObjectFactory
    {
        private readonly TypeMetadataCache _metadataCache;
        private readonly IStorageInitializer _storage;
        private readonly IFrameworkDatabaseConfig _dbConfiguration;
        private readonly Dictionary<Type, Type> _repositoryContractByEntityContract;
        private readonly Dictionary<Type, IDbConnectionStringResolver> _connectionStringResolverByContractType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DataRepositoryFactoryBase(
            DynamicModule module,
            TypeMetadataCache metadataCache,
            IStorageInitializer storage,
            IFrameworkDatabaseConfig dbConfiguration,
            IEnumerable<IDbConnectionStringResolver> connectionStringResolvers)
            : base(module)
        {
            _metadataCache = metadataCache;
            _storage = storage;
            _dbConfiguration = dbConfiguration;
            _repositoryContractByEntityContract = new Dictionary<Type, Type>();
            _connectionStringResolverByContractType = connectionStringResolvers.ToDictionary(resolver => resolver.DomainContextType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract IApplicationDataRepository NewUnitOfWork(
            IResourceConsumerScopeHandle consumerScope, 
            Type repositoryType, 
            bool autoCommit, 
            UnitOfWorkScopeOption? scopeOption = null,
            string connectionString = null);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //TODO: remove IAutoObjectFactory support 
        public TService CreateService<TService>() where TService : class
        {
            const bool autoCommit = false;
            return (TService)NewUnitOfWork(null, typeof(TService), autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetDataRepositoryContract(Type entityContractType)
        {
            return _repositoryContractByEntityContract.GetOrThrow(
                entityContractType,
                key => new RegistrationMissingException(string.Format(
                    "Entity contract '{0}' could not be found in any of registered repositories.",
                    key.FullName)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ResolveConnectionString(string connectionStringOverride, Type domainContextType)
        {
            if ( !string.IsNullOrEmpty(connectionStringOverride) )
            {
                return connectionStringOverride;
            }

            var connectionConfig = _dbConfiguration.GetContextConnectionConfig(domainContextType);

            if ( connectionConfig != null && !connectionConfig.IsWildcard )
            {
                return connectionConfig.ConnectionString;
            }

            IDbConnectionStringResolver resolver;

            if ( _connectionStringResolverByContractType.TryGetValue(domainContextType, out resolver) )
            {
                var configuredValue = (connectionConfig != null ? connectionConfig.ConnectionString : _dbConfiguration.ConnectionString);
    
                return resolver.ResolveConnectionString(
                    configuredValue, 
                    context: (IAccessControlContext)Session.Current, 
                    storage: _storage);
            }
            else 
            {
                return _dbConfiguration.ConnectionString;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ServiceAncestorMarkerType
        {
            get
            {
                return typeof(IApplicationDataRepository);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataCache MetadataCache
        {
            get { return _metadataCache; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IStorageInitializer Storage
        {
            get { return _storage; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected void UpdateEntityRepositoryMap(Type repositoryContractType, IEnumerable<Type> entityContractTypes)
        {
            foreach ( var entityContract in entityContractTypes )
            {
                _repositoryContractByEntityContract[entityContract] = repositoryContractType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ValidateContractProperty(PropertyInfo property, out Type entityContractType)
        {
            var type = property.PropertyType;

            if ( !type.IsGenericType || type.IsGenericTypeDefinition || 
                (type.GetGenericTypeDefinition() != typeof(IEntityRepository<>) && type.GetGenericTypeDefinition() != typeof(IPartitionedRepository<,>)) )
            {
                throw new ContractConventionException(
                    typeof(DataRepositoryConvention), property.DeclaringType, property, "Property must be of type IEntityRepository<T>");
            }

            if ( property.GetGetMethod() == null || property.GetSetMethod() != null )
            {
                throw new ContractConventionException(
                    typeof(DataRepositoryConvention), property.DeclaringType, property, "Property must be read-only");
            }

            entityContractType = property.PropertyType.GetGenericArguments()[0];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class DataRepositoryConvention : ImplementationConvention
        {
            private readonly TypeMetadataCache _metadataCache;
            private readonly List<EntityInRepository> _entitiesInRepository;
            private readonly HashSet<Type> _entityContractsInRepository;
            private readonly EntityObjectFactory _entityFactory;
            private readonly List<Action<ConstructorWriter>> _initializers;
            private readonly Dictionary<Type, Field<IEntityRepository<TT.TContract>>> _repositoryFieldByContract;
            private DataRepositoryFactoryBase _ownerFactory;
            private Field<EntityObjectFactory> _entityFactoryField;
            private Field<IDomainObjectFactory> _domainObjectFactoryField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected DataRepositoryConvention(TypeMetadataCache metadataCache, EntityObjectFactory entityFactory)
                : base(Will.InspectDeclaration | Will.ImplementBaseClass | Will.ImplementPrimaryInterface)
            {
                _metadataCache = metadataCache;
                _entityFactory = entityFactory;
                _entitiesInRepository = new List<EntityInRepository>();
                _repositoryFieldByContract = new Dictionary<Type, Field<IEntityRepository<TypeTemplate.TContract>>>();
                _entityContractsInRepository = new HashSet<Type>();
                _initializers = new List<Action<ConstructorWriter>>();

                this.RepositoryBaseType = typeof(DataRepositoryBase);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void DecomposeContractProperty(PropertyInfo property, out Type entityContractType)
            {
                entityContractType = property.PropertyType.GetGenericArguments()[0];

                if ( !EntityContractAttribute.IsEntityContract(entityContractType) )
                {
                    throw new ContractConventionException(
                        this,
                        property.DeclaringType,
                        property,
                        "IEntityRepository<T> must specify T which is an entity contract interface");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void DecomposeContractProperty(PropertyInfo property, out Type entityContractType, out Type entityImplementationType)
            {
                DecomposeContractProperty(property, out entityContractType);
                entityImplementationType = _entityFactory.GetOrBuildEntityImplementation(entityContractType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeMetadataCache MetadataCache
            {
                get { return _metadataCache; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityObjectFactory EntityFactory
            {
                get { return _entityFactory; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                _ownerFactory = (DataRepositoryFactoryBase)context.Factory;
                context.BaseType = this.RepositoryBaseType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                ValidateRepositoryContract(writer);
                FindEntitiesInRepository(writer);
                EnsureAllContractsImplemented();
                _ownerFactory.UpdateEntityRepositoryMap(writer.OwnerClass.Key.PrimaryInterface, this.EntitiesInRepository.Select(e => e.ContractType));

                _entityFactoryField = writer.Field<EntityObjectFactory>("EntityFactory", isPublic: true);
                _domainObjectFactoryField = writer.Field<IDomainObjectFactory>("$domainFactory");

                ImplementStaticConstructor(writer);
                ImplementEntityRepositoryProperties(writer);
                ImplementGetOrBuildDbCompiledModel(writer);
                ImplementConstructor(writer);
                ImplementGetEntityTypesInRepository(writer);
                ImplementGetEntityContractsInRepository(writer);
                ImplementGetEntityRepositories(writer);
                ImplementNewEntityMethods(writer);
                ImplementToStringMethod(writer);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementStaticConstructor(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementEntityRepositoryProperties(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                foreach ( var entity in EntitiesInRepository.Where(e => e.Metadata.IsEntity && e.PartitionedRepositoryProperty == null) )
                {
                    ImplementEntityRepositoryProperty(writer, entity);
                }

                foreach ( var entity in EntitiesInRepository.Where(e => e.PartitionedRepositoryProperty != null) )
                {
                    ImplementPartitionedRepositoryProperty(writer, entity);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementEntityRepositoryProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer, EntityInRepository entity)
            {
                if ( entity.RepositoryProperty != null )
                {
                    writer.Property(entity.RepositoryProperty).Implement(p => p.Get(w => w.Return(p.BackingField)));
                }

                Initializers.Add(cw => {
                    using ( TT.CreateScope<TT.TContract, TT.TImpl>(entity.ContractType, entity.ImplementationType) )
                    {
                        var backingField = (
                            entity.RepositoryProperty != null
                            ? writer.OwnerClass.GetPropertyBackingField(entity.RepositoryProperty).AsOperand<IEntityRepository<TT.TContract>>()
                            : writer.Field<IEntityRepository<TT.TContract>>("m_" + entity.Metadata.Name));

                        backingField.Assign(GetNewEntityRepositoryExpression(entity, cw, partitionValue: null));
                        cw.This<DataRepositoryBase>().Void(x => x.RegisterEntityRepository<TT.TContract, TT.TImpl>, backingField);
                        
                        _repositoryFieldByContract[entity.ContractType] = backingField;
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementPartitionedRepositoryProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer, EntityInRepository entity)
            {
                if ( entity.Metadata.PartitionProperty == null )
                {
                    throw new ContractConventionException(
                        typeof(DataRepositoryConvention), entity.ContractType, entity.PartitionedRepositoryProperty,
                        "Entity declared with IPartitionedRepository<,> must have a partition property - use [PropertyContract.Partition] attribute.");
                }

                writer.Property(entity.PartitionedRepositoryProperty).Implement(p => p.Get(w => w.Return(p.BackingField)));

                Initializers.Add(cw => {
                    using ( TT.CreateScope<TT.TContract, TT.TImpl, TT.TIndex1>(
                        entity.ContractType, entity.ImplementationType, entity.Metadata.PartitionProperty.ClrType) )
                    {
                        var backingField = writer.OwnerClass
                            .GetPropertyBackingField(entity.PartitionedRepositoryProperty)
                            .AsOperand<IPartitionedRepository<TT.TContract, TT.TIndex1>>();

                        backingField.Assign(cw.New<PartitionedRepository<TT.TContract, TT.TIndex1>>(
                            cw.Lambda<TT.TIndex1, IEntityRepository<TT.TContract>>(partitionValue => this.GetNewEntityRepositoryExpression(entity, cw, partitionValue)),
                            Static.Func(ResolutionExtensions.Resolve<IDomainContextLogger>, cw.This<DataRepositoryBase>().Prop(x => x.Components))
                        ));

                        cw.This<DataRepositoryBase>().Void(x => x.RegisterPartitionedRepository<TT.TContract, TT.TIndex1>, backingField);

                        var repoField = writer.Field<IEntityRepository<TT.TContract>>("m_" + entity.Metadata.Name);
                        repoField.Assign(GetNewEntityRepositoryExpression(entity, cw, partitionValue: null));
                        cw.This<DataRepositoryBase>().Void(x => x.RegisterEntityRepository<TT.TContract, TT.TImpl>, repoField);

                        _repositoryFieldByContract[entity.ContractType] = repoField;
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementGetOrBuildDbCompiledModel(ImplementationClassWriter<TT.TInterface> writer)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected abstract IOperand<IEntityRepository<TT.TContract>> GetNewEntityRepositoryExpression(
                EntityInRepository entity,
                MethodWriterBase writer,
                IOperand<TT.TIndex1> partitionValue);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementConstructor(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.Constructor<IResourceConsumerScopeHandle, IComponentContext, EntityObjectFactory, bool>(
                    (cw, scope, components, entityFactory, autoCommit) => {
                        cw.Base(scope, components, autoCommit);
                    
                        _entityFactoryField.Assign(entityFactory);
                        _domainObjectFactoryField.Assign(Static.GenericFunc(c => ResolutionExtensions.Resolve<IDomainObjectFactory>(c), components));
                    
                        Initializers.ForEach(init => init(cw));

                        cw.This<DataRepositoryBase>().Void(x => x.EnsureDomainObjectTypesCreated);
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementToStringMethod(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.ImplementBase<object>()
                    .Method<string>(x => x.ToString).Implement(w => {
                        w.Return(w.Const("DomainContext[" + writer.OwnerClass.Key.PrimaryInterface.Name.TrimLead("I") + "]"));        
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementGetEntityTypesInRepository(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.ImplementBase<DataRepositoryBase>()
                    .Method<Type[]>(x => x.GetEntityTypesInRepository)
                    .Implement(m => m.Return(m.NewArray<Type>(constantValues: EntitiesInRepository.Select(e => e.ImplementationType).ToArray())));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementGetEntityContractsInRepository(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.ImplementBase<DataRepositoryBase>()
                    .Method<Type[]>(x => x.GetEntityContractsInRepository)
                    .Implement(m => m.Return(m.NewArray<Type>(constantValues: EntitiesInRepository.Select(e => e.ContractType).ToArray())));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementGetEntityRepositories(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.ImplementBase<DataRepositoryBase>()
                    .Method<IEntityRepository[]>(x => x.GetEntityRepositories)
                    .Implement(m => {
                        var repoArray = m.Local<IEntityRepository[]>();
                        repoArray.Assign(m.NewArray<IEntityRepository>(m.Const(EntitiesInRepository.Count)));

                        for ( int index = 0 ; index < EntitiesInRepository.Count ; index++ )
                        {
                            var entity = EntitiesInRepository[index];

                            //if ( entity.RepositoryProperty != null )
                            //{
                            //    var backingField = writer.OwnerClass.GetPropertyBackingField(entity.RepositoryProperty);
                            //    repoArray.ItemAt(index).Assign(backingField.AsOperand<IEntityRepository>());
                            //}
                            //else if ( entity.PartitionedRepositoryProperty != null )

                            Field<IEntityRepository<TT.TContract>> backingField;

                            if ( _repositoryFieldByContract.TryGetValue(entity.ContractType, out backingField) )
                            {
                                repoArray.ItemAt(index).Assign(backingField.CastTo<IEntityRepository>());
                            }
                            else
                            {
                                repoArray.ItemAt(index).Assign(m.Const<IEntityRepository>(null));
                            }
                        }

                        m.Return(repoArray);
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementNewEntityMethods(ImplementationClassWriter<TT.TInterface> writer)
            {
                writer.AllMethods(IsNewEntityMethod).Implement(m => {
                    var entity = FindEntityInRepositoryByContract(m.OwnerMethod.Signature.ReturnType);
                    var entityImplementationType = EntityFactory.FindImplementationType(entity.ContractType);

                    using ( TT.CreateScope<TT.TImpl>(entityImplementationType) )
                    {
                        var entityObjectLocal = m.Local<TT.TReturn>();
                        entityObjectLocal.Assign(
                            _domainObjectFactoryField.Func<TT.TReturn, TT.TReturn>(x => x.CreateDomainObjectInstance,
                                _entityFactoryField.Func<IComponentContext, TT.TReturn>(x => x.NewEntity<TT.TReturn>, 
                                    m.This<DataRepositoryBase>().Prop(x => x.Components)
                                )
                            )
                        );

                        m.ForEachArgument(arg => {
                            var property = entity.FindEntityContractPropertyOrThrow(arg.Name, arg.OperandType);
                            entityObjectLocal.Prop<TT.TArgument>(property).Assign(arg);
                        });

                        m.Return(entityObjectLocal);
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected EntityInRepository FindEntityInRepositoryByContract(Type contractType)
            {
                var entity = _entitiesInRepository.FirstOrDefault(e => e.ContractType == contractType);

                if ( entity == null )
                {
                    entity = new EntityInRepository(contractType, this);
                    _entitiesInRepository.Add(entity);
                }

                return entity;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected bool IsNewEntityMethod(MethodInfo method)
            {
                var result = (method.Name.StartsWith("New") && (method.ReturnType.IsEntityContract() || method.ReturnType.IsEntityPartContract()));
                //EntityContractsInRepository.Any(contract => contract.IsAssignableFrom(method.ReturnType)));
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected bool MustImplementProperty(PropertyInfo property)
            {
                return (property.DeclaringType != typeof(IApplicationDataRepository) && property.DeclaringType != typeof(IUnitOfWork));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void FindEntitiesInRepository(ImplementationClassWriter<TT.TInterface> writer)
            {
                ScanEntitiesFromRepositoryContract(writer);
                EnsureAllRelatedEntitiesRegistered();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void FindEntityContractsInRepository(ITypeMetadata type)
            {
                if ( !EntityContractsInRepository.Add(type.ContractType) )
                {
                    return;
                }

                foreach ( var property in type.Properties )
                {
                    if ( property.Relation != null && property.Relation.RelatedPartyType != null )
                    {
                        FindEntityContractsInRepository(property.Relation.RelatedPartyType);
                    }
                }

                foreach ( var derivedType in type.DerivedTypes )
                {
                    FindEntityContractsInRepository(derivedType);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            protected Type RepositoryBaseType { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected List<EntityInRepository> EntitiesInRepository
            {
                get { return _entitiesInRepository; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected HashSet<Type> EntityContractsInRepository
            {
                get { return _entityContractsInRepository; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected Field<EntityObjectFactory> EntityFactoryField
            {
                get { return _entityFactoryField; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected Field<IDomainObjectFactory> DomainObjectFactoryField
            {
                get { return _domainObjectFactoryField; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected List<Action<ConstructorWriter>> Initializers
            {
                get { return _initializers; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void EnsureAllContractsImplemented()
            {
                foreach (var entity in EntitiesInRepository)
                {
                    entity.EnsureImplementationType();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void ValidateRepositoryContract(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.AllProperties(MustImplementProperty).ForEach(property => {
                    Type entityContractType;
                    ValidateContractProperty(property, out entityContractType);
                        
                    var metaType = _metadataCache.GetTypeMetadata(entityContractType); // force entity metadata to be created now, if not yet
                    metaType = null;
                });

                _metadataCache.AcceptVisitor(new CrossTypeFixupMetadataVisitor(_metadataCache));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ScanEntitiesFromRepositoryContract(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.AllProperties(MustImplementProperty).ForEach(property => {
                    var entity = new EntityInRepository(property, this);
                    EntitiesInRepository.Add(entity);
                    FindEntityContractsInRepository(entity.Metadata);
                });

                writer.AllMethods(IsNewEntityMethod).ForEach(method => {
                    var entityContractType = method.ReturnType;
                    if ( !EntityContractsInRepository.Contains(entityContractType) )
                    {
                        FindEntityContractsInRepository(_metadataCache.GetTypeMetadata(entityContractType));
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void EnsureAllRelatedEntitiesRegistered()
            {
                foreach ( var contractType in EntityContractsInRepository )
                {
                    FindEntityInRepositoryByContract(contractType);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class ConnectedModelDataRepositoryConvention<TConnection, TModel> : DataRepositoryConvention
            where TConnection : class
            where TModel : class
        {
            private Field<ITypeMetadataCache> _metadataCacheField;
            private MethodMember _methodGetOrBuildDbCompieldModel;
            private Field<TModel> _compiledModelField;
            private Field<object> _compiledModelSyncRootField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected ConnectedModelDataRepositoryConvention(EntityObjectFactory entityFactory, TypeMetadataCache metadataCache)
                : base(metadataCache, entityFactory)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void ImplementStaticConstructor(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.Field("_metadataCache", out _metadataCacheField);
                writer.StaticField("_s_compiledModel", out _compiledModelField);
                writer.StaticField("_s_compiledModelSyncRoot", out _compiledModelSyncRootField);

                writer.StaticConstructor(cw => {
                    _compiledModelSyncRootField.Assign(cw.New<object>());
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of DataRepositoryConvention

            protected override void ImplementConstructor(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.Constructor<IResourceConsumerScopeHandle, IComponentContext, EntityObjectFactory, ITypeMetadataCache, TConnection, bool>(
                    (cw, scope, components, entityFactory, metadata, connection, autoCommit) => {
                        cw.Base(
                            scope, 
                            components,
                            entityFactory,
                            Static.Func<TModel>(_methodGetOrBuildDbCompieldModel, metadata, connection),
                            connection,
                            autoCommit);
                        EntityFactoryField.Assign(entityFactory);
                        DomainObjectFactoryField.Assign(Static.GenericFunc(c => ResolutionExtensions.Resolve<IDomainObjectFactory>(c), components));
                        MetadataCacheField.Assign(metadata);
                        Initializers.ForEach(init => init(cw));
                    });
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void ImplementGetOrBuildDbCompiledModel(ImplementationClassWriter<TT.TInterface> writer)
            {
                writer.NewStaticFunction<ITypeMetadataCache, TConnection, TModel>("GetOrBuildDbCompiledModel", "metadataCache", "connection")
                    .Implement((m, metadataCache, connection) => {
                        _methodGetOrBuildDbCompieldModel = m.OwnerMethod;

                        m.If(_compiledModelField == m.Const<TModel>(null)).Then(() => {
                            m.Lock(_compiledModelSyncRootField, millisecondsTimeout: 10000).Do(() => {
                                m.If(_compiledModelField == m.Const<TModel>(null)).Then(() => {
                                    
                                    ImplementBuildDbCompiledModel(m, metadataCache, connection);

                                });
                            });
                        });

                        m.Return(_compiledModelField);
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected abstract void ImplementBuildDbCompiledModel(
                FunctionMethodWriter<TModel> writer, 
                Operand<ITypeMetadataCache> metadataCache,
                Operand<TConnection> connection);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected Field<ITypeMetadataCache> MetadataCacheField
            {
                get { return _metadataCacheField; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            protected Field<TModel> CompiledModelField
            {
                get { return _compiledModelField; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityInRepository : IDomainContextEntityMetadata
        {
            private readonly DataRepositoryConvention _ownerConvention;
            private readonly Type _contractType;
            private Type _implementationType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityInRepository(PropertyInfo property, DataRepositoryConvention ownerConvention)
            {
                _ownerConvention = ownerConvention;
                ownerConvention.DecomposeContractProperty(property, out _contractType);

                if ( property.PropertyType.GetGenericTypeDefinition() == typeof(IEntityRepository<>) )
                {
                    this.RepositoryProperty = property;
                }
                else if ( property.PropertyType.GetGenericTypeDefinition() == typeof(IPartitionedRepository<,>) )
                {
                    this.PartitionedRepositoryProperty = property;
                }
                else
                {
                    throw new ContractConventionException(
                        typeof(DataRepositoryConvention), 
                        property.DeclaringType, property, 
                        "Domain context properties must be of type IEntityRepository<> or IPartitionedRepository<,>.");
                }

                this.Metadata = ownerConvention.MetadataCache.GetTypeMetadata(_contractType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityInRepository(Type contractType, DataRepositoryConvention ownerConvention)
            {
                _contractType = contractType;
                _ownerConvention = ownerConvention;

                this.RepositoryProperty = null;
                this.PartitionedRepositoryProperty = null;
                this.Metadata = ownerConvention.MetadataCache.GetTypeMetadata(_contractType);
                //this.Metadata.TryGetImplementation(_ownerConvention.EntityFactory.GetType(), out _implementationType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PropertyInfo FindEntityContractPropertyOrThrow(string propertyName, Type propertyType)
            {
                var property = this.Metadata.Properties.FirstOrDefault(p =>
                    p.Name.EqualsIgnoreCase(propertyName) &&
                        p.ClrType.IsAssignableFrom(propertyType));

                if ( property != null )
                {
                    return property.ContractPropertyInfo;
                }

                throw new ContractConventionException(
                    _ownerConvention,
                    _contractType,
                    String.Format("No property matches NewXXXX() method argument '{0}':{1}", propertyName, propertyType.Name));
            }


            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void EnsureImplementationType()
            {
                if ( _implementationType == null )
                {
                    _implementationType = _ownerConvention.EntityFactory.FindImplementationType(_contractType);
                    ((TypeMetadataBuilder)this.Metadata).UpdateImplementation(_ownerConvention.EntityFactory, _implementationType);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of Object

            public override string ToString()
            {
                return Metadata.Name;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PropertyInfo RepositoryProperty { get; private set; }
            public PropertyInfo PartitionedRepositoryProperty { get; private set; }
            public ITypeMetadata Metadata { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            ITypeMetadata IDomainContextEntityMetadata.MetaType
            {
                get { return this.Metadata; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ContractType
            {
                get { return _contractType; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ImplementationType
            {
                get { return _implementationType; }
            }
        }
    }
}
