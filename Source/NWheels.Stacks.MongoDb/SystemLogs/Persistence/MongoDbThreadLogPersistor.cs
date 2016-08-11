using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Autofac;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
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
        private ShuttleService<IReadOnlyThreadLog> _persistenceShuttle;
        private IFrameworkLoggingConfiguration _loggingConfig;
        private IMongoDbThreadLogPersistorConfig _persistenceConfig;
        private MongoDatabase _database;
        private MongoCollection _threadLogCollection;
        private MongoCollection _logMessageCollection;
        private MongoCollection _dailySummaryCollection;
        private MongoGridFS _threadLogGridfs;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoDbThreadLogPersistor(IFramework framework, IPlainLog plainLog)
        {
            _framework = framework;
            _plainLog = plainLog;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IThreadPostMortem

        public void Examine(IReadOnlyThreadLog threadLog)
        {
            if (_persistenceShuttle != null)
            {
                _persistenceShuttle.Board(threadLog);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeLoading()
        {
            _loggingConfig = _framework.As<ICoreFramework>().Components.Resolve<IFrameworkLoggingConfiguration>();
            _persistenceConfig = _framework.As<ICoreFramework>().Components.Resolve<IMongoDbThreadLogPersistorConfig>();

            if (!_persistenceConfig.Enabled)
            {
                return;
            }

            ConnectToDatabase();

            _persistenceShuttle = new ShuttleService<IReadOnlyThreadLog>(
                _framework,
                "MongoDbThreadLogPersistor",
                maxItemsOnBoard: _persistenceConfig.BatchSize,
                boardingTimeout: _persistenceConfig.BatchTimeout,
                driverThreadCount: _persistenceConfig.ThreadCount,
                driver: StoreThreadLogsToDb);

            _persistenceShuttle.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeUnloaded()
        {
            if (_persistenceShuttle != null)
            {
                _persistenceShuttle.Stop(TimeSpan.FromSeconds(15));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoCollection ThreadLogCollection
        {
            get { return _threadLogCollection; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoGridFS ThreadLogGridfs
        {
            get { return _threadLogGridfs; }
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
            var clock = Stopwatch.StartNew();

            try
            {
                TimeSpan dbTime;

                var batchPersistor = new ThreadLogBatchPersistor(this, threadLogs);
                batchPersistor.PersistBatch(out dbTime);

                var elapsed = clock.Elapsed;
                var cpuTime = elapsed.Subtract(dbTime);

                //_plainLog.Info(
                //    "MongoDbThreadLogPersistor.StoreThreadLogsToDb[thread {0}] : {1} in batch, {2} queued, time CPU: {3}, DB: {4} ; done {5} in total",
                //    Thread.CurrentThread.ManagedThreadId,
                //    threadLogs.Length,
                //    _persistenceShuttle.DepartureQueueLength,
                //    cpuTime,
                //    dbTime,
                //    Interlocked.Add(ref _s_totalThreadLogsReceived, threadLogs.Length));
            }
            catch (MongoBulkWriteException e)
            {
                _plainLog.Error(
                    "MongoDbThreadLogPersistor.StoreThreadLogsToDb[thread {0}] : FAILED! {1}: {2} -- BULK WRITE ERRORS: {3}", 
                    Thread.CurrentThread.ManagedThreadId,
                    e.GetType().Name,
                    e.ToString(),
                    string.Join(";", e.WriteErrors.Select(err => err.Message)));
            }
            catch (Exception e)
            {
                _plainLog.Error(
                    "MongoDbThreadLogPersistor.StoreThreadLogsToDb[thread {0}] : FAILED! {1}: {2}", 
                    Thread.CurrentThread.ManagedThreadId,
                    e.GetType().Name,
                    e.ToString());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConnectToDatabase()
        {
            _database = ConnectToDatabase(_loggingConfig);

            var dailySummaryCollectionName = DbNamingConvention.GetDailySummaryCollectionName(_framework.CurrentNode.EnvironmentName);
            var logMessageCollectionName = DbNamingConvention.GetLogMessageCollectionName(_framework.CurrentNode.EnvironmentName);
            var threadLogCollectionName = DbNamingConvention.GetThreadLogCollectionName(_framework.CurrentNode.EnvironmentName);

            var maxSize = _persistenceConfig.MaxCollectionSizeMb * 1024L * 1024L;
            var maxDocuments = _persistenceConfig.MaxCollectionDocuments;

            _dailySummaryCollection = _database.GetCollection(dailySummaryCollectionName);
            _logMessageCollection = GetOrCreateCappedCollection(logMessageCollectionName, maxSize, maxDocuments);
            _threadLogCollection = GetOrCreateCappedCollection(threadLogCollectionName, maxSize, maxDocuments);

            _threadLogGridfs = _database.GetGridFS(DbNamingConvention.GetThreadLogGridfsSettings(_framework.CurrentNode.EnvironmentName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MongoCollection GetOrCreateCappedCollection(string collectionName, long maxSize, long maxDocuments)
        {
            if (!_database.CollectionExists(collectionName))
            {
                var options = CollectionOptions
                   .SetCapped(true)
                   .SetMaxSize(maxSize)
                   .SetMaxDocuments(maxDocuments);

                _database.CreateCollection(collectionName, options);
            }

            return _database.GetCollection(collectionName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly int _s_processId = Process.GetCurrentProcess().Id;
        private static readonly string _s_machineName = System.Environment.MachineName;
        private static int _s_totalThreadLogsReceived = 0;

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static int ProcessId
        {
            get { return _s_processId; }
        }
    }
}
