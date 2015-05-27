using System;
using System.Data;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Testing.Entities.Impl
{
    public class TestDataRepositoryFactory : DataRepositoryFactoryBase
    {
        private readonly EntityObjectFactory _entityFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestDataRepositoryFactory(DynamicModule module, ITypeMetadataCache metadataCache, EntityObjectFactory entityFactory)
            : base(module, metadataCache)
        {
            _entityFactory = entityFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IApplicationDataRepository NewUnitOfWork(Type repositoryType, bool autoCommit, IsolationLevel? isolationLevel = null)
        {
            return (IApplicationDataRepository)CreateInstanceOf(repositoryType).UsingConstructor(_entityFactory, autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return new IObjectFactoryConvention[] {
                new TestEntityDataRepositoryConvention(base.MetadataCache, _entityFactory)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestEntityDataRepositoryConvention : DataRepositoryConvention
        {
            public TestEntityDataRepositoryConvention(ITypeMetadataCache metadataCache, EntityObjectFactory entityFactory)
                : base(metadataCache, entityFactory)
            {
                base.RepositoryBaseType = typeof(TestDataRepositoryBase);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override IOperand<IEntityRepository<TypeTemplate.TContract>> GetNewEntityRepositoryExpression(MethodWriterBase writer)
            {
                return writer.New<TestEntityRepository<TypeTemplate.TContract>>(base.EntityFactoryField);
            }
        }
    }
}
