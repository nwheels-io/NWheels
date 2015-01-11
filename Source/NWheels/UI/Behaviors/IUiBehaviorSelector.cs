using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;

namespace NWheels.UI.Behaviors
{
    public interface IUiBehaviorSelector<TModel, TState, TInput> : IUiElementBuilder
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class UiBehaviorChoices
    {
        public static IMoveUiBehaviorBuilder<TModel, TState, TInput> Move<TModel, TState, TInput>(
            this IUiBehaviorSelector<TModel, TState, TInput> selector)
        {
            return selector.CreateChildBuilder<IMoveUiBehaviorBuilder<TModel, TState, TInput>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ICallUiBehaviorBuilder<TModel, TState, TInput> Call<TModel, TState, TInput>(this IUiBehaviorSelector<TModel, TState, TInput> selector)
        {
            return selector.CreateChildBuilder<ICallUiBehaviorBuilder<TModel, TState, TInput>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IAlertUiBehaviorBuilder<TModel, TState, TInput> Alert<TModel, TState, TInput>(
            this IUiBehaviorSelector<TModel, TState, TInput> selector)
        {
            return selector.CreateChildBuilder<IAlertUiBehaviorBuilder<TModel, TState, TInput>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static IRemoveBoundItemUiBehaviorBuilder<TModel, TState, TInput> RemoveBoundItem<TModel, TState, TInput>(
            this IUiBehaviorSelector<TModel, TState, TInput> selector)
        {
            return selector.CreateChildBuilder<IRemoveBoundItemUiBehaviorBuilder<TModel, TState, TInput>>();
        }
    }
}
