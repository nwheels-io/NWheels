using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using Autofac;
using NWheels.Conventions.Core;
using NWheels.Entities.Core;

namespace NWheels.Stacks.EntityFramework
{
    public abstract class EfDataRepositoryBase : DataRepositoryBase
    {
        private readonly DbCompiledModel _compiledModel;
        private readonly DbConnection _connection;
        private readonly IEntityObjectFactory _entityFactory;
        private ObjectContext _objectContext;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected EfDataRepositoryBase(
            IComponentContext components, 
            IEntityObjectFactory entityFactory, 
            DbCompiledModel compiledModel, 
            DbConnection connection, 
            bool autoCommit)
            : base(components, autoCommit)
        {
            _entityFactory = entityFactory;
            _compiledModel = compiledModel;
            _connection = connection;
            _objectContext = compiledModel.CreateObjectContext<ObjectContext>(connection);
            _objectContext.ContextOptions.ProxyCreationEnabled = false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Dispose()
        {
            _objectContext.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DbCompiledModel CompiledModel
        {
            get
            {
                return _compiledModel;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEntityObjectFactory EntityFactory
        {
            get { return _entityFactory; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ObjectSet<T> CreateObjectSet<T>() where T : class
        {
            return _objectContext.CreateObjectSet<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ObjectContext ObjectContext
        {
            get
            {
                return _objectContext;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnCommitChanges()
        {
            _objectContext.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnRollbackChanges()
        {
            _objectContext.Dispose();
            _connection.Dispose();
        }
    }
}
