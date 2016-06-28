using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;
using NWheels.Processing.Commands;
using NWheels.Processing.Commands.Impl;
using NWheels.Processing.Documents.Core;
using NWheels.Processing.Documents.Impl;
using NWheels.Processing.Messages;
using NWheels.Processing.Messages.Impl;

namespace NWheels.Processing
{
    public class DocumentFormattersFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Processing().RegisterActor<DocumentFormatActor>().SingleInstance();
            builder.RegisterType<CsvOutputDocumentFormatter>().As<IOutputDocumentFormatter>();
            builder.RegisterType<ZipInputDocumentParser>().As<IInputDocumentParser>();
        }

        #endregion
    }
}
