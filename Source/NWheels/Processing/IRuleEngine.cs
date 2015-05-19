using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Core;

namespace NWheels.Processing
{
    public interface IRuleEngine
    {
        CompiledRuleSystem<TDataContext> CompileRuleSystem<TCodeBehind, TDataContext>(RuleSystemData rules)
            where TCodeBehind : IRuleSystemCodeBehind<TDataContext>;
    }
}
