using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Widgets
{
    public interface IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> : IBoundUiElementBuilder<TModel, TState>
    {
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> HeadersInvisible();
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TRowState> RowsExpandable();
        IDataGridUiWidgetBuilder<TModel, TState, TNewRowModel, TRowState> BindRowsToModel<TNewRowModel>(
            Expression<Func<TModel, IEnumerable<TNewRowModel>>> path);
        IDataGridUiWidgetBuilder<TModel, TState, TNewRowModel, TRowState> BindRowsToUiState<TNewRowModel>(
            Expression<Func<TState, IEnumerable<TNewRowModel>>> path);
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TNewRowState> BindRowUiStateTo<TNewRowState>();
        IDataGridUiWidgetBuilder<TModel, TState, TRowModel, TNewRowState> BindRowUiStateTo<TNewRowState>(
            Expression<Func<TState, IEnumerable<TNewRowState>>> path);
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> ColumnAdd();
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> ColumnLast();
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> Column(int index);
        IUiLayoutBuilder<TRowModel, TRowState> RowExpandedView();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> : IBoundUiElementBuilder<TModel, TState>
    {
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> BindTo(string format, params Expression<Func<IUiScope<TRowModel, TRowState>, object>>[] paths);
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> BindTo(Expression<Func<IUiScope<TRowModel, TRowState>, object>> path);
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> AlignRight();
        IDataGridColumnBuilder<TModel, TState, TRowModel, TRowState> AlignCenter();
        IUiLayoutBuilder<TRowModel, TRowState> TemplateHeader();
        IUiLayoutBuilder<TRowModel, TRowState> TemplateItem();
        IUiLayoutBuilder<TRowModel, TRowState> TemplateItemAlternate();
    }
}
