using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Processing.Rules.Core;

namespace NWheels.Processing.Rules.Impl
{
    public class RealRuleEngine : IRuleEngine
    {
        private readonly IFramework _framework;
        private readonly IComponentContext _components;
        private readonly IRuleEngineLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RealRuleEngine(IFramework framework, IComponentContext components, IRuleEngineLogger logger)
        {
            _framework = framework;
            _components = components;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRuleEngine

        public RuleSystemDescription DescribeRuleSystem<TCodeBehind, TDataContext>() 
            where TCodeBehind : IRuleSystemCodeBehind<TDataContext>
        {
            var builder = new RuleSystemBuilder<TDataContext>(typeof(TCodeBehind), _logger);
            var codeBehind = _components.Resolve<TCodeBehind>();

            codeBehind.BuildRuleSystem(builder);

            using ( _logger.BuildingRuleSystemDescription(context: typeof(TDataContext), codeBehind: codeBehind.GetType()) )
            {
                return builder.GetDescription();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompiledRuleSystem<TDataContext> CompileRuleSystem<TCodeBehind, TDataContext>(RuleSystemData rules) 
            where TCodeBehind : IRuleSystemCodeBehind<TDataContext>
        {
            var builder = new RuleSystemBuilder<TDataContext>(typeof(TCodeBehind), _logger);
            var codeBehind = _components.Resolve<TCodeBehind>();

            codeBehind.BuildRuleSystem(builder);

            using ( _logger.CompilingRuleSystem(context: typeof(TDataContext), codeBehind: codeBehind.GetType()) )
            {
                return builder.CompileRuleSystem(rules);
            }
        }

        #endregion
    }
}
