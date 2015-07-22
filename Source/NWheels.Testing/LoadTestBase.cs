using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NWheels.Testing
{
    [TestFixture, Category(TestCategory.Load)]
    public abstract class LoadTestBase : TestFixtureWithNodeHosts
    {
    }
}
