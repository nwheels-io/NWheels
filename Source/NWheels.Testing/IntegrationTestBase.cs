using System;
using Autofac;
using NUnit.Framework;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Testing
{
    [TestFixture, Category(TestCategory.Integration)]
    public abstract class IntegrationTestBase : TestFixtureWithoutNodeHosts
    {
    }
}
