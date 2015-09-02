using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Processing.Messages;
using NWheels.Processing.Messages.Impl;

namespace NWheels
{
    /// <summary>
    /// This class exists in order to allow configuration of optional core features in boot.config.
    /// (because in order to configure a feature, boot.config requires containing module to be configured first).
    /// </summary>
    public class ModuleLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LocalFileContentTemplateProvider>().As<IContentTemplateProvider>().SingleInstance();
        }

        #endregion
    }
}
