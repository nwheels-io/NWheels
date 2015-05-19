using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Core
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
