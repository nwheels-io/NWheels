using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.Domains.Security;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Testing;

namespace NWheels.Stacks.ODataBreeze.SystemTests
{
    [TestFixture]
    public class BreezeApiControllerFactoryTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void CanCreateApiController()
        {
            //-- arrange

            base.Framework.UpdateComponents(builder => builder.NWheelsFeatures().Logging().RegisterLogger<IBreezeEndpointLogger>());
            var factory = 
                new BreezeApiControllerFactory(base.DyamicModule, base.Framework.MetadataCache, base.Resolve<IDomainObjectFactory>(), base.Resolve<IEntityObjectFactory>());

            //-- act

            var controllerType = factory.CreateControllerType(dataRepositoryContract: typeof(IUserAccountDataRepository));
            TestHelpers.UpdateComponentContainer(Framework.Components, controllerType);
            var controllerInstance = Framework.Components.Resolve(controllerType);

            //-- assert

            Assert.That(controllerInstance, Is.InstanceOf<BreezeApiControllerBase<IUserAccountDataRepository>>());
        }
    }
}
