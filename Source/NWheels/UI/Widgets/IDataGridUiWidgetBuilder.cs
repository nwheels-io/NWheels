using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;
using NWheels.UI.Layouts;

namespace NWheels.UI.Widgets
{
    public interface IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> : 
        IVisualUiElementBuilder<TModel, TState, IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState>>
    {
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> HeadersInvisible();
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> RowsExpandable();
        IDataGridUiWidgetBuilder<TModel, TState, TNewRowModel, TRowState> BindRowsTo<TNewRowModel>(
            Expression<Func<IUiScope<TModel, TState>, IEnumerable<TNewRowModel>>> path);
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TNewRowState> BindRowUiStateTo<TNewRowState>();
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TNewRowState> BindRowUiStateTo<TNewRowState>(
            Expression<Func<TState, IEnumerable<TNewRowState>>> path);
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> Column(string title, Expression<Func<IUiScope<TRowModel, TRowState>, object>> valueBinding);
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> Column(string title, string valueFormat, params Expression<Func<IUiScope<TRowModel, TRowState>, object>>[] valueFormatArgs);
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> ColumnAlignRight(string title, Expression<Func<IUiScope<TRowModel, TRowState>, object>> valueBinding);
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> ColumnAlignRight(string title, string valueFormat, params Expression<Func<IUiScope<TRowModel, TRowState>, object>>[] valueFormatArgs);
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> ColumnAlignCenter(string title, Expression<Func<IUiScope<TRowModel, TRowState>, object>> valueBinding);
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> ColumnAlignCenter(string title, string valueFormat, params Expression<Func<IUiScope<TRowModel, TRowState>, object>>[] valueFormatArgs);
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> Column(Action<IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState>> definition);
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> RowExpandedView(Action<IUiLayoutBuilder<TRowModel, TRowState>> definition);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> : 
        IVisualUiElementBuilder<TModel, TState, IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState>>
    {
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> Title(string titleText);
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> Title(Expression<Func<IUiScope<TModel, TState>, string>> titleTextBinding);
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> Title(string format, params Expression<Func<IUiScope<TModel, TState>, string>>[] args);
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> BindTo(string format, params Expression<Func<IUiScope<TRowModel, TRowState>, object>>[] args);
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> BindTo(Expression<Func<IUiScope<TRowModel, TRowState>, object>> path);
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> AlignRight();
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> AlignCenter();
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> TemplateHeader(Action<IUiLayoutBuilder<TRowModel, TRowState>> definition);
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> TemplateItem(Action<IUiLayoutBuilder<TRowModel, TRowState>> definition);
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> TemplateItemAlternate(Action<IUiLayoutBuilder<TRowModel, TRowState>> definition);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDataGridRowUiState
    {
        int Index { get; }
    }
}
