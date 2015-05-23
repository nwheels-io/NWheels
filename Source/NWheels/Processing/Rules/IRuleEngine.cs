using NWheels.Processing.Rules.Core;

namespace NWheels.Processing.Rules
{
    public interface IRuleEngine
    {
        CompiledRuleSystem<TDataContext> CompileRuleSystem<TCodeBehind, TDataContext>(RuleSystemData rules)
            where TCodeBehind : IRuleSystemCodeBehind<TDataContext>;
    }
}
