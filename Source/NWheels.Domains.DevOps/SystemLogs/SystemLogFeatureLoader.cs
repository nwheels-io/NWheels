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
            // nothing
        }

        #endregion
    }
}
