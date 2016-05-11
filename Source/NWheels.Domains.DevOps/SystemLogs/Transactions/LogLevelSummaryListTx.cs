using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Processing;
using NWheels.UI;

namespace NWheels.Domains.DevOps.SystemLogs.Transactions
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public class LogLevelSummaryListTx : TransactionScript<Empty.Context, ILogTimeRangeCriteria, IQueryable<ILogLevelSummaryEntity>>
    {
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogLevelSummaryListTx(IFramework framework)
        {
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Input,ILogTimeRangeCriteria,IQueryable<ILogLevelSummaryEntity>>

        public override IQueryable<ILogLevelSummaryEntity> Execute(ILogTimeRangeCriteria input)
        {
            var random = new Random();
            var today = DateTime.UtcNow.Date;
            var results = new List<ILogLevelSummaryEntity>();

            foreach (var node in new[] { "BackEnd", "ETL", "WebApp1", "WebApp2" })
            {
                foreach (var instance in new[] { "Master", "Slave" })
                {
                    foreach (var replica in new[] { "1", "2", "3" })
                    {
                        var result = _framework.NewDomainObject<ILogLevelSummaryEntity>();

                        result.Environment = "PROD";
                        result.Machine = "QWESVR123" + random.Next(10);
                        result.Node = node;
                        result.Instance = instance;
                        result.Replica = replica;
                        result.DebugCount = random.Next(1000);
                        result.VerboseCount = result.DebugCount / 2;
                        result.InfoCount = result.DebugCount / 3;
                        result.WarningCount = result.DebugCount / 4;
                        result.ErrorCount = result.DebugCount / 5;

                        results.Add(result);
                    }
                }
            }

            return results.AsQueryable();
        }

        #endregion
    }
}
