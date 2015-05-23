namespace NWheels.Processing.Rules.Core
{
    public interface IRuleSystemCodeBehind<TDataContext>
    {
        void BuildRuleSystem(RuleSystemBuilder<TDataContext> builder);
    }
}
