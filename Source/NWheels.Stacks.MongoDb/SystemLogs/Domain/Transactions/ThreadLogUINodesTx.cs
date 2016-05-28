using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Extensions;
using NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;
using NWheels.UI.Factories;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions
{
    public class ThreadLogUINodesTx : AbstractThreadLogUINodesTx
    {
        private readonly IFramework _framework;
        private readonly MongoDbThreadLogQueryService _queryService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLogUINodesTx(IFramework framework, IViewModelObjectFactory viewModelFactory, MongoDbThreadLogQueryService queryService)
            : base(framework, viewModelFactory)
        {
            _framework = framework;
            _queryService = queryService;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Context,IThreadLogSearchCriteria,IQueryable<IThreadLogUINodeEntity>>

        public override IQueryable<IRootThreadLogUINodeEntity> Execute(IThreadLogSearchCriteria input)
        {
            var task = _queryService.QueryThreadLogsAsync(input, CancellationToken.None);
            task.Wait();

            var results = new List<IRootThreadLogUINodeEntity>();
            int treeNodeIndex = 0;

            foreach (var record in task.Result.OrderBy(r => r.Timestamp))
            {
                var result = _framework.NewDomainObject<IRootThreadLogUINodeEntity>().As<RootThreadLogUINodeEntity>();
                result.CopyFormRecord(ref treeNodeIndex, record);
                results.Add(result);
            }

            return results.AsQueryable();
        }

        #endregion
    }
}
