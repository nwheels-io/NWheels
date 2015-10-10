using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Autofac;
using NWheels.Concurrency;
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
            IResourceConsumerScopeHandle consumerScope,
            IComponentContext components, 
            IEntityObjectFactory entityFactory, 
            DbCompiledModel compiledModel, 
            DbConnection connection, 
            bool autoCommit)
            : base(consumerScope, components, autoCommit)
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
            bool shouldDisposeResourcesNow;
            base.DisposeConsumerScope(out shouldDisposeResourcesNow);

            if ( shouldDisposeResourcesNow )
            {
                _objectContext.Dispose();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void InitializeCurrentSchema()
        {
            var script = _objectContext.CreateDatabaseScript();

            using ( var command = _connection.CreateCommand() )
            {
                command.CommandType = CommandType.Text;
                command.CommandText = script;

                Console.WriteLine(script);

                command.CommandTimeout = 120000;
                command.ExecuteNonQuery();
            }
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

        public override IEntityObjectFactory PersistableObjectFactory
        {
            get { return _entityFactory; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ObjectSet<T> CreateObjectSet<T>() where T : class
        {
            return _objectContext.CreateObjectSet<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected ObjectContext ObjectContext
        {
            get
            {
                return _objectContext;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<IEntityObject> GetCurrentChangeSet()
        {
            var changeEntityStates = 
                System.Data.Entity.EntityState.Added | 
                System.Data.Entity.EntityState.Modified | 
                System.Data.Entity.EntityState.Deleted;

            var changeSet = _objectContext.ObjectStateManager.GetObjectStateEntries(changeEntityStates)
                .Where(entry => entry.Entity != null)
                .Select(entry => entry.Entity)
                .Cast<IEntityObject>();

            return changeSet;
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
