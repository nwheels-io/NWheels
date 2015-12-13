using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels;
using NWheels.Authorization;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Logging;

namespace NWheels.Entities.Impl
{
    public class DatabaseInitializer : LifecycleEventListenerBase
    {
        private readonly IStorageInitializer _initializer;
        private readonly Pipeline<IDomainContextPopulator> _populators;
        private readonly IFrameworkDatabaseConfig _configuration;
        private readonly ILogger _logger;
        private readonly ISessionManager _sessionManager;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DatabaseInitializer(
            IStorageInitializer initializer, 
            Pipeline<IDomainContextPopulator> populators, 
            Auto<IFrameworkDatabaseConfig> configuration, 
            ISessionManager sessionManager,
            ILogger logger)
        {
            _sessionManager = sessionManager;
            _initializer = initializer;
            _populators = populators;
            _configuration = configuration.Instance;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeConfigured(List<ILifecycleEventListener> additionalComponentsToHost)
        {
            var anyStorageSchemaMissing = false;
            CheckIfStorachSchemaExists(_configuration.ConnectionString, ref anyStorageSchemaMissing);

            foreach ( var context in _configuration.Contexts )
            {
                CheckIfStorachSchemaExists(context.ConnectionString, ref anyStorageSchemaMissing);
            }

            if ( !anyStorageSchemaMissing )
            {
                return;
            }

            using ( _logger.InitializingNewDatabase() )
            {
                try
                {
                    InitializeNewDatabases();
                }
                catch ( Exception e )
                {
                    _logger.DatabaseInitializationFailed(_configuration.ConnectionString, e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CheckIfStorachSchemaExists(string connectionString, ref bool anySchemaMissing)
        {
            if ( !string.IsNullOrEmpty(connectionString) )
            {
                if ( _initializer.StorageSchemaExists(connectionString) )
                {
                    _logger.FoundDatabase(connectionString);
                }
                else
                {
                    _logger.DatabaseNotFound(connectionString);
                    anySchemaMissing = true;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeNewDatabases()
        {
            if ( _configuration.ConnectionString != null )
            {
                _initializer.CreateStorageSchema(_configuration.ConnectionString);
                _logger.NewDatabaseInitialized(_configuration.ConnectionString);
            }

            foreach ( var context in _configuration.Contexts )
            {
                _initializer.CreateStorageSchema(context.ConnectionString);
                _logger.NewDatabaseInitialized(context.ConnectionString);
            }

            using ( _sessionManager.JoinGlobalSystem() )
            {
                foreach ( var populator in _populators )
                {
                    using ( var populatorActivity = _logger.InvokingContextPopulator(type: populator.GetType().FullName) )
                    {
                        try
                        {
                            //populator.Populate();
                        }
                        catch ( Exception e )
                        {
                            populatorActivity.Fail(e);
                            throw;
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogInfo]
            void FoundDatabase(string connectionString);
            
            [LogWarning]
            void DatabaseNotFound(string connectionString);
            
            [LogInfo]
            void NewDatabaseInitialized(string connectionString);
            
            [LogError]
            void DatabaseInitializationFailed(string connectionString, Exception error);

            [LogActivity]
            ILogActivity InitializingNewDatabase();

            [LogActivity]
            ILogActivity InvokingContextPopulator(string type);
        }
    }
}
