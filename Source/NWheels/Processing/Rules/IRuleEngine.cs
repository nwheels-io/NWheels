using NWheels.Processing.Rules.Core;

namespace NWheels.Processing.Rules
{
    public interface IRuleEngine
    {
        RuleSystemDescription DescribeRuleSystem<TCodeBehind, TDataContext>()
             where TCodeBehind : IRuleSystemCodeBehind<TDataContext>;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        CompiledRuleSystem<TDataContext> CompileRuleSystem<TCodeBehind, TDataContext>(RuleSystemData rules)
            where TCodeBehind : IRuleSystemCodeBehind<TDataContext>;
    }
}
