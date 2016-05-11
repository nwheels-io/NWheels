using Autofac;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Extensions;

namespace NWheels.Domains.DevOps.SystemLogs
{
    public class SystemLogFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Processing().RegisterTransactionScript<LogLevelSummaryChartTx>();
            builder.NWheelsFeatures().Processing().RegisterTransactionScript<LogLevelSummaryListTx>();
        }

        #endregion
    }
}
