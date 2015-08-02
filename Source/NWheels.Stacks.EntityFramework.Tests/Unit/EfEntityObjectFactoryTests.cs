using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Testing;
using NWheels.Testing.Entities.Stacks;
using NWheels.Stacks.EntityFramework.Factories;

namespace NWheels.Stacks.EntityFramework.Tests.Unit
{
    [TestFixture]
    public class EfEntityObjectFactoryTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void CanCreateEntityObjects()
        {
            FactoryOperations.Repository1.ExecuteEntityCreation(Framework, CreateEntityObjectFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EntityObjectFactory CreateEntityObjectFactory()
        {
            return new EfEntityObjectFactory(Framework.Components, base.DyamicModule, (TypeMetadataCache)Framework.MetadataCache);
        }
    }
}
