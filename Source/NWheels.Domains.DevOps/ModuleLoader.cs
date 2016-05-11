using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Domains.DevOps
{
    public class ModuleLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            // nothing
        }

        #endregion
    }
}
