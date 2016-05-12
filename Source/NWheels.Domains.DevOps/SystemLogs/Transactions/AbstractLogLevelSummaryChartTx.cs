using System;
using System.Collections.Generic;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Processing;
using NWheels.UI;
using NWheels.UI.Factories;
using NWheels.UI.Toolbox;

namespace NWheels.Domains.DevOps.SystemLogs.Transactions
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public abstract class AbstractLogLevelSummaryChartTx : TransactionScript<Empty.Context, ILogTimeRangeCriteria, ChartData>
    {
    }
}
