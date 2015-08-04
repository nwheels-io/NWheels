using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Reflection;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Stacks.EntityFramework.EFConventions;
using TT = Hapil.TypeTemplate;

// ReSharper disable ConvertToLambdaExpression

namespace NWheels.Stacks.EntityFramework.Factories
{
    public class EfDataRepositoryFactory : DataRepositoryFactoryBase
    {
        private readonly IComponentContext _components;
        private readonly DbProviderFactory _dbProvider;
        private readonly IFrameworkDatabaseConfig _config;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly EntityObjectFactory _entityFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfDataRepositoryFactory(
            IComponentContext components,
            DynamicModule module,
            EfEntityObjectFactory entityFactory,
            TypeMetadataCache metadataCache,
            DbProviderFactory dbProvider = null,
            Auto<IFrameworkDatabaseConfig> config = null)
            : base(module, metadataCache)
        {
            _components = components;
            _entityFactory = entityFactory;
            _dbProvider = dbProvider;
            _config = (config != null ? config.Instance : null);
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IApplicationDataRepository NewUnitOfWork(Type repositoryType, bool autoCommit, IsolationLevel? isolationLevel = null)
        {
            var connection = _dbProvider.CreateConnection();
            connection.ConnectionString = _config.ConnectionString;
            connection.Open();

            return (IApplicationDataRepository)CreateInstanceOf(repositoryType).UsingConstructor(_components, _entityFactory, _metadataCache, connection, autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return new IObjectFactoryConvention[] {
                new EfDataRepositoryConvention(_entityFactory, base.MetadataCache)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EfDataRepositoryConvention : ConnectedModelDataRepositoryConvention<DbConnection, DbCompiledModel>
        {
            public EfDataRepositoryConvention(EntityObjectFactory entityFactory, TypeMetadataCache metadataCache)
                : base(entityFactory, metadataCache)
            {
                this.RepositoryBaseType = typeof(EfDataRepositoryBase);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void ImplementBuildDbCompiledModel(
                FunctionMethodWriter<DbCompiledModel> writer,
                Operand<ITypeMetadataCache> metadataCache,
                Operand<DbConnection> connection)
            {
                var m = writer;
                var modelBuilderLocal = m.Local<DbModelBuilder>(initialValue: m.New<DbModelBuilder>());
                
                //modelBuilderLocal.Prop(x => x.Conventions).Void(x => x.Add, m.NewArray<IConvention>(values:
                //    m.New<NoUnderscoreForeignKeyNamingConvention>()
                //));

                foreach ( var entity in base.EntitiesInRepository )
                {
                    entity.EnsureImplementationType();

                    var entityConfigurationMethod = entity.ImplementationType.GetMethod("ConfigureEfModel", BindingFlags.Public | BindingFlags.Static);
                    Static.Void(entityConfigurationMethod, metadataCache, modelBuilderLocal);
                }

                var modelLocal = m.Local(initialValue: modelBuilderLocal.Func<DbConnection, DbModel>(x => x.Build, connection));
                base.CompiledModelField.Assign(modelLocal.Func<DbCompiledModel>(x => x.Compile));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override IOperand<IEntityRepository<TT.TContract>> GetNewEntityRepositoryExpression(MethodWriterBase writer)
            {
                return writer.New<EfEntityRepository<TT.TContract, TT.TImpl>>(writer.This<EfDataRepositoryBase>());
            }
        }
    }
}
