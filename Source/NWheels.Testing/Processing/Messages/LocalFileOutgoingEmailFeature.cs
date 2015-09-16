using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;

namespace NWheels.Testing.Processing.Messages
{
    public class LocalFileOutgoingEmailFeature : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Processing().RegisterActor<LocalFileOutgoingEmailActor>().SingleInstance();
        }

        #endregion
    }
}
