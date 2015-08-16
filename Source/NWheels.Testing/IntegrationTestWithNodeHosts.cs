using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;
using NUnit.Framework;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging.Core;
using NWheels.Testing.Entities.Impl;

namespace NWheels.Testing
{
    [TestFixture, Category(TestCategory.System)]
    public abstract class IntegrationTestWithNodeHosts : TestFixtureWithNodeHosts
    {
    }
}
