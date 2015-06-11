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
        IBindingSourceSelector<TModel, TState, TValue> Bind<TValue>(Expression<Func<TTarget, TValue>> receiverProperty);
        IBehaviorBuilder<Unbound, TModel, TState> OnCommand(Expression<Func<TTarget, ICommand>> commandSelector);
        IBehaviorBuilder<Unbound, TModel, TState> OnNotification<TRepo>(Expression<Action<TRepo>> notificationSelector) 
            where TRepo : IUINotificationRepository;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IBindingSourceSelector<TModel, TState, TValue>
    {
        void ToModel(Expression<Func<TModel, TValue>> modelProperty);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ToState(Expression<Func<TState, TValue>> stateProperty);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ToApi<TContract>(Expression<Func<TContract, TValue>> apiCall);
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ToApi<TContract>(Expression<Func<TContract, TModel, TValue>> apiCall);
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ToApi<TContract>(Expression<Func<TContract, TModel, TState, TValue>> apiCall);
    }
}
