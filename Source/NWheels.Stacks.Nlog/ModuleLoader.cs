using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging.Core;

namespace NWheels.Stacks.Nlog
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(NLogBasedPlainLog.Instance).As<IPlainLog>();
        }
    }
}
