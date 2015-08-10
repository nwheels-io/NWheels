using System;
using System.Data;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Testing.Entities.Impl
{
    public class TestDataRepositoryFactory : DataRepositoryFactoryBase
    {
        private readonly IComponentContext _components;
        private readonly EntityObjectFactory _entityFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestDataRepositoryFactory(
            IComponentContext components, 
            DynamicModule module, 
            TypeMetadataCache metadataCache, 
            TestEntityObjectFactory entityFactory)
            : base(module, metadataCache)
        {
            _components = components;
            _entityFactory = entityFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IApplicationDataRepository NewUnitOfWork(Type repositoryType, bool autoCommit, IsolationLevel? isolationLevel = null)
        {
            return (IApplicationDataRepository)CreateInstanceOf(repositoryType).UsingConstructor(_components, _entityFactory, autoCommit);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return new IObjectFactoryConvention[] {
                new TestDataRepositoryConvention(base.MetadataCache, _entityFactory)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestDataRepositoryConvention : DataRepositoryConvention
        {
            public TestDataRepositoryConvention(TypeMetadataCache metadataCache, EntityObjectFactory entityFactory)
                : base(metadataCache, entityFactory)
            {
                base.RepositoryBaseType = typeof(TestDataRepositoryBase);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override IOperand<IEntityRepository<TypeTemplate.TContract>> GetNewEntityRepositoryExpression(MethodWriterBase writer)
            {
                return writer.New<TestEntityRepository<TypeTemplate.TContract>>(
                    writer.This<DataRepositoryBase>().Prop(x => x.Components), 
                    base.EntityFactoryField, 
                    base.DomainObjectFactoryField);
            }
        }
    }
}
