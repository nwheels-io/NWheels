using Autofac;
using NWheels.Extensions;
using NWheels.Processing.Documents.Core;
using NWheels.Processing.Messages;
using NWheels.Processing.Messages.Impl;

namespace NWheels.Stacks.Formats.EPPlus
{
    public class ModuleLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExcelOutputDocumentFormatter>().As<IOutputDocumentFormatter>();
        }

        #endregion
    }
}
