using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;

namespace NWheels.Samples.MyHRApp
{
    public class ModuleLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
        }

        #endregion
    }
}