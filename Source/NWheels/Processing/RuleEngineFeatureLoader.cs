using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;
using NWheels.Processing.Rules;
using NWheels.Processing.Rules.Core;
using NWheels.Processing.Rules.Impl;

namespace NWheels.Processing
{
    public class RuleEngineFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Logging().RegisterLogger<IRuleEngineLogger>();
            builder.RegisterType<RealRuleEngine>().As<IRuleEngine>().SingleInstance();
        }

        #endregion
    }
}
