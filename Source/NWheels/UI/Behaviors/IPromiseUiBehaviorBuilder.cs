using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Behaviors
{
    public interface IPromiseUiBehaviorBuilder<TModel, TState, TPromise> : IBoundUiElementBuilder<TModel, TState>
    {
        IUiBehaviorSelector<TModel, TState> Then(Func<IUiBehaviorSelector<TModel, TState>, IUiBehaviorBuilder<TModel, TState>> onError = null);
        IUiCommandBuilder<TModel, TState> End();
    }
}
