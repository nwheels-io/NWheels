using Autofac;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities;
using NWheels.Stacks.MongoDb.SystemLogs.Domain.Transactions;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;

namespace NWheels.Stacks.MongoDb.SystemLogs
{
    public class SystemLogUIFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().ObjectContracts().Concretize<ILogLevelSummaryEntity>().With<LogLevelSummaryEntity>();

            //builder.Register(
            //    c => (AbstractLogLevelSummaryChartTx)new LogLevelSummaryChartTx()
            //).As<AbstractLogLevelSummaryChartTx, ITransactionScript>();
            
            //builder.Register(
            //    c => (AbstractLogLevelSummaryListTx)new LogLevelSummaryListTx(c.Resolve<IFramework>())
            //).As<AbstractLogLevelSummaryListTx, ITransactionScript>();

            builder.NWheelsFeatures().UI().RegisterEntityHandlerExtension<LogLevelSummaryEntityHandlerExtension>();

            builder.NWheelsFeatures().Processing().RegisterTransactionScript<AbstractLogLevelSummaryChartTx, LogLevelSummaryChartTx>();
            builder.NWheelsFeatures().Processing().RegisterTransactionScript<AbstractLogLevelSummaryListTx, LogLevelSummaryListTx>();

            builder.RegisterType<MongoDbThreadLogQueryService>().AsSelf().SingleInstance();
        }

        #endregion
    }
}
