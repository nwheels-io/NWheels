using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IBound<TTarget, TModel, TState>
    {
        IBindTo<TModel, TState, TValue> Bind<TValue>(Expression<Func<TTarget, TValue>> receiverProperty);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IBindTo<TModel, TState, TValue>
    {
        void ToModel(Expression<Func<TModel, TValue>> modelProperty);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ToState(Expression<Func<TState, TValue>> stateProperty);
    }
}
