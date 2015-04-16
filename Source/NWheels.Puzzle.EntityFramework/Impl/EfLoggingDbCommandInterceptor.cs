using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Hosting;

namespace NWheels.Puzzle.EntityFramework.Impl
{
    internal class EfLoggingDbCommandInterceptor : LifecycleEventListenerBase, IDbCommandInterceptor
    {
        private readonly IDbCommandLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfLoggingDbCommandInterceptor(IDbCommandLogger logger)
        {
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NonQueryExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void NonQueryExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            _logger.ExecutingSql(command.CommandText);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReaderExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReaderExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext)
        {
            _logger.ExecutingSql(command.CommandText);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ScalarExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ScalarExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            _logger.ExecutingSql(command.CommandText);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LifecycleEventListenerBase

        public override void NodeLoading()
        {
            DbInterception.Add(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeUnloaded()
        {
            DbInterception.Remove(this);
        }

        #endregion
    }
}
