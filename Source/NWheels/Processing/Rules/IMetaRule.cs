using NWheels.Processing.Core;
using NWheels.Processing.Rules.Core;

namespace NWheels.Processing.Rules
{
    public interface IMetaRule : IParameterizedRuleDomainObject
    {
        RuleSystemDescription.Rule[] CreateRules(string[] parameterValues);
    }
}
