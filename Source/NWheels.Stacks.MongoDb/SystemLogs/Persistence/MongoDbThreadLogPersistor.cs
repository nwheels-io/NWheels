using System;
using System.Diagnostics;
using Autofac;
using MongoDB.Driver;
using NWheels.Concurrency;
using NWheels.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Logging.Core;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
{
    public class MongoDbThreadLogPersistor : LifecycleEventListenerBase, IThreadPostMortem
    {
        private readonly IFramework _framework;
        private readonly IPlainLog _plainLog;
        private readonly ShuttleService<IReadOnlyThreadLog> _persistenceShuttle;
        private IFrameworkLoggingConfiguration _loggingConfig;
        private MongoDatabase _database;
        private MongoCollection _threadLogCollection;
        private MongoCollection _logMessageCollection;
        private MongoCollection _dailySummaryCollection;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoDbThreadLogPersistor(IFramework framework, IPlainLog plainLog)
        {
            _framework = framework;
            _plainLog = plainLog;
            _persistenceShuttle = new ShuttleService<IReadOnlyThreadLog>(
                _framework,
                "MongoDbThreadLogPersistor",
                maxItemsOnBoard: 100,
                boardingTimeout: TimeSpan.FromMilliseconds(500),
                driverThreadCount: 2,
                driver: StoreThreadLogsToDb);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IThreadPostMortem

        public void Examine(IReadOnlyThreadLog threadLog)
        {
            _persistenceShuttle.Board(threadLog);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeLoading()
        {
            _loggingConfig = _framework.As<ICoreFramework>().Components.Resolve<IFrameworkLoggingConfiguration>();
            ConnectToDatabase();
            _persistenceShuttle.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeUnloaded()
        {
            _persistenceShuttle.Stop(TimeSpan.FromSeconds(15));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoCollection ThreadLogCollection
        {
            get { return _threadLogCollection; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoCollection DailySummaryCollection
        {
            get { return _dailySummaryCollection; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoCollection LogMessageCollection
        {
            get { return _logMessageCollection; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IFrameworkLoggingConfiguration LoggingConfig
        {
            get { return _loggingConfig; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void StoreThreadLogsToDb(IReadOnlyThreadLog[] threadLogs)
        {
            var batchPersistor = new ThreadLogBatchPersistor(this, threadLogs);
            batchPersistor.PersistBatch();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConnectToDatabase()
        {
            _database = ConnectToDatabase(_loggingConfig);

            _logMessageCollection = _database.GetCollection(DbNamingConvention.GetLogMessageCollectionName(_framework.CurrentNode.EnvironmentName));
            _threadLogCollection = _database.GetCollection(DbNamingConvention.GetThreadLogCollectionName(_framework.CurrentNode.EnvironmentName));
            _dailySummaryCollection = _database.GetCollection(DbNamingConvention.GetDailySummaryCollectionName(_framework.CurrentNode.EnvironmentName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly int _s_processId = Process.GetCurrentProcess().Id;
        private static readonly string _s_machineName = System.Environment.MachineName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MongoDatabase ConnectToDatabase(IFrameworkLoggingConfiguration configuration)
        {
            var connectionStringValue = (configuration.ThreadLogDbConnectionString ?? "server=localhost;database=" + DbNamingConvention.DefaultDatabaseName);
            var connectionString = new MongoConnectionStringBuilder(connectionStringValue);
            var client = new MongoClient(connectionString.ConnectionString);
            var server = client.GetServer();
            var database = server.GetDatabase(connectionString.DatabaseName);
            
            return database;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string MachineName
        {
            get { return _s_machineName; }
        }
    }
}
