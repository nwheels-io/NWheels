using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Core;

namespace NWheels.Processing
{
    public interface IMetaRule : IParameterizedRuleDomainObject
    {
        RuleSystemDescription.Rule[] CreateRules(string[] parameterValues);
    }
}
