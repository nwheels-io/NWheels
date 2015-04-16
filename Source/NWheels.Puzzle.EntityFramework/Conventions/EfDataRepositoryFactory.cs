using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
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
using NWheels.Puzzle.EntityFramework.EFConventions;
using NWheels.Puzzle.EntityFramework.Impl;
using System.Reflection;
using TT = Hapil.TypeTemplate;
using Hapil.Members;
using NWheels.Conventions;
using NWheels.DataObjects.Core;
using System.Data;
using System.Linq.Expressions;

// ReSharper disable ConvertToLambdaExpression

namespace NWheels.Puzzle.EntityFramework.Conventions
{
    public class EfDataRepositoryFactory : ConventionObjectFactory, IDataRepositoryFactory, IAutoObjectFactory
    {
        private readonly DbProviderFactory _dbProvider;
        private readonly IFrameworkDatabaseConfig _config;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfDataRepositoryFactory(
            DynamicModule module, 
            EfEntityObjectFactory entityFactory, 
            ITypeMetadataCache metadataCache, 
            DbProviderFactory dbProvider = null,
            Auto<IFrameworkDatabaseConfig> config = null)
            : base(module, context => new IObjectFactoryConvention[] { new DataRepositoryConvention(entityFactory, metadataCache) })
        {
            _dbProvider = dbProvider;
            _config = config.Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository CreateDataRepository<TRepo>(DbConnection connection, bool autoCommit) where TRepo : IApplicationDataRepository
        {
            return CreateInstanceOf<TRepo>().UsingConstructor(connection, autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public IApplicationDataRepository NewUnitOfWork(Type repositoryType, bool autoCommit, IsolationLevel? isolationLevel = null)
        {
            var connection = _dbProvider.CreateConnection();
            connection.ConnectionString = _config.ConnectionString;
            connection.Open();

            return (IApplicationDataRepository)CreateInstanceOf(repositoryType).UsingConstructor<DbConnection, bool>(connection, autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TRepository NewUnitOfWork<TRepository>(bool autoCommit, IsolationLevel? isolationLevel = null) where TRepository : class, IApplicationDataRepository
        {
            var connection = _dbProvider.CreateConnection();
            connection.ConnectionString = _config.ConnectionString;
            connection.Open();

            return CreateInstanceOf<TRepository>().UsingConstructor<DbConnection, bool>(connection, autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService CreateService<TService>() where TService : class
        {
            const bool autoCommit = false;
            
            var connection = _dbProvider.CreateConnection();
            connection.ConnectionString = _config.ConnectionString;
            connection.Open();

            return CreateInstanceOf<TService>().UsingConstructor<DbConnection, bool>(connection, autoCommit);
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
            private readonly EfEntityObjectFactory _entityFactory;
            private readonly ITypeMetadataCache _metadataCache;
            private readonly List<Action<ConstructorWriter>> _initializers;
            private readonly List<EntityInRepository> _entitiesInRepository;
            private readonly HashSet<Type> _entityContractsInRepository;
            private MethodMember _methodGetOrBuildDbCompieldModel;
            private Field<DbCompiledModel> _compiledModelField;
            private Field<object> _compiledModelSyncRootField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DataRepositoryConvention(EfEntityObjectFactory entityFactory, ITypeMetadataCache metadataCache)
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
                writer.NewStaticFunction<DbConnection, DbCompiledModel>("GetOrBuildDbCompiledModel", "connection").Implement((m, connection) => {
                    _methodGetOrBuildDbCompieldModel = m.OwnerMethod;

                    m.If(_compiledModelField == m.Const<DbCompiledModel>(null)).Then(() => {
                        m.Lock(_compiledModelSyncRootField, millisecondsTimeout: 10000).Do(() => {
                            m.If(_compiledModelField == m.Const<DbCompiledModel>(null)).Then(() => {

                                var modelBuilderLocal = m.Local<DbModelBuilder>(initialValue: m.New<DbModelBuilder>());
                                modelBuilderLocal.Prop(x => x.Conventions).Void(x => x.Add, m.NewArray<IConvention>(values: 
                                    m.New<NoUnderscoreForeignKeyNamingConvention>()
                                ));

                                var parameterExpressionLocal = m.Local<ParameterExpression>();

                                foreach ( var entity in _entitiesInRepository )
                                {
                                    entity.EnsureImplementationType();

                                    var entityConfigurationWriter = new EfEntityConfigurationWriter(
                                        entity.Metadata, 
                                        m, 
                                        modelBuilderLocal, 
                                        parameterExpressionLocal);
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
                writer.Constructor<DbConnection, bool>(
                    (cw, connection, autoCommit) => {
                        cw.Base(
                            Static.Func<DbCompiledModel>(_methodGetOrBuildDbCompieldModel, connection), 
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
}
