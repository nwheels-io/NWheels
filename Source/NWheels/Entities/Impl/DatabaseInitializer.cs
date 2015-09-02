using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels;
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
        private readonly Pipeline<IDataRepositoryPopulator> _populators;
        private readonly IFrameworkDatabaseConfig _configuration;
        private readonly ILogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DatabaseInitializer(
            IStorageInitializer initializer, 
            Pipeline<IDataRepositoryPopulator> populators, 
            Auto<IFrameworkDatabaseConfig> configuration, 
            ILogger logger)
        {
            _initializer = initializer;
            _populators = populators;
            _configuration = configuration.Instance;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeConfigured(List<ILifecycleEventListener> additionalComponentsToHost)
        {
            if ( _initializer.StorageSchemaExists(_configuration.ConnectionString) )
            {
                _logger.FoundDatabase(_configuration.ConnectionString);
                return;
            }

            _logger.DatabaseNotFound(_configuration.ConnectionString);

            using ( _logger.InitializingNewDatabase() )
            {
                try
                {
                    InitializeNewDatabase();
                }
                catch ( Exception e )
                {
                    _logger.DatabaseInitializationFailed(_configuration.ConnectionString, e);
                    throw;
                }
            }

            _logger.NewDatabaseInitialized(_configuration.ConnectionString);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeNewDatabase()
        {
            _initializer.CreateStorageSchema(_configuration.ConnectionString);

            foreach ( var populator in _populators )
            {
                using ( var populatorActivity = _logger.InvokingDataPopulator(type: populator.GetType().FullName) )
                {
                    try
                    {
                        populator.Populate();
                    }
                    catch ( Exception e )
                    {
                        populatorActivity.Fail(e);
                        throw;
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
            ILogActivity InvokingDataPopulator(string type);
        }
    }
}
