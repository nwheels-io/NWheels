using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public class BindingSelector<TModel, TState, TValue> : IBindingSelector<TModel, TState, TValue>
    {
        #region Implementation of IBindingSelector<TModel,TState,TValue>

        public void ToModel(Expression<Func<TModel, TValue>> modelProperty)
        {
            //
        }

        public void ToState(Expression<Func<TState, TValue>> stateProperty)
        {
            //
        }

        #endregion
    }
}
