using System.Collections.Generic;

namespace NWheels.Processing.Rules.Core
{
    public interface IRuleActionContext<TDataContext>
    {
        TDataContext Data { get; }
        string RuleSetIdName { get; }
        string RuleSetDescription { get; }
        string RuleIdName { get; }
        string RuleDescription { get; }
        IReadOnlyList<string> ArgumentDescriptions { get; }
    }
}
