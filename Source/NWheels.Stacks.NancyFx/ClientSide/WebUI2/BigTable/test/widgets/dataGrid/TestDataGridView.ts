///<reference path="../../../lib/typings/jasmine/jasmine.d.ts" />

namespace UIDL.Widgets.DataGrid.Tests
{
    export class TestDataGridView extends UIDLDataGrid {
        public constructor() {
            super();
            this.changeLog = [];
        }

        //-------------------------------------------------------------------------------------------------------------

        public setBinding(binding: DataGrid.IDataGridBinding): void {
            super.setBinding(binding);
            binding.changed().bind(args => {
                this.changeLog.push(args);
            });
        }

        //-------------------------------------------------------------------------------------------------------------

        public changeLog: DataGrid.DataGridRowsChangedEventArgs[];
    }
}