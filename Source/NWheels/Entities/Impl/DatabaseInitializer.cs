using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels;
using NWheels.Authorization;
using NWheels.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Migrations;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Logging;

namespace NWheels.Entities.Impl
{
    public class DatabaseInitializer : LifecycleEventListenerBase
    {
        private readonly IComponentContext _components;
        private readonly IStorageInitializer _storageInitializer;
        private readonly IEnumerable<DataRepositoryRegistration> _contextRegistrations;
        private readonly IEnumerable<DatabaseInitializationCheckRegistration> _initializationCheckRegistrations;
        private readonly IEnumerable<IDbConnectionStringResolver> _connectionStringResolvers;
        private readonly Pipeline<IDomainContextPopulator> _populators;
        private readonly Pipeline<SchemaMigrationCollection> _migrations;
        private readonly ISessionManager _sessionManager;
        private readonly UnitOfWorkFactory _unitOfWorkFactory;
        private readonly ILogger _logger;
        private IFrameworkDatabaseConfig _configuration;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DatabaseInitializer(
            IComponentContext components,
            IStorageInitializer storageInitializer,
            IEnumerable<DataRepositoryRegistration> contextRegistrations,
            IEnumerable<DatabaseInitializationCheckRegistration> initializationCheckRegistrations,
            IEnumerable<IDbConnectionStringResolver> connectionStringResolvers,
            Pipeline<IDomainContextPopulator> populators, 
            Pipeline<SchemaMigrationCollection> migrations,
            ISessionManager sessionManager,
            UnitOfWorkFactory unitOfWorkFactory,
            ILogger logger)
        {
            _components = components;
            _storageInitializer = storageInitializer;
            _contextRegistrations = contextRegistrations;
            _initializationCheckRegistrations = initializationCheckRegistrations;
            _connectionStringResolvers = connectionStringResolvers;
            _populators = populators;
            _migrations = migrations;
            _sessionManager = sessionManager;
            _unitOfWorkFactory = unitOfWorkFactory;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeConfigured(List<ILifecycleEventListener> additionalComponentsToHost)
        {
            _configuration = _components.Resolve<IFrameworkDatabaseConfig>();

            foreach (var registration in _contextRegistrations.OrderBy(r => r.HasMultipleDatabases))
            {
                if (ShouldInitializeStorageOnStartup(registration))
                {
                    _logger.RunningStorageInitializationCheck(contextType: registration.DataRepositoryType);

                    bool newDatabaseCreated;
                    RunStorageInitializationCheck(out newDatabaseCreated, registration.DataRepositoryType);
                }
                else
                {
                    _logger.StorageInitializationTurnedOffSkipping(contextType: registration.DataRepositoryType);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunStorageInitializationCheck(out bool newDatabaseCreated, Type contextType)
        {
            var connectionConfig = _configuration.GetContextConnectionConfig(contextType);
            newDatabaseCreated = false;

            if (connectionConfig == null) return;

            if (!connectionConfig.IsWildcard)
            {
                RunStorageInitializationCheck(out newDatabaseCreated, contextType, connectionStringOverride: null);
            }
            else
            {
                var resolver = _connectionStringResolvers.FirstOrDefault(r => r.DomainContextType == contextType);

                if (resolver != null)
                {
                    foreach (var connectionString in resolver.GetAllConnectionStrings(_storageInitializer))
                    {
                        bool singleDatabaseCreated;
                        RunStorageInitializationCheck(out singleDatabaseCreated, contextType, connectionStringOverride: connectionString);
                        newDatabaseCreated |= singleDatabaseCreated;
                    }
                }
                else
                {
                    _logger.StorageInitializationNotConfiguredSkipping(contextType);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunStorageInitializationCheck(out bool newDatabaseCreated, Type contextType, string connectionStringOverride)
        {
            var connectionString = connectionStringOverride ?? GetConfiguredContextConnectionString(contextType);

            if (_storageInitializer.StorageSchemaExists(connectionString))
            {
                _logger.FoundDatabase(contextType, connectionString);
                newDatabaseCreated = false;

                if (ShouldMigrateStorageSchemaOnStartup(contextType))
                {
                    MigrateExistingDatabase(contextType, connectionString);
                }
            }
            else
            {
                _logger.DatabaseNotFound(contextType, connectionString);

                using (var dbActivity = _logger.InitializingNewDatabase(contextType, connectionString))
                {
                    try
                    {
                        InitializeNewDatabase(contextType, connectionString);
                        _logger.NewDatabaseInitialized(contextType, connectionString);
                    }
                    catch (Exception e)
                    {
                        dbActivity.Fail(e);
                        throw _logger.DatabaseInitializationFailed(contextType, connectionString, e);
                    }
                }

                newDatabaseCreated = true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldInitializeStorageOnStartup(DataRepositoryRegistration contextRegistration)
        {
            if ( _initializationCheckRegistrations.Any(check => check.ContextType == contextRegistration.DataRepositoryType) )
            {
                return true;
            }

            var contextConfig = _configuration.GetContextConnectionConfig(contextRegistration.DataRepositoryType);

            if ( contextConfig != null )
            {
                return contextConfig.AutoCreateDatabase;
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldMigrateStorageSchemaOnStartup(Type contextType)
        {
            var contextConfig = _configuration.GetContextConnectionConfig(contextType);

            if (contextConfig != null)
            {
                return contextConfig.AutoMigrateDatabase;
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeNewDatabase(Type contextType, string connectionString)
        {
            var schemaVersion = GetCurrentSchemaVersionInApplication(contextType);

            _storageInitializer.CreateStorageSchema(connectionString, schemaVersion);
            _logger.DatabaseSchemaCreated(connectionString);

            using ( _sessionManager.JoinGlobalSystem() )
            {
                foreach ( var populator in _populators.Where(p => p.ContextType.IsAssignableFrom(contextType)) )
                {
                    using ( var populatorActivity = _logger.InvokingContextPopulator(contextType, populator.GetType()) )
                    {
                        try
                        {
                            using ( var context = _unitOfWorkFactory.NewUnitOfWork(contextType, connectionString: connectionString) )
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

        private int GetCurrentSchemaVersionInApplication(Type contextType)
        {
            return _migrations
                .Where(m => m.DomainContextType.IsAssignableFrom(contextType))
                .Select(c => c.SchemaVersion)
                .DefaultIfEmpty(defaultValue: 1)
                .Max();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void MigrateExistingDatabase(Type contextType, string connectionString)
        {
            using (var activity = _logger.MigratingExistingDatabase(contextType, connectionString))
            {
                var migrationCollections = _migrations.Where(m => m.DomainContextType.IsAssignableFrom(contextType)).ToArray();

                using (_sessionManager.JoinGlobalSystem())
                {
                    try
                    {
                        using (var context = _unitOfWorkFactory.NewUnitOfWork(contextType, connectionString: connectionString))
                        {
                            foreach (var collection in migrationCollections)
                            {
                                _storageInitializer.MigrateStorageSchema(connectionString, context.As<DataRepositoryBase>(), collection);
                            }

                            if (context.UnitOfWorkState != UnitOfWorkState.Committed)
                            {
                                context.CommitChanges();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        activity.Fail(e);
                        throw;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetConfiguredContextConnectionString(Type contextType)
        {
            var configuredConnectionString = _configuration.GetContextConnectionString(contextType);

            if ( string.IsNullOrEmpty(configuredConnectionString) )
            {
                throw _logger.ConnectionStringNotConfigured(contextType);
            }

            return configuredConnectionString;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogInfo]
            void RunningStorageInitializationCheck(Type contextType);

            [LogVerbose]
            void StorageInitializationTurnedOffSkipping(Type contextType);

            [LogWarning]
            void StorageInitializationNotConfiguredSkipping(Type contextType);

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

            [LogActivity]
            ILogActivity MigratingExistingDatabase(Type contextType, string connectionString);
        }
    }
}
