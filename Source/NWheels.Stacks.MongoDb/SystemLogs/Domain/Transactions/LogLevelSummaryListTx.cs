using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities;
using NWheels.UI;
using NWheels.UI.Factories;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    public class LogLevelSummaryListTx : AbstractLogLevelSummaryListTx
    {
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogLevelSummaryListTx(IFramework framework, IViewModelObjectFactory viewModelFactory)
            : base(framework, viewModelFactory)
        {
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Input,ILogTimeRangeCriteria,IQueryable<ILogLevelSummaryEntity>>

        public override IQueryable<ILogLevelSummaryEntity> Execute(ILogTimeRangeCriteria input)
        {
            var random = new Random();
            var results = new List<ILogLevelSummaryEntity>();

            foreach (var node in new[] { "BackEnd", "ETL", "WebApp1", "WebApp2" })
            {
                foreach (var instance in new[] { "Master", "Slave" })
                {
                    foreach (var replica in new[] { "1", "2", "3" })
                    {
                        var result = _framework.NewDomainObject<ILogLevelSummaryEntity>().As<LogLevelSummaryEntity>();

                        result.Randomize(
                            machine: "QWESVR123" + random.Next(10),
                            environment: "PROD",
                            node: node,
                            instance: instance,
                            replica: replica);

                        results.Add(result);
                    }
                }
            }

            return results.AsQueryable();
        }

        #endregion
    }
}
