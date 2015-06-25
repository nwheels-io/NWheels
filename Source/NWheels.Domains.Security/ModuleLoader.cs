using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Domains.Security.Impl;
using NWheels.Domains.Security.UI;
using NWheels.Extensions;

namespace NWheels.Domains.Security
{
    public class ModuleLoader : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SecurityDomainApi>().As<ISecurityDomainApi>().SingleInstance();
        }
    }
}
