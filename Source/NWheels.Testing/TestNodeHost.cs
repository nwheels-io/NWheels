using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Hosting;
using NWheels.Hosting.Core;

namespace NWheels.Testing
{
    public class TestNodeHost : NodeHost
    {
        public TestNodeHost(BootConfiguration bootConfig, Action<ContainerBuilder> registerHostComponents = null)
            : base(bootConfig, registerHostComponents)
        {
        }
    }
}
