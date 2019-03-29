using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace NWheels.UI.Model
{

    public class DataGridProps<T> : PropsOf<DataGrid<T>>
    {
        public IList<DataGridColumnProps<T>> Columns;

        public DataSource<T> DataSource;

        public IDataGridEditor<T> Editor;

        public IDataGridAppender<T> Appender;

        public IDataGridFilter<T> Filter;
        
        public IDataGridPager<T> Pager;

        public DataGridProps<T> WithDataSource(DataSource<T> dataSource) => default;
        public DataGridProps<T> WithAutoColumns() => default;
        public DataGridProps<T> WithColumns(params Func<T, object>[] fields) => default;
        public DataGridProps<T> WithInlineEditor() => default;
        public DataGridProps<T> WithAppenderRow() => default;
        public DataGridProps<T> WithAppenderForm(Form<T> form) => default;
    }

    public class DataGridColumnProps<T>
    {
        public Func<T, object> Expression;
        public string Title;
    }

    public class DataGridState<T>
    {
        public readonly IEnumerable<T> LocalBuffer;
        public DataGridState<T> SetLocalBuffer(IEnumerable<T> localBuffer) => null;
    }

    public interface IDataGridEditor<T>
    {
    }

    public interface IDataGridAppender<T>
    {
    }

    public interface IDataGridFilter<T>
    {
    }

    public interface IDataGridPager<T>
    {
    }

    public class DataGrid<T> : UIComponent<DataGridProps<T>, DataGridState<T>>
    {
        public DataGrid(Action<DataGridProps<T>> setProps)
        {
        }
    }
}
