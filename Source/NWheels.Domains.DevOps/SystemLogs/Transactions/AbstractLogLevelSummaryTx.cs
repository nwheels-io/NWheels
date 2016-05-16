using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Processing;
using NWheels.UI;
using NWheels.UI.Factories;
using NWheels.UI.Toolbox;

namespace NWheels.Domains.DevOps.SystemLogs.Transactions
{
    [TransactionScript(SupportsInitializeInput = true, SupportsPreview = false)]
    public abstract class AbstractLogLevelSummaryTx : TransactionScript<Empty.Context, ILogTimeRangeCriteria, IQueryable<ILogLevelSummaryEntity>>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AbstractLogLevelSummaryTx(IFramework framework, IViewModelObjectFactory viewModelFactory)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Context,ILogTimeRangeCriteria,ChartData>

        public override ILogTimeRangeCriteria InitializeInput(Empty.Context context)
        {
            var criteria = _viewModelFactory.NewEntity<ILogTimeRangeCriteria>();
            var now = _framework.UtcNow;

            criteria.From = now.Date;
            criteria.Until = now.Date.AddDays(1).AddSeconds(-1);

            return criteria;
        }

        #endregion
    }
}
