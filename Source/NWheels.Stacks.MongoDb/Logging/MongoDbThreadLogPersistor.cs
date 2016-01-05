using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using MongoDB.Driver;
using NWheels.Concurrency;
using NWheels.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Logging.Core;

namespace NWheels.Stacks.MongoDb.Logging
{
    public class MongoDbThreadLogPersistor : LifecycleEventListenerBase, IThreadPostMortem
    {
        private readonly IFramework _framework;
        private readonly IPlainLog _plainLog;
        private readonly ShuttleService<IReadOnlyThreadLog> _persistenceShuttle;
        private IFrameworkLoggingConfiguration _loggingConfig;
        private MongoDatabase _database;
        private MongoCollection _threadLogCollection;

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
            if ( threadLog.ShouldBePersisted )
            {
                _persistenceShuttle.Board(threadLog);
            }
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

        private void StoreThreadLogsToDb(IReadOnlyThreadLog[] threadLogs)
        {
            _threadLogCollection.InsertBatch(threadLogs.Select(log => log.TakeSnapshot()));
            //;  new ThreadLogRecord() {
            //    RootActivity = log.RootActivity.ToString(),
            //    TaskType = log.TaskType,
            //    CorrelationId = log.CorrelationId,
            //    LogId = log.LogId,
            //    MachineName = _s_machineName,
            //    NodeInstance = log.Node.InstanceId,
            //    NodeName = log.Node.NodeName,
            //    ProcessId = _s_processId,
            //    StartedAtUtc = log.ThreadStartedAtUtc
            //}));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConnectToDatabase()
        {
            var connectionString = new MongoConnectionStringBuilder(_loggingConfig.ThreadLogDbConnectionString ?? "server=localhost;database=nwheels_threadlog");
            var client = new MongoClient(connectionString.ConnectionString);
            var server = client.GetServer();
            
            _database = server.GetDatabase(connectionString.DatabaseName);

            var collectionPrefix = string.Format("System.AppEvents.{0}.", _framework.CurrentNode.EnvironmentName);

            _threadLogCollection = _database.GetCollection<ThreadLogRecord>(collectionPrefix + "ThreadLogs");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly static string _s_machineName = System.Environment.MachineName;
        private static readonly int _s_processId = System.Diagnostics.Process.GetCurrentProcess().Id;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ThreadLogRecord
        {
            public string NodeName { get; set; }
            public string NodeInstance { get; set; }
            public string MachineName { get; set; }
            public int ProcessId { get; set; }
            public Guid LogId { get; set; }
            public Guid CorrelationId { get; set; }
            public DateTime StartedAtUtc { get; set; }
            public ThreadTaskType TaskType { get; set; }
            public string RootActivity { get; set; }
        }
    }
}
