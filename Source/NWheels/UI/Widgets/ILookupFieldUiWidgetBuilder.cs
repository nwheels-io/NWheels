using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Widgets
{
    public interface ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> : 
        IBoundValueUiElementBuilder<TModel, TState, ILookupFieldUiWidgetBuilder<TModel, TState, TLookup>>
    {
        ILookupFieldUiWidgetBuilder<TModel, TState, TNewLookup> BindLookupTo<TNewLookup>();
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> LookupLabel(Expression<Func<TLookup, object>> path);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> LookupFilter(Expression<Func<TModel, TLookup, bool>> filter);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> DefaultToEmpty();
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> DefaultToFirstItem();
    }
}
