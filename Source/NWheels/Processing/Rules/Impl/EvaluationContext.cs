using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Rules.Core;

namespace NWheels.Processing.Rules.Impl
{
    public class EvaluationContext<TDataContext>
    {
        public EvaluationContext(
            IReadOnlyDictionary<string, RuleSystemDescription.DomainObject> objectDescriptionByIdName,
            IReadOnlyDictionary<string, IRuleDomainObject> runtimeObjectByIdName, 
            TDataContext dataContext)
        {
            this.ObjectDescriptionByIdName = objectDescriptionByIdName;
            this.RuntimeObjectByIdName = runtimeObjectByIdName;
            this.DataContext = dataContext;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyDictionary<string, IRuleDomainObject> RuntimeObjectByIdName { get; private set; }
        public IReadOnlyDictionary<string, RuleSystemDescription.DomainObject> ObjectDescriptionByIdName { get; private set; }
        public TDataContext DataContext { get; private set; }
    }
}
