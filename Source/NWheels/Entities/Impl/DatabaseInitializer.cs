using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels;
using NWheels.Authorization;
using NWheels.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Logging;

namespace NWheels.Entities.Impl
{
    public class DatabaseInitializer : LifecycleEventListenerBase
    {
        private readonly IStorageInitializer _storageInitializer;
        private readonly IEnumerable<DataRepositoryRegistration> _contextRegistrations;
        private readonly Pipeline<IDomainContextPopulator> _populators;
        private readonly IFrameworkDatabaseConfig _configuration;
        private readonly ISessionManager _sessionManager;
        private readonly UnitOfWorkFactory _unitOfWorkFactory;
        private readonly ILogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DatabaseInitializer(
            IStorageInitializer storageInitializer,
            IEnumerable<DataRepositoryRegistration> contextRegistrations,
            Pipeline<IDomainContextPopulator> populators, 
            Auto<IFrameworkDatabaseConfig> configuration, 
            ISessionManager sessionManager,
            UnitOfWorkFactory unitOfWorkFactory,
            ILogger logger)
        {
            _storageInitializer = storageInitializer;
            _contextRegistrations = contextRegistrations;
            _populators = populators;
            _configuration = configuration.Instance;
            _sessionManager = sessionManager;
            _unitOfWorkFactory = unitOfWorkFactory;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeConfigured(List<ILifecycleEventListener> additionalComponentsToHost)
        {
            foreach ( var registration in _contextRegistrations )
            {
                if ( registration.ShouldInitializeStorageOnStartup )
                {
                    _logger.RunningStorageInitializationCheck(contextType: registration.DataRepositoryType);
                    RunStorageInitializationCheck(registration.DataRepositoryType);
                }
                else
                {
                    _logger.StorageInitializationTurnedOffSkipping(contextType: registration.DataRepositoryType);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunStorageInitializationCheck(Type contextType)
        {
            var connectionString = _configuration.GetContextConnectionString(contextType);

            if ( string.IsNullOrEmpty(connectionString) )
            {
                throw _logger.ConnectionStringNotConfigured(contextType);
            }

            if ( _storageInitializer.StorageSchemaExists(connectionString) )
            {
                _logger.FoundDatabase(contextType, connectionString);
                return;
            }

            _logger.DatabaseNotFound(contextType, connectionString);

            using ( var dbActivity = _logger.InitializingNewDatabase(contextType, connectionString) )
            {
                try
                {
                    InitializeNewDatabase(contextType, connectionString, databaseNameOverride: null);
                    _logger.NewDatabaseInitialized(contextType, connectionString);
                }
                catch ( Exception e )
                {
                    dbActivity.Fail(e);
                    throw _logger.DatabaseInitializationFailed(contextType, connectionString, e);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeNewDatabase(Type contextType, string connectionString, string databaseNameOverride)
        {
            _storageInitializer.CreateStorageSchema(_configuration.ConnectionString);
            _logger.DatabaseSchemaCreated(_configuration.ConnectionString);

            using ( _sessionManager.JoinGlobalSystem() )
            {
                foreach ( var populator in _populators.Where(p => p.ContextType.IsAssignableFrom(contextType)) )
                {
                    using ( var populatorActivity = _logger.InvokingContextPopulator(contextType, populator.GetType()) )
                    {
                        try
                        {
                            using ( var context = _unitOfWorkFactory.NewUnitOfWork(contextType, databaseName: databaseNameOverride) )
                            {
                                populator.Populate(context);

                                if ( context.UnitOfWorkState != UnitOfWorkState.Committed )
                                {
                                    context.CommitChanges();
                                }
                            }
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
            void RunningStorageInitializationCheck(Type contextType);

            [LogVerbose]
            void StorageInitializationTurnedOffSkipping(Type contextType);

            [LogError]
            ConfigurationErrorsException ConnectionStringNotConfigured(Type contextType);

            [LogInfo]
            void FoundDatabase(Type contextType, string connectionString);
            
            [LogWarning]
            void DatabaseNotFound(Type contextType, string connectionString);

            [LogVerbose]
            void DatabaseSchemaCreated(string connectionString);

            [LogInfo]
            void NewDatabaseInitialized(Type contextType, string connectionString);
            
            [LogCritical]
            Exception DatabaseInitializationFailed(Type contextType, string connectionString, Exception error);

            [LogActivity]
            ILogActivity InitializingNewDatabase(Type contextType, string connectionString);

            [LogActivity]
            ILogActivity InvokingContextPopulator(Type contextType, Type populatorType);
        }
    }
}
