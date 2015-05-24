using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Rules.Core
{
    public class RuleVariable<TDataContext, TValue> : IRuleVariable<TDataContext, TValue>
    {
        private readonly Func<TDataContext, TValue> _onGetValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RuleVariable(Func<TDataContext, TValue> onGetValue, string idName, string description)
        {
            _onGetValue = onGetValue;
            
            this.IdName = idName;
            this.Description = description;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TValue GetValue(TDataContext context)
        {
            return _onGetValue(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string IdName { get; private set; }
        public string Description { get; private set; }
    }
}
