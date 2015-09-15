using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Exceptions;
using NWheels.Logging;

namespace NWheels.Processing.Rules.Core
{
    public interface IRuleEngineLogger : IApplicationEventLogger
    {
        [LogActivity]
        ILogActivity BuildingRuleSystemDescription(Type context, Type codeBehind);

        [LogActivity]
        ILogActivity CompilingRuleSystem(Type context, Type codeBehind);

        [LogError]
        ArgumentException DuplicateDomainObjectName(string idName, Type duplicate, Type existing);

        [LogActivity]
        ILogActivity RunRuleSystem(string name, string context);

        [LogActivity]
        ILogActivity RunRuleSet(int index, string name);

        [LogActivity]
        ILogActivity RunRule(int index, string name);

        [LogDebug]
        void RuleConditionEvaluated(bool result);

        [LogActivity]
        ILogActivity ApplyRuleActions();

        [LogActivity]
        ILogActivity ApplyRuleAction(int index, string name);

        [LogVerbose]
        void ExitRuleSetOnFirstRuleMatch();

        [LogError]
        BusinessRuleException NoRuleMatchedInRuleSet(string name);

        [LogVerbose]
        void RuleSetPreconditionEvaluated(bool result);
    }
}
