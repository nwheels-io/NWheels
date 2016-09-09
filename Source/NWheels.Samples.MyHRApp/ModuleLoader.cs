using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using NWheels.Domains.Security;
using NWheels.Extensions;
using NWheels.Samples.MyHRApp.Deployment;
using NWheels.Samples.MyHRApp.Domain;
using NWheels.Samples.MyHRApp.UI;
using NWheels.Stacks.MongoDb;

namespace NWheels.Samples.MyHRApp
{
    public class ModuleLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Entities().RegisterDataPopulator<HRContextPopulator>().FirstInPipeline();
            builder.NWheelsFeatures().Entities().RegisterDataRepository<IHRContext>().WithInitializeStorageOnStartup();

            builder.NWheelsFeatures().ObjectContracts().Concretize<IUserAccountDataRepository>().With<IHRContext>();
            builder.NWheelsFeatures().ObjectContracts().Concretize<IAutoIncrementIdDataRepository>().With<IHRContext>();

            builder.NWheelsFeatures().Processing().RegisterTransactionScript<HRLoginTx>();
            builder.NWheelsFeatures().UI().RegisterApplication<HRApp>().WithWebEndpoint();
        }

        #endregion
    }
}