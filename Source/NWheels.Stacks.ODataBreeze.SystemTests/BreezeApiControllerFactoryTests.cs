using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Domains.Security;
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

            var factory = new BreezeApiControllerFactory(base.DyamicModule, base.Framework.MetadataCache);
            base.Framework.UpdateComponents(builder => builder.NWheelsFeatures().Logging().RegisterLogger<IBreezeEndpointLogger>());

            //-- act

            var controllerType = factory.CreateControllerType(dataRepositoryContract: typeof(IUserAccountDataRepository));
            TestHelpers.UpdateComponentContainer(Framework.Components, controllerType);
            var controllerInstance = Framework.Components.Resolve(controllerType);

            //-- assert

            Assert.That(controllerInstance, Is.InstanceOf<BreezeApiControllerBase<IUserAccountDataRepository>>());
        }
    }
}
