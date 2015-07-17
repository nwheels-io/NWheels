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
using NWheels.Entities.Core;
using TT = Hapil.TypeTemplate;

// ReSharper disable ConvertToLambdaExpression

namespace NWheels.Conventions.Core
{
    public abstract class DataRepositoryFactoryBase : ConventionObjectFactory, IDataRepositoryFactory, IAutoObjectFactory
    {
        private readonly TypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DataRepositoryFactoryBase(
            DynamicModule module,
            TypeMetadataCache metadataCache)
            : base(module)
        {
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract IApplicationDataRepository NewUnitOfWork(Type repositoryType, bool autoCommit, IsolationLevel? isolationLevel = null);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual TRepository NewUnitOfWork<TRepository>(bool autoCommit, IsolationLevel? isolationLevel = null) where TRepository : class, IApplicationDataRepository
        {
            return (TRepository)NewUnitOfWork(typeof(TRepository), autoCommit, isolationLevel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService CreateService<TService>() where TService : class
        {
            const bool autoCommit = false;
            const IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;

            return (TService)NewUnitOfWork(typeof(TService), autoCommit, isolationLevel);
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

        public static void ValidateContractProperty(PropertyInfo property, out Type entityContractType)
        {
            var type = property.PropertyType;

            if ( !type.IsGenericType || type.IsGenericTypeDefinition || type.GetGenericTypeDefinition() != typeof(IEntityRepository<>) )
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
            private Field<EntityObjectFactory> _entityFactoryField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected DataRepositoryConvention(TypeMetadataCache metadataCache, EntityObjectFactory entityFactory)
                : base(Will.InspectDeclaration | Will.ImplementPrimaryInterface)
            {
                _metadataCache = metadataCache;
                _entityFactory = entityFactory;
                _entitiesInRepository = new List<EntityInRepository>();
                _entityContractsInRepository = new HashSet<Type>();
                _initializers = new List<Action<ConstructorWriter>>();

                this.RepositoryBaseType = typeof(DataRepositoryBase);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void DecomposeContractProperty(PropertyInfo property, out Type entityContractType, out Type entityImplementationType)
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
                context.BaseType = this.RepositoryBaseType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                ValidateRepositoryContract(writer);
                FindEntitiesInRepository(writer);

                _entityFactoryField = writer.Field<EntityObjectFactory>("EntityFactory", isPublic: true);

                ImplementStaticConstructor(writer);
                ImplementEntityRepositoryProperties(writer);
                ImplementGetOrBuildDbCompiledModel(writer);
                ImplementConstructor(writer);
                ImplementGetEntityTypesInRepository(writer);
                ImplementGetEntityContractsInRepository(writer);
                ImplementGetEntityRepositories(writer);
                ImplementNewEntityMethods(writer);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementStaticConstructor(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementEntityRepositoryProperties(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                foreach ( var entity in EntitiesInRepository.Where(e => e.RepositoryProperty != null) )
                {
                    ImplementEntityRepositoryProperty(writer, entity);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementEntityRepositoryProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer, EntityInRepository entity)
            {
                writer.Property(entity.RepositoryProperty).Implement(p => p.Get(w => w.Return(p.BackingField)));

                Initializers.Add(cw => {
                    using (TT.CreateScope<TT.TContract, TT.TImpl>(entity.ContractType, entity.ImplementationType))
                    {
                        var backingField = writer.OwnerClass
                            .GetPropertyBackingField(entity.RepositoryProperty)
                            .AsOperand<IEntityRepository<TT.TContract>>();

                        backingField.Assign(GetNewEntityRepositoryExpression(cw));
                        cw.This<DataRepositoryBase>().Void(x => x.RegisterEntityRepository<TT.TContract, TT.TImpl>, backingField);
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementGetOrBuildDbCompiledModel(ImplementationClassWriter<TT.TInterface> writer)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected abstract IOperand<IEntityRepository<TT.TContract>> GetNewEntityRepositoryExpression(MethodWriterBase writer);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual void ImplementConstructor(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.Constructor<EntityObjectFactory, bool>((cw, entityFactory, autoCommit) => {
                    cw.Base(autoCommit);
                    _entityFactoryField.Assign(entityFactory);
                    Initializers.ForEach(init => init(cw));
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

                            if ( entity.RepositoryProperty != null )
                            {
                                var backingField = writer.OwnerClass.GetPropertyBackingField(entity.RepositoryProperty);
                                repoArray.ItemAt(index).Assign(backingField.AsOperand<IEntityRepository>());
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
                    var entityImplementationType = EntityFactory.FindDynamicType(new TypeKey(primaryInterface: entity.ContractType));

                    using ( TT.CreateScope<TT.TImpl>(entityImplementationType) )
                    {
                        var entityObjectLocal = m.Local<TT.TReturn>(initialValue: _entityFactoryField.Func<TT.TReturn>(x => x.NewEntity<TT.TReturn>));

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
                var result = (method.Name.StartsWith("New") && EntityContractsInRepository.Any(contract => contract.IsAssignableFrom(method.ReturnType)));
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
                    if ( property.Relation != null )
                    {
                        FindEntityContractsInRepository(property.Relation.RelatedPartyType);
                    }
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

            protected List<Action<ConstructorWriter>> Initializers
            {
                get { return _initializers; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private void ValidateRepositoryContract(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.AllProperties(MustImplementProperty).ForEach(property => {
                    Type entityContractType;
                    ValidateContractProperty(property, out entityContractType);
                        
                    _metadataCache.GetTypeMetadata(entityContractType); // force entity metadata to be created now, if not yet
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
                writer.Constructor<EntityObjectFactory, ITypeMetadataCache, TConnection, bool>(
                    (cw, entityFactory, metadata, connection, autoCommit) => {
                        cw.Base(
                            entityFactory,
                            Static.Func<TModel>(_methodGetOrBuildDbCompieldModel, metadata, connection),
                            connection,
                            autoCommit);
                        EntityFactoryField.Assign(entityFactory);
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

        public class EntityInRepository
        {
            private readonly DataRepositoryConvention _ownerConvention;
            private readonly Type _contractType;
            private Type _implementationType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityInRepository(PropertyInfo property, DataRepositoryConvention ownerConvention)
            {
                _ownerConvention = ownerConvention;
                ownerConvention.DecomposeContractProperty(property, out _contractType, out _implementationType);

                this.RepositoryProperty = property;
                this.Metadata = ownerConvention.MetadataCache.GetTypeMetadata(_contractType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityInRepository(Type contractType, DataRepositoryConvention ownerConvention)
            {
                _contractType = contractType;
                _ownerConvention = ownerConvention;

                this.RepositoryProperty = null;
                this.Metadata = ownerConvention.MetadataCache.GetTypeMetadata(_contractType);
                this.Metadata.TryGetImplementation(_ownerConvention.EntityFactory.GetType(), out _implementationType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

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
                    String.Format("No property matches NewXXXX() method argument '{0}':{1}", argumentName, argumentType.Name));
            }


            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void EnsureImplementationType()
            {
                if ( _implementationType == null )
                {
                    var implementationTypeKey = new TypeKey(primaryInterface: _contractType);
                    _implementationType = _ownerConvention.EntityFactory.FindDynamicType(implementationTypeKey);
                    ((TypeMetadataBuilder)this.Metadata).UpdateImplementation(_ownerConvention.EntityFactory, _implementationType);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PropertyInfo RepositoryProperty { get; private set; }
            public ITypeMetadata Metadata { get; private set; }

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
