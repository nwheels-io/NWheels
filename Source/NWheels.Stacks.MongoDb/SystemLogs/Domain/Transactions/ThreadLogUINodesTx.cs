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
using NWheels.UI;
using NWheels.UI.Factories;
using NWheels.Utilities;

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

        public override IQueryable<IThreadLogUINodeEntity> Execute(IThreadLogSearchCriteria input)
        {
            int detailTreeNodeIndex;
            var isDetailQuery = TryParseDetailQuery(input, out detailTreeNodeIndex);

            var task = _queryService.QueryThreadLogsAsync(input, CancellationToken.None);
            task.Wait();

            var results = new List<IThreadLogUINodeEntity>();

            foreach (var record in task.Result.OrderBy(r => r.Timestamp))
            {
                var result = _framework.NewDomainObject<IRootThreadLogUINodeEntity>().As<RootThreadLogUINodeEntity>();
                result.CopyFormThreadRecord(record);
                results.Add(result);
            }

            if (isDetailQuery && results.Count == 1)
            {
                ThreadLogUINodeEntity detailNode;

                var queryByExample = _framework.NewDomainObject<IThreadLogUINodeEntity>().As<ThreadLogUINodeEntity>();
                queryByExample.SetQueryByExample(detailTreeNodeIndex);

                if (results[0].As<ThreadLogUINodeEntity>().TryFindTreeNodeByIndex(queryByExample, out detailNode))
                {
                    detailNode.BuildDetails();
                    detailNode.ClearChildren();
                    return new IThreadLogUINodeEntity[] { detailNode }.AsQueryable();
                }
            }

            return results.AsQueryable();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryParseDetailQuery(IThreadLogSearchCriteria input, out int treeNodeIndex)
        {
            treeNodeIndex = -1;
            var uiContext = UIOperationContext.Current;

            if (uiContext != null && IsLogNodeDetailsQuery(uiContext))
            {
                var idFilterItem = uiContext.Query.Filter.FirstOrDefault(
                    f => f.PropertyName.EqualsIgnoreCase(ApplicationEntityService.QueryOptions.IdPropertyName)
                );

                if (idFilterItem != null && !string.IsNullOrEmpty(idFilterItem.StringValue))
                {
                    uiContext.Query.Filter.Remove(idFilterItem);

                    var idFromQuery = idFilterItem.StringValue;
                    var hashPosition = idFromQuery.LastIndexOf('#');

                    if (hashPosition > 0 && hashPosition < idFromQuery.Length - 1)
                    {
                        input.Id = idFromQuery.Substring(0, hashPosition);
                        return Int32.TryParse(idFromQuery.Substring(hashPosition + 1), out treeNodeIndex);
                    }
                }
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_detailsPropertyName =
            ExpressionUtility.GetPropertyInfoFrom<IRootThreadLogUINodeEntity>(x => x.Details).Name;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static bool IsLogNodeDetailsQuery(UIOperationContext uiContext)
        {
            if (uiContext.Query.SelectPropertyNames == null || uiContext.Query.SelectPropertyNames.Count != 1)
            {
                return false;
            }

            var singleSelectItem = uiContext.Query.SelectPropertyNames[0];

            if (singleSelectItem != null && singleSelectItem.PropertyPath != null && singleSelectItem.PropertyPath.Count == 1)
            {
                return singleSelectItem.PropertyPath[0].EqualsIgnoreCase(_s_detailsPropertyName);
            }

            return false;
        }
    }
}
