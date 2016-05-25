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
    public abstract class AbstractThreadLogUINodesTx : TransactionScript<Empty.Context, IThreadLogSearchCriteria, IQueryable<IRootThreadLogUINodeEntity>>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AbstractThreadLogUINodesTx(IFramework framework, IViewModelObjectFactory viewModelFactory)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Context,ILogTimeRangeCriteria,IQueryable<ILogMessageEntity>>

        public override IThreadLogSearchCriteria InitializeInput(Empty.Context context)
        {
            return _viewModelFactory.NewEntity<IThreadLogSearchCriteria>();
        }

        #endregion
    }
}
