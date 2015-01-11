using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;

namespace NWheels.UI.Widgets
{
    public interface ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> : IVisualUiElementBuilder<TModel, TState, ILookupFieldUiWidgetBuilder<TModel, TState, TLookup>>
    {
        ILookupFieldUiWidgetBuilder<TModel, TState, TNewLookup> BindLookupTo<TNewLookup>();
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> LookupFilter(Expression<Func<TLookup, bool>> filter);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> LookupFilter(Expression<Func<TModel, TLookup, bool>> filter);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> LookupFilter(Expression<Func<TModel, TState, TLookup, bool>> filter);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> LookupQuery(Expression<Func<IQueryable<TLookup>, IQueryable<TLookup>>> query);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> LookupQuery(Expression<Func<TModel, IQueryable<TLookup>, IQueryable<TLookup>>> query);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> LookupQuery(Expression<Func<TModel, TState, IQueryable<TLookup>, IQueryable<TLookup>>> query);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> LookupLabel(Expression<Func<TLookup, object>> labelPath);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> LookupImage(Expression<Func<TLookup, string>> imagePath);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> DefaultToEmpty();
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> DefaultToFirstItem();
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> DefaultToAll();
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> BindSingleValueTo(Expression<Func<IUiScope<TModel, TState>, TLookup>> path);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> BindMultipleValuesTo(Expression<Func<IUiScope<TModel, TState>, ICollection<TLookup>>> path);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> FilterMultipleValuesBy(Expression<Func<TLookup, bool>> valueSubsetFilter = null);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> StyleTagsDropDown(bool useAllTag);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> StyleCheckboxList(int columns);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> StyleRadioButtons(int columns);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> StyleFlowGrid(int columns);
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> ItemStyleText();
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> ItemStyleImage();
        ILookupFieldUiWidgetBuilder<TModel, TState, TLookup> ItemStyleTextAndImage();
    }
}
