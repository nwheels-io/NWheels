namespace NWheels.Processing.Core
{
    public interface IRuleSystemCodeBehind<TDataContext>
    {
        void BuildRuleSystem(RuleSystemBuilder<TDataContext> builder);
    }
}
