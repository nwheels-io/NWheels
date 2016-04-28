using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;
using NWheels.Processing.Messages.Impl;

namespace NWheels.Processing
{
    public class SmptEmailSenderFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Processing().RegisterActor<SmtpOutgoingEmailActor>();
            builder.NWheelsFeatures().Configuration().RegisterSection<SmtpOutgoingEmailActor.IConfigSection>();
            builder.NWheelsFeatures().Logging().RegisterLogger<SmtpOutgoingEmailActor.ILogger>();
        }

        #endregion
    }
}
