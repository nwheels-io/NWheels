using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using NWheels.Domains.Security;
using NWheels.Domains.Security.UI;
using NWheels.Extensions;
using NWheels.Samples.MyMusicDB.Authorization;
using NWheels.Samples.MyMusicDB.Deployment;
using NWheels.Samples.MyMusicDB.Domain;
using NWheels.Samples.MyMusicDB.UI;
using NWheels.Stacks.MongoDb;

namespace NWheels.Samples.MyMusicDB
{
    public class ModuleLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Entities().RegisterDataPopulator<SecurityPopulator>().FirstInPipeline();
            builder.NWheelsFeatures().Entities().RegisterDataPopulator<DemoDataPopulator>().LastInPipeline();
            builder.NWheelsFeatures().Entities().RegisterDataRepository<IMusicDBContext>().WithInitializeStorageOnStartup();

            builder.NWheelsFeatures().ObjectContracts().Concretize<IUserAccountDataRepository>().With<IMusicDBContext>();
            builder.NWheelsFeatures().ObjectContracts().Concretize<IAutoIncrementIdDataRepository>().With<IMusicDBContext>();
            builder.NWheelsFeatures().ObjectContracts().Concretize<IAdministratorAcl>().With<AdministratorAcl>();
            builder.NWheelsFeatures().ObjectContracts().Concretize<IOperatorAcl>().With<OperatorAcl>();

            builder.NWheelsFeatures().ObjectContracts().Concretize<IGenreRelation>().With<GenreRelation>();
            builder.NWheelsFeatures().ObjectContracts().Concretize<ITrackEntity>().With<TrackEntity>();
            
            builder.NWheelsFeatures().Processing().RegisterTransactionScript<InteractiveLoginTx>();
            builder.NWheelsFeatures().Processing().RegisterTransactionScript<DashboardQueryTx>();
            
            builder.NWheelsFeatures().UI().RegisterApplication<MusicDBApp>().WithWebEndpoint();
            
            builder.RegisterType<MusicDBApiEndpoint>();
            builder.NWheelsFeatures().Api().RegisterContract<MusicDBApiEndpoint>().WithHttpApiEndpoint();
            builder.NWheelsFeatures().Logging().RegisterLogger<IApiRequestLogger>();
            builder.NWheelsFeatures().Authorizarion().RegisterAnonymousEntityAccessRule<AnonymousAcl>();
        }

        #endregion
    }
}