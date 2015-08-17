using System;
using System.Data;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NWheels.Concurrency;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using TT = Hapil.TypeTemplate;

// ReSharper disable ConvertToLambdaExpression

namespace NWheels.Stacks.MongoDb.Factories
{
    public class MongoDataRepositoryFactory : DataRepositoryFactoryBase
    {
        private readonly IComponentContext _components;
        private readonly IFrameworkDatabaseConfig _config;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly MongoEntityObjectFactory _entityFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoDataRepositoryFactory(
            IComponentContext components,
            DynamicModule module,
            MongoEntityObjectFactory entityFactory,
            TypeMetadataCache metadataCache,
            IFrameworkDatabaseConfig config = null)
            : base(module, metadataCache)
        {
            _components = components;
            _entityFactory = entityFactory;
            _config = config;
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IApplicationDataRepository NewUnitOfWork(IResourceConsumerScopeHandle consumerScope, Type repositoryType, bool autoCommit, IsolationLevel? isolationLevel = null)
        {
            var connectionString = new MongoConnectionStringBuilder(_config.ConnectionString);
            var client = new MongoClient(_config.ConnectionString);
            var server = client.GetServer();
            var database = server.GetDatabase(connectionString.DatabaseName);

            return (IApplicationDataRepository)CreateInstanceOf(repositoryType).UsingConstructor(
                consumerScope,
                _components, 
                _entityFactory, 
                _metadataCache, 
                database, 
                autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return new IObjectFactoryConvention[] {
                new MongoDataRepositoryConvention(_entityFactory, base.MetadataCache)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MongoDataRepositoryConvention : ConnectedModelDataRepositoryConvention<MongoDatabase, object>
        {
            public MongoDataRepositoryConvention(EntityObjectFactory entityFactory, TypeMetadataCache metadataCache)
                : base(entityFactory, metadataCache)
            {
                this.RepositoryBaseType = typeof(MongoDataRepositoryBase);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void ImplementBuildDbCompiledModel(
                FunctionMethodWriter<object> writer,
                Operand<ITypeMetadataCache> metadataCache,
                Operand<MongoDatabase> connection)
            {
                WritePolymorphicEntityRegistrations(writer);
                base.CompiledModelField.Assign(writer.New<object>());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override IOperand<IEntityRepository<TT.TContract>> GetNewEntityRepositoryExpression(MethodWriterBase writer)
            {
                return writer.New<MongoEntityRepository<TT.TContract, TT.TImpl>>(
                    // ownerRepo
                    writer.This<MongoDataRepositoryBase>(),                             
                    // metadataCache
                    base.MetadataCacheField,
                    // objectFactory
                    base.EntityFactoryField);                                           
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WritePolymorphicEntityRegistrations(FunctionMethodWriter<object> writer)
            {
                foreach ( var entity in base.EntitiesInRepository )
                {
                    if ( entity.Metadata.BaseType != null || entity.Metadata.DerivedTypes.Count > 0 )
                    {
                        using ( TT.CreateScope<TT.TImpl>(entity.ImplementationType) )
                        {
                            Static.GenericFunc<BsonClassMap<TT.TImpl>>(() => BsonClassMap.RegisterClassMap<TT.TImpl>());
                        }
                        //Static.Func(BsonClassMap.LookupClassMap, writer.Const(entity.ImplementationType));
                    }
                }
            }
        }
    }


    #if false
    public class EfDataRepositoryFactory : ConventionObjectFactory, IDataRepositoryFactory, IAutoObjectFactory
    {
        private readonly DbProviderFactory _dbProvider;
        private readonly IFrameworkDatabaseConfig _config;
        private readonly ITypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfDataRepositoryFactory(
            DynamicModule module, 
            EntityObjectFactory entityFactory, 
            ITypeMetadataCache metadataCache, 
            DbProviderFactory dbProvider = null,
            Auto<IFrameworkDatabaseConfig> config = null)
            : base(module, context => new IObjectFactoryConvention[] { new DataRepositoryConvention(entityFactory, metadataCache) })
        {
            _dbProvider = dbProvider;
            _config = config.Instance;
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository CreateDataRepository<TRepo>(DbConnection connection, bool autoCommit) where TRepo : IApplicationDataRepository
        {
            return CreateInstanceOf<TRepo>().UsingConstructor(_metadataCache, connection, autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public IApplicationDataRepository NewUnitOfWork(Type repositoryType, bool autoCommit, IsolationLevel? isolationLevel = null)
        {
            var connection = _dbProvider.CreateConnection();
            connection.ConnectionString = _config.ConnectionString;
            connection.Open();

            return (IApplicationDataRepository)CreateInstanceOf(repositoryType).UsingConstructor(_metadataCache, connection, autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit, IsolationLevel? isolationLevel = null) where TRepository : class, IApplicationDataRepository
        {
            var connection = _dbProvider.CreateConnection();
            connection.ConnectionString = _config.ConnectionString;
            connection.Open();

            return CreateInstanceOf<TRepository>().UsingConstructor(_metadataCache, connection, autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService CreateService<TService>() where TService : class
        {
            const bool autoCommit = false;
            
            var connection = _dbProvider.CreateConnection();
            connection.ConnectionString = _config.ConnectionString;
            connection.Open();

            return CreateInstanceOf<TService>().UsingConstructor(_metadataCache, connection, autoCommit);
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

        private class DataRepositoryConvention : ImplementationConvention
        {
            private readonly EntityObjectFactory _entityFactory;
            private readonly ITypeMetadataCache _metadataCache;
            private readonly List<Action<ConstructorWriter>> _initializers;
            private readonly List<EntityInRepository> _entitiesInRepository;
            private readonly HashSet<Type> _entityContractsInRepository;
            private MethodMember _methodGetOrBuildDbCompieldModel;
            private Field<DbCompiledModel> _compiledModelField;
            private Field<object> _compiledModelSyncRootField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DataRepositoryConvention(EntityObjectFactory entityFactory, ITypeMetadataCache metadataCache)
                : base(Will.InspectDeclaration | Will.ImplementPrimaryInterface)
            {
                _metadataCache = metadataCache;
                _entityFactory = entityFactory;
                _initializers = new List<Action<ConstructorWriter>>();
                _entitiesInRepository = new List<EntityInRepository>();
                _entityContractsInRepository = new HashSet<Type>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                context.BaseType = typeof(EfDataRepositoryBase);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                FindEntitiesInRepository(writer);

                ImplementStaticConstructor(writer);
                ImplementEntityRepositoryProperties(writer);
                ImplementGetOrBuildDbCompiledModel(writer);
                ImplementConstructor(writer);
                ImplementGetEntityTypesInRepository(writer);
                ImplementNewEntityMethods(writer);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FindEntitiesInRepository(ImplementationClassWriter<TT.TInterface> writer)
            {
                writer.AllProperties(MustImplementProperty).ForEach(property => {
                    var entity = new EntityInRepository(property, this);
                    _entitiesInRepository.Add(entity);
                    FindEntityContractsInRepository(entity.Metadata);
                });

                foreach ( var contractType in _entityContractsInRepository )
                {
                    FindEntityInRepositoryByContract(contractType);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FindEntityContractsInRepository(ITypeMetadata type)
            {
                if ( !_entityContractsInRepository.Add(type.ContractType) )
                {
                    return;
                }

                foreach ( var property in type.Properties )
                {
                    if ( property.Relation != null )
                    {
                        FindEntityContractsInRepository(property.Relation.RelatedPartyType);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementGetOrBuildDbCompiledModel(ImplementationClassWriter<TT.TInterface> writer)
            {
                writer.NewStaticFunction<ITypeMetadataCache, DbConnection, DbCompiledModel>("GetOrBuildDbCompiledModel", "metadataCache", "connection")
                    .Implement((m, metadataCache, connection) => {
                        _methodGetOrBuildDbCompieldModel = m.OwnerMethod;

                        m.If(_compiledModelField == m.Const<DbCompiledModel>(null)).Then(() => {
                            m.Lock(_compiledModelSyncRootField, millisecondsTimeout: 10000).Do(() => {
                                m.If(_compiledModelField == m.Const<DbCompiledModel>(null)).Then(() => {

                                    var modelBuilderLocal = m.Local<DbModelBuilder>(initialValue: m.New<DbModelBuilder>());
                                    modelBuilderLocal.Prop(x => x.Conventions).Void(x => x.Add, m.NewArray<IConvention>(values: 
                                        m.New<NoUnderscoreForeignKeyNamingConvention>()
                                    ));

                                    var typeMetadataLocal = m.Local<ITypeMetadata>();
                                    var entityTypeConfigurationLocal = m.Local<object>();

                                    foreach ( var entity in _entitiesInRepository )
                                    {
                                        entity.EnsureImplementationType();

                                        var entityConfigurationWriter = new EfEntityConfigurationWriter(
                                            entity.Metadata, 
                                            m, 
                                            modelBuilderLocal, 
                                            metadataCache,
                                            typeMetadataLocal,
                                            entityTypeConfigurationLocal);

                                        entityConfigurationWriter.WriteEntityTypeConfiguration();
                                    }

                                    var modelLocal = m.Local(initialValue: modelBuilderLocal.Func<DbConnection, DbModel>(x => x.Build, connection));
                                    _compiledModelField.Assign(modelLocal.Func<DbCompiledModel>(x => x.Compile));
                                });
                            });
                        });

                        m.Return(_compiledModelField);
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementStaticConstructor(ImplementationClassWriter<TypeTemplate.TInterface> writer) 
            {
                writer.StaticField("_s_compiledModel", out _compiledModelField);
                writer.StaticField("_s_compiledModelSyncRoot", out _compiledModelSyncRootField);

                writer.StaticConstructor(cw => {
                    _compiledModelSyncRootField.Assign(cw.New<object>());
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementConstructor(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.Constructor<ITypeMetadataCache, DbConnection, bool>(
                    (cw, metadata, connection, autoCommit) => {
                        cw.Base(
                            Static.Func<DbCompiledModel>(_methodGetOrBuildDbCompieldModel, metadata, connection), 
                            connection, 
                            autoCommit);
                        _initializers.ForEach(init => init(cw));
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityRepositoryProperties(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                foreach ( var entity in _entitiesInRepository.Where(e => e.RepositoryProperty != null) )
                {
                    ImplementEntityRepositoryProperty(writer, entity);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityRepositoryProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer, EntityInRepository entity)
            {
                writer.Property(entity.RepositoryProperty).Implement(p => p.Get(w => w.Return(p.BackingField)));

                _initializers.Add(cw => {
                    using ( TT.CreateScope<TT.TContract, TT.TImpl>(entity.ContractType, entity.ImplementationType) )
                    {
                        writer.OwnerClass.GetPropertyBackingField(entity.RepositoryProperty)
                            .AsOperand<IEntityRepository<TT.TContract>>()
                            .Assign(cw.New<EfEntityRepository<TT.TContract, TT.TImpl>>(cw.This<EfDataRepositoryBase>()));
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementGetEntityTypesInRepository(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.ImplementBase<EfDataRepositoryBase>()
                    .Method<IEnumerable<Type>>(x => x.GetEntityTypesInRepository)
                    .Implement(m => m.Return(m.NewArray<Type>(constantValues: _entitiesInRepository.Select(e => e.ImplementationType).ToArray())));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementNewEntityMethods(ImplementationClassWriter<TT.TInterface> writer)
            {
                writer.AllMethods(IsNewEntityMethod).Implement(m => {
                    var entity = FindEntityInRepositoryByContract(m.OwnerMethod.Signature.ReturnType);
                    var entityImplementationType = _entityFactory.FindDynamicType(new TypeKey(primaryInterface: entity.ContractType));

                    using ( TT.CreateScope<TT.TImpl>(entityImplementationType) )
                    {
                        var enttityObjectLocal = m.Local<TT.TReturn>(initialValue: m.New<TT.TImpl>().CastTo<TT.TReturn>());

                        m.ForEachArgument(arg => {
                            var property = entity.FindEntityContractPropertyOrThrow(arg.Name, arg.OperandType);
                            enttityObjectLocal.Prop<TT.TArgument>(property).Assign(arg);
                        });

                        m.Return(enttityObjectLocal);
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private EntityInRepository FindEntityInRepositoryByContract(Type contractType)
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

            private bool MustImplementProperty(PropertyInfo property)
            {
                return (property.DeclaringType != typeof(IApplicationDataRepository) && property.DeclaringType != typeof(IUnitOfWork));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsNewEntityMethod(MethodInfo method)
            {
                var result = (method.Name.StartsWith("New") && _entityContractsInRepository.Contains(method.ReturnType));
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ValidateContractProperty(PropertyInfo property, out Type entityContractType, out Type entityImplementationType)
            {
                var type = property.PropertyType;

                if ( !type.IsGenericType || type.IsGenericTypeDefinition || type.GetGenericTypeDefinition() != typeof(IEntityRepository<>) )
                {
                    throw new ContractConventionException(
                        this, property.DeclaringType, property, "Property must be of type IEntityRepository<T>");
                }

                if ( property.GetGetMethod() == null || property.GetSetMethod() != null )
                {
                    throw new ContractConventionException(
                        this, property.DeclaringType, property, "Property must be read-only");
                }

                entityContractType = property.PropertyType.GetGenericArguments()[0];

                if ( !EntityContractAttribute.IsEntityContract(entityContractType) )
                {
                    throw new ContractConventionException(
                        this, property.DeclaringType, property, "IEntityRepository<T> must specify T which is an entity contract interface");
                }

                entityImplementationType = _entityFactory.GetOrBuildEntityImplementation(entityContractType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private class EntityInRepository
            {
                private readonly DataRepositoryConvention _ownerConvention;
                private readonly Type _contractType;
                private Type _implementationType;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public EntityInRepository(PropertyInfo property, DataRepositoryConvention ownerConvention)
                {
                    _ownerConvention = ownerConvention;
                    ownerConvention.ValidateContractProperty(property, out _contractType, out _implementationType);

                    this.RepositoryProperty = property;
                    this.Metadata = ownerConvention._metadataCache.GetTypeMetadata(_contractType);
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public EntityInRepository(Type contractType, DataRepositoryConvention ownerConvention)
                {
                    _contractType = contractType;
                    _ownerConvention = ownerConvention;

                    this.RepositoryProperty = null;
                    this.Metadata = ownerConvention._metadataCache.GetTypeMetadata(_contractType);

                    _implementationType = this.Metadata.ImplementationType;
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public PropertyInfo FindEntityContractPropertyOrThrow(string argumentName, Type argumentType)
                {
                    var property = this.Metadata.Properties.FirstOrDefault(p => 
                        p.Name.EqualsIgnoreCase(argumentName) && 
                        p.ClrType.IsAssignableFrom(argumentType));

                    if ( property != null )
                    {
                        return property.ContractPropertyInfo;
                    }

                    throw new ContractConventionException(
                        _ownerConvention,
                        _contractType,
                        string.Format("No property matches NewXXXX() method argument '{0}':{1}", argumentName, argumentType.Name));
                }


                //---------------------------------------------------------------------------------------------------------------------------------------------
                
                public void EnsureImplementationType()
                {
                    if ( _implementationType == null )
                    {
                        var implementationTypeKey = new TypeKey(primaryInterface: _contractType);
                        _implementationType = _ownerConvention._entityFactory.FindDynamicType(implementationTypeKey);
                        ((TypeMetadataBuilder)this.Metadata).UpdateImplementation(_implementationType);
                    }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public PropertyInfo RepositoryProperty { get; private set; }
                public ITypeMetadata Metadata { get; private set; }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public Type ContractType
                {
                    get { return _contractType; }
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public Type ImplementationType
                {
                    get { return _implementationType; }
                }
            }
        }
    }

    #endif
}
