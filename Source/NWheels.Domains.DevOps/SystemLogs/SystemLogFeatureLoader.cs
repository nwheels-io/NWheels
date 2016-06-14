using Autofac;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Domains.DevOps.SystemLogs.UI.Formatters;
using NWheels.Extensions;
using NWheels.Processing.Documents.Core;

namespace NWheels.Domains.DevOps.SystemLogs
{
    public class SystemLogFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ThreadLogTextDocumentFormatter>().As<IOutputDocumentFormatter>();
        }

        #endregion
    }
}
