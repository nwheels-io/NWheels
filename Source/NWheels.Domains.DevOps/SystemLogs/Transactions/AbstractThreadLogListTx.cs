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
    public abstract class AbstractThreadLogListTx : TransactionScript<Empty.Context, ILogTimeRangeCriteria, IQueryable<IThreadLogEntity>>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AbstractThreadLogListTx(IFramework framework, IViewModelObjectFactory viewModelFactory)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Context,ILogTimeRangeCriteria,IQueryable<ILogMessageEntity>>

        public override ILogTimeRangeCriteria InitializeInput(Empty.Context context)
        {
            var criteria = _viewModelFactory.NewEntity<ILogTimeRangeCriteria>();
            var now = _framework.UtcNow;
            var timeRange = TimeRangePreset.LastHour.GetIntervalRelativeTo(now);

            criteria.From = timeRange.LowerBound;
            criteria.Until = timeRange.UpperBound;

            return criteria;
        }

        #endregion
    }
}
