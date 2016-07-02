namespace UIDL.Widgets.DataGrid
{
    export class LocalDataGridBinding extends DataGridBindingBase  {
        private _rows: Object[];

        //-------------------------------------------------------------------------------------------------------------

        public constructor(rows: Object[]) {
            super();
            this._rows = rows.slice(0);
        }

        //-------------------------------------------------------------------------------------------------------------

        public renderRow(index: number, el: HTMLTableRowElement): void {
            // nothing
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowCount(): number {
            return this._rows.length;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRealRowCount(): number {
            return this._rows.length;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowDataAt(index: number): Object {
            return this._rows[index];
        }

        //-------------------------------------------------------------------------------------------------------------

        public getAllRowsData(): Object[] {
            return this._rows;
        }
    }
}