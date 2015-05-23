namespace NWheels.Processing.Rules.Core
{
    public interface IMetaRule : IParameterizedRuleDomainObject
    {
        RuleSystemDescription.Rule[] CreateRules(string[] parameterValues);
    }
}
