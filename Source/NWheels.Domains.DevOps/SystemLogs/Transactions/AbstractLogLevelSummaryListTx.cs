using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Processing;
using NWheels.UI;

namespace NWheels.Domains.DevOps.SystemLogs.Transactions
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public abstract class AbstractLogLevelSummaryListTx : TransactionScript<Empty.Context, ILogTimeRangeCriteria, IQueryable<ILogLevelSummaryEntity>>
    {
    }
}
