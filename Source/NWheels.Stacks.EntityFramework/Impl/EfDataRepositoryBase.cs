using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using NWheels.Stacks.EntityFramework.Conventions;
using System.Reflection;
using Autofac;
using Hapil;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.Entities.Core;

namespace NWheels.Stacks.EntityFramework.Impl
{
    public abstract class EfDataRepositoryBase : DataRepositoryBase
    {
        private readonly DbCompiledModel _compiledModel;
        private readonly DbConnection _connection;
        private readonly IEntityObjectFactory _entityFactory;
        private ObjectContext _objectContext;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected EfDataRepositoryBase(IEntityObjectFactory entityFactory, DbCompiledModel compiledModel, DbConnection connection, bool autoCommit)
            : base(autoCommit)
        {
            _entityFactory = entityFactory;
            _compiledModel = compiledModel;
            _connection = connection;
            _objectContext = compiledModel.CreateObjectContext<ObjectContext>(connection);
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
