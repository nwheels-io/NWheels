using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;
using NWheels.Globalization.Core;
using NWheels.Stacks.Apis.MicrosoftTranslator.Domain;
using NWheels.Stacks.Apis.MicrosoftTranslator.UI;

namespace NWheels.Stacks.Apis.MicrosoftTranslator
{
    public class ModuleLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures()
                .UI()
                .RegisterUidlExtension<LocaleCrudScreenPart, LocaleCrudScreenPartExtension>();
            
            builder.NWheelsFeatures()
                .Processing()
                .RegisterTransactionScript<LocaleBulkTranslateTx>();
        }

        #endregion
    }
}
