using System.Collections.Generic;
using MongoDB.Driver;
using NWheels.Extensions;
using NWheels.Logging;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
{
    internal class ThreadLogBatchPersistor
    {
        private readonly MongoDbThreadLogPersistor _owner;
        private readonly IReadOnlyThreadLog[] _threadLogs;
        private readonly Dictionary<string, DailySummaryRecord> _dailySummaryRecordById;
        private readonly List<LogMessageRecord> _logMessageBatch;
        private readonly List<ThreadLogRecord> _threadLogBatch;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLogBatchPersistor(MongoDbThreadLogPersistor owner, IReadOnlyThreadLog[] threadLogs)
        {
            _owner = owner;
            _threadLogs = threadLogs;
            _dailySummaryRecordById = new Dictionary<string, DailySummaryRecord>(capacity: 64);
            _logMessageBatch = new List<LogMessageRecord>(capacity: 200 * threadLogs.Length);
            _threadLogBatch = new List<ThreadLogRecord>(capacity: threadLogs.Length);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void PersistBatch()
        {
            foreach ( var log in _threadLogs )
            {
                VisitLogActivity(log.RootActivity);

                if ( ShouldPersistThreadLog(log) )
                {
                    _threadLogBatch.Add(new ThreadLogRecord(log));
                }
            }

            if ( _dailySummaryRecordById.Count > 0 )
            {
                var dailySummaryBulkWrite = BuildDailySummaryBulkWriteOperation();
                dailySummaryBulkWrite.Execute(WriteConcern.Acknowledged);
            }

            if ( _logMessageBatch.Count > 0 )
            {
                _owner.LogMessageCollection.InsertBatch(_logMessageBatch, WriteConcern.Acknowledged);
            }

            if ( _threadLogBatch.Count > 0 )
            {
                _owner.ThreadLogCollection.InsertBatch(_threadLogBatch, WriteConcern.Acknowledged);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldPersistThreadLog(IReadOnlyThreadLog log)
        {
            if ((log.RootActivity.Options & LogOptions.RetainThreadLog) != 0)
            {
                return true;
            }

            switch ( _owner.LoggingConfig.ThreadLogPersistenceLevel )
            {
                case ThreadLogPersistenceLevel.All:
                    return true;
                case ThreadLogPersistenceLevel.StartupShutdown:
                    return (
                        log.TaskType == ThreadTaskType.StartUp || 
                        log.TaskType == ThreadTaskType.ShutDown);
                case ThreadLogPersistenceLevel.StartupShutdownErrors:
                    return (
                        log.TaskType == ThreadTaskType.StartUp || 
                        log.TaskType == ThreadTaskType.ShutDown ||
                        log.RootActivity.Level >= LogLevel.Error);
                case ThreadLogPersistenceLevel.StartupShutdownErrorsWarnings:
                    return (
                        log.TaskType == ThreadTaskType.StartUp ||
                        log.TaskType == ThreadTaskType.ShutDown ||
                        log.RootActivity.Level >= LogLevel.Warning);
                case ThreadLogPersistenceLevel.StartupShutdownErrorsWarningsDuration:
                    return (
                        log.TaskType == ThreadTaskType.StartUp ||
                        log.TaskType == ThreadTaskType.ShutDown ||
                        log.RootActivity.Level >= LogLevel.Warning ||
                        log.RootActivity.MillisecondsDuration >= 1000);
                default:
                    return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private BulkWriteOperation BuildDailySummaryBulkWriteOperation()
        {
            var bulkWrite = _owner.DailySummaryCollection.InitializeUnorderedBulkOperation();

            foreach ( var record in _dailySummaryRecordById.Values )
            {
                record.BuildIncrementUpsert(bulkWrite);
            }

            return bulkWrite;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void VisitLogActivity(ActivityLogNode activity)
        {
            VisitLogNode(activity);

            for ( var child = activity.FirstChild ; child != null ; child = child.NextSibling )
            {
                var childActivity = child as ActivityLogNode;

                if ( childActivity != null )
                {
                    VisitLogActivity(childActivity);
                }
                else
                {
                    VisitLogNode(child);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void VisitLogNode(LogNode node)
        {
            if ( node.Level >= LogLevel.Info )
            {
                var summaryRecord = _dailySummaryRecordById.GetOrAdd(
                    DailySummaryRecord.GetRecordId(node),
                    id => new DailySummaryRecord(node));
                summaryRecord.Increment(node);

                var messageRecord = new LogMessageRecord(node);
                _logMessageBatch.Add(messageRecord);
            }
        }
    }
}
