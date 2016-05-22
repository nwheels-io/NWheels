using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;
using NWheels.Logging;
using NWheels.UI;
using NWheels.UI.Factories;
using NWheels.UI.Toolbox;
using RecordKey = System.Tuple<string, string, string, string, string>;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    public class ThreadLogListTx : AbstractThreadLogListTx
    {
        private readonly IFramework _framework;
        private readonly MongoDbThreadLogQueryService _queryService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLogListTx(IFramework framework, IViewModelObjectFactory viewModelFactory, MongoDbThreadLogQueryService queryService)
            : base(framework, viewModelFactory)
        {
            _framework = framework;
            _queryService = queryService;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Input,ILogTimeRangeCriteria,IQueryable<ILogMessageEntity>>

        public override IQueryable<IThreadLogEntity> Execute(ILogTimeRangeCriteria input)
        {
            MongoDbThreadLogQueryService.NormalizeTimeRange(input);

            var query = (UIOperationContext.Current != null ? UIOperationContext.Current.Query : null);
            var task = _queryService.QueryThreadLogsAsync(input, query, CancellationToken.None);
            task.Wait();

            var resultSetBuilder = new ResultSetBuilder(_framework);
            var chartBuilder = new ChartBuilder(input);

            if (task.Result != null)
            {
                resultSetBuilder.AddRecords(task.Result);
                chartBuilder.AddRecords(task.Result);
            }

            return new VisualizedQueryable<IThreadLogEntity>(
                resultSetBuilder.GetResultRows().AsQueryable(),
                chartBuilder.GetChart());
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ResultSetBuilder
        {
            private readonly List<IThreadLogEntity> _resultSet = new List<IThreadLogEntity>();
            private readonly IFramework _framework;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ResultSetBuilder(IFramework framework)
            {
                _framework = framework;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddRecords(IEnumerable<ThreadLogRecord> records)
            {
                _resultSet.AddRange(records.Select(CreateResult));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<IThreadLogEntity> GetResultRows()
            {
                return _resultSet;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IThreadLogEntity CreateResult(ThreadLogRecord record)
            {
                var result = _framework.NewDomainObject<IThreadLogEntity>().As<ThreadLogEntity>();
                result.CopyFromRecord(record);
                return result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ChartBuilder : ChartBuilderBase<ThreadLogRecord, LogLevel>
        {
            public ChartBuilder(ILogTimeRangeCriteria input)
                : base(input)
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            protected override IEnumerable<LogLevel> GetSeries()
            {
                yield return LogLevel.Info;
                yield return LogLevel.Warning;
                yield return LogLevel.Error;
                yield return LogLevel.Critical;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void Accumulate(ThreadLogRecord record)
            {
                var levelToIncrement = (record.Level > LogLevel.Info ? record.Level : LogLevel.Info);
                IncrementSeriesTimebox(levelToIncrement, record.Timestamp, 1);
            }
        }
    }
}
