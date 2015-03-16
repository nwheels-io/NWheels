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
using NWheels.Puzzle.EntityFramework.EFConventions;
using NWheels.Puzzle.EntityFramework.Impl;
using System.Reflection;
using TT = Hapil.TypeTemplate;
using Hapil.Members;

// ReSharper disable ConvertToLambdaExpression

namespace NWheels.Puzzle.EntityFramework.Conventions
{
    public class EfDataRepositoryFactory : ConventionObjectFactory
    {
        public EfDataRepositoryFactory(DynamicModule module, EfEntityObjectFactory entityFactory, ITypeMetadataCache metadataCache)
            : base(module, context => new IObjectFactoryConvention[] { new DataRepositoryConvention(entityFactory, metadataCache) })
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository CreateDataRepository<TRepo>(DbConnection connection, bool autoCommit) where TRepo : IApplicationDataRepository
        {
            return CreateInstanceOf<TRepo>().UsingConstructor(connection, autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class DataRepositoryConvention : ImplementationConvention
        {
            private readonly EfEntityObjectFactory _entityFactory;
            private readonly ITypeMetadataCache _metadataCache;
            private readonly List<Action<ConstructorWriter>> _initializers;
            private readonly List<EntityInRepository> _entitiesInRepository;
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
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                context.BaseType = typeof(EntityFrameworkDataRepositoryBase);
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

                writer.AllMethods(m => m.DeclaringType == TT.Resolve<TT.TInterface>()).Throw<NotImplementedException>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FindEntitiesInRepository(ImplementationClassWriter<TT.TInterface> writer)
            {
                writer.AllProperties(MustImplementProperty).ForEach(property => {
                    _entitiesInRepository.Add(new EntityInRepository(property, this));
                });
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

                                foreach ( var entity in _entitiesInRepository )
                                {
                                    var entityConfigurationWriter = new EfEntityConfigurationWriter(entity.Metadata, m, modelBuilderLocal);
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
                foreach ( var entity in _entitiesInRepository )
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
                            .Assign(cw.New<EntityFrameworkEntityRepository<TT.TContract, TT.TImpl>>(cw.This<EntityFrameworkDataRepositoryBase>()));
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementGetEntityTypesInRepository(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.ImplementBase<EntityFrameworkDataRepositoryBase>()
                    .Method<IEnumerable<Type>>(x => x.GetEntityTypesInRepository)
                    .Implement(m => m.Return(m.NewArray<Type>(constantValues: _entitiesInRepository.Select(e => e.ImplementationType).ToArray())));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool MustImplementProperty(PropertyInfo property)
            {
                return (property.DeclaringType != typeof(IApplicationDataRepository) && property.DeclaringType != typeof(IUnitOfWork));
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
                private readonly Type _contractType;
                private readonly Type _implementationType;

                //---------------------------------------------------------------------------------------------------------------------------------------------

                public EntityInRepository(PropertyInfo property, DataRepositoryConvention owner)
                {
                    owner.ValidateContractProperty(property, out _contractType, out _implementationType);

                    this.RepositoryProperty = property;
                    this.Metadata = owner._metadataCache.GetTypeMetadata(_contractType);
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
