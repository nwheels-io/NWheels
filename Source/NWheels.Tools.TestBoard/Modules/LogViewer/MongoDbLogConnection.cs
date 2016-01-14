using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NWheels.Logging;
using NWheels.Stacks.MongoDb.Logging;
using NWheels.Testing.Controllers;

namespace NWheels.Tools.TestBoard.Modules.LogViewer
{
    public class MongoDbLogConnection : ILogConnection
    {
        private const int MaxRecentLogIds = 100;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly object _syncRoot = new object();
        private readonly MongoDatabase[] _dbs;
        private readonly MongoCollection<ThreadLogRecord>[] _dbCollections;
        private readonly Queue<Guid> _recentLogIds;
        private DateTime _lastLogTimestamp = DateTime.MinValue;
        private Timer _pollingTimer = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoDbLogConnection(string[] dbConnectionStrings)
        {
            _dbs = dbConnectionStrings.Select(OpenMongoDatabase).ToArray();
            _dbCollections = _dbs.SelectMany(OpenThreadLogCollections).ToArray();
            _recentLogIds = new Queue<Guid>(capacity: MaxRecentLogIds);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IDisposable

        public void Dispose()
        {
            StopCapture();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ILogConnection

        public void StartCapture()
        {
            lock ( _syncRoot )
            {
                _pollingTimer = new Timer(
                    state => {
                        FetchLatestThreadLogs();
                    },
                    state: null,
                    dueTime: TimeSpan.FromSeconds(0.5),
                    period: TimeSpan.FromSeconds(2));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StopCapture()
        {
            lock ( _syncRoot )
            {
                if ( _pollingTimer != null )
                {
                    _pollingTimer.Dispose();
                    _pollingTimer = null;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsCapturing
        {
            get 
            {
                lock ( _syncRoot )
                {
                    return (_pollingTimer != null);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler<ThreadLogsCapturedEventArgs> ThreadLogsCaptured;
        public event EventHandler<PlainLogsCapturedEventArgs> PlainLogsCaptured;

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FetchLatestThreadLogs()
        {
            lock ( _syncRoot )
            {
                var query = Query<ThreadLogRecord>.GTE(x => x.Timestamp, _lastLogTimestamp);
                var logs = new List<ThreadLogSnapshot>();

                try
                {
                    foreach ( var collection in _dbCollections )
                    {
                        logs.AddRange(collection.Find(query)
                            .SetSortOrder(SortBy.Ascending("Timestamp"))
                            .Select(EnsureCanDeserializeThreadLog)
                            .Where(record => IsNewThreadLog(record.Snapshot.LogId))
                            .Select(record => record.Snapshot));
                    }
                }
                catch ( Exception e )
                {
                    logs.Add(CreateGeneralErrorRecord(e).Snapshot);
                }

                if ( logs.Count > 0 )
                {
                    _lastLogTimestamp = logs[logs.Count - 1].StartedAtUtc;

                    if ( ThreadLogsCaptured != null )
                    {
                        ThreadLogsCaptured(this, new ThreadLogsCapturedEventArgs(logs.ToArray()));
                    }

                    if ( PlainLogsCaptured != null )
                    {
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLogRecord EnsureCanDeserializeThreadLog(ThreadLogRecord record)
        {
            try
            {
                var log = record.Snapshot;
                return record;
            }
            catch ( Exception e )
            {
                return CreateDeserializationErrorRecord(record, e);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLogRecord CreateGeneralErrorRecord(Exception exception)
        {
            return new ThreadLogRecord(new ThreadLogSnapshot() {
                NodeName = "TestBoard",
                RootActivity = new ThreadLogSnapshot.LogNodeSnapshot() {
                    MessageId = "LOG FETCH ERROR",
                    NameValuePairs = new ThreadLogSnapshot.NameValuePairSnapshot[0],
                    SubNodes = new ThreadLogSnapshot.LogNodeSnapshot[0],
                    IsActivity = true,
                    Level = LogLevel.Error,
                    ExceptionTypeName = exception.GetType().FullName,
                    ExceptionDetails = exception.ToString()
                },
                MachineName = Environment.MachineName,
                LogId = Guid.NewGuid(),
                CorrelationId = Guid.Empty,
                TaskType = ThreadTaskType.LogProcessing,
                ProcessId = 0,
                NodeInstance = "N/A",
                StartedAtUtc = DateTime.UtcNow,
                EnvironmentName = "N/A"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLogRecord CreateDeserializationErrorRecord(ThreadLogRecord record, Exception exception)
        {
            return new ThreadLogRecord(new ThreadLogSnapshot() {
                NodeName = record.NodeName,
                RootActivity = new ThreadLogSnapshot.LogNodeSnapshot() {
                    IsActivity = true,
                    MessageId = "LOG FETCH ERROR",
                    NameValuePairs = new ThreadLogSnapshot.NameValuePairSnapshot[0],
                    SubNodes = new ThreadLogSnapshot.LogNodeSnapshot[0],
                    Level = LogLevel.Error,
                    ExceptionTypeName = exception.GetType().FullName,
                    ExceptionDetails = exception.ToString()
                },
                MachineName = record.MachineName,
                LogId = Guid.Parse(record.LogId),
                CorrelationId = Guid.Parse(record.CorrelationId),
                TaskType = record.TaskType,
                ProcessId = 0,
                NodeInstance = record.NodeInstance,
                StartedAtUtc = record.Timestamp,
                EnvironmentName = "???"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsNewThreadLog(Guid logId)
        {
            if ( _recentLogIds.Contains(logId) )
            {
                return false;
            }

            while ( _recentLogIds.Count > MaxRecentLogIds )
            {
                _recentLogIds.Dequeue();
            }

            _recentLogIds.Enqueue(logId);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MongoDatabase OpenMongoDatabase(string connectionString)
        {
            var connectionStringBuilder = new MongoConnectionStringBuilder(connectionString);

            var client = new MongoClient(connectionStringBuilder.ConnectionString);
            var server = client.GetServer();
            var database = server.GetDatabase(connectionStringBuilder.DatabaseName);

            return database;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IEnumerable<MongoCollection<ThreadLogRecord>> OpenThreadLogCollections(MongoDatabase db)
        {
            return db.GetCollectionNames().Where(IsThreadLogCollectionName).Select(db.GetCollection<ThreadLogRecord>);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsThreadLogCollectionName(string name)
        {
            return (name.StartsWith("System.Logs.") && name.EndsWith(".ThreadLog"));
        }
    }
}
