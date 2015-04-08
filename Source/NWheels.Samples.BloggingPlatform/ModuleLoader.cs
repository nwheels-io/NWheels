using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Modules.Security;
using NWheels.Samples.BloggingPlatform.Apps;
using NWheels.Samples.BloggingPlatform.Domain;
using NWheels.UI.Endpoints;

namespace NWheels.Samples.BloggingPlatform
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Entities().Concretize<IUserAccountEntity>().With<IBlogUserAccountEntity>();
            builder.NWheelsFeatures().Entities().DataRepository<IBlogDataRepository>(initializeStorageOnStartup: true);
            builder.NWheelsFeatures().UI().Application<BlogApp>().WebEndpoint();
        }
    }
}
