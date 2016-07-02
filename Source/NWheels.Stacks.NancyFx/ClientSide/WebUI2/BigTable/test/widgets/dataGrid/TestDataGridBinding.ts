///<reference path="../../../lib/typings/jasmine/jasmine.d.ts" />

namespace UIDL.Widgets.DataGrid.Tests
{
    export class TestDataGridBinding<T> extends DataGridBindingBase {
        private _data: T[];

        //-------------------------------------------------------------------------------------------------------------

        public constructor(data: T[]) {
            super();
            this._data = data.slice(0);
        }

        //-------------------------------------------------------------------------------------------------------------

        public renderRow(index: number, el: HTMLTableRowElement): void {
            el.cells[0].innerHTML = this.getRowDataAt(index).toString();
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowCount(): number {
            return this._data.length;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRealRowCount(): number {
            return this._data.length;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowDataAt(index: number): Object {
            return this._data[index];
        }

        //-------------------------------------------------------------------------------------------------------------

        public getAllRowsData(): Object[] {
            return this._data;
        }

        //-------------------------------------------------------------------------------------------------------------

        public insertRows(atIndex: number, data: T[]) {
            let newData: T[] = [];

            for (let i = 0; i < atIndex; i++) {
                newData.push(this._data[i]);
            }
            for (let i = 0; i < data.length; i++) {
                newData.push(data[i]);
            }
            for (let i = atIndex; i < this._data.length; i++) {
                newData.push(this._data[i]);
            }

            this._data = newData;
            let args = new DataGridRowsChangedEventArgs(DataGridRowsChangeType.inserted, atIndex, data.length);
            this.changed().raise(args);
        }

        //-------------------------------------------------------------------------------------------------------------

        public updateRows(atIndex: number, data: T[]) {
            for (let i = 0; i < data.length; i++) {
                this._data[atIndex + i] = data[i];
            }

            let args = new DataGridRowsChangedEventArgs(DataGridRowsChangeType.updated, atIndex, data.length);
            this.changed().raise(args);
        }

        //-------------------------------------------------------------------------------------------------------------

        public deleteRows(atIndex: number, count: number) {
            this._data.splice(atIndex, count);

            let args = new DataGridRowsChangedEventArgs(DataGridRowsChangeType.deleted, atIndex, count);
            this.changed().raise(args);
        }
    }
}