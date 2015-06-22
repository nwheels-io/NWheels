using System;
using System.Collections.Generic;

namespace NWheels.UI.OldToolbox
{
    public class Table<TItem> : WidgetComponent<Table<TItem>, Table<TItem>.IData, Table<TItem>.IState>
        where TItem : class 
    {
        public override void DescribePresenter(IWidgetPresenter<Table<TItem>, IData, IState> presenter)
        {
            presenter.Bind(view => view.SelectedItem).ToState(state => state.SelectedItem);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DefineColumns(params Func<TItem, object>[] itemProperties)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TItem SelectedItem { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IData : IAccessParentData<IEnumerable<TItem>>
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IState
        {
            TItem SelectedItem { get; set; }
        }
    }
}
