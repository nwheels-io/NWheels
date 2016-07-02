namespace UIDL.Widgets.DataGrid
{
    export interface IDataGridBinding {
        attachView(view: UIDLDataGrid);
        renderRow(index: number, el: HTMLTableRowElement): void;
        expandRow(index: number, recursive?: boolean): void;
        collapseRow(index: number): void;
        getRowCount(): number;
        getRealRowCount(): number;
        getRowDataAt(index: number): Object;
        getAllRowsData(): Object[];
        changed(): UIDLEvent<DataGridRowsChangedEventArgs>;
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class DataGridRowsChangedEventArgs {
        private _changeType: DataGridRowsChangeType;
        private _startIndex: number;
        private _count: number;
        private _virtualPage: DataGridVirtualPage;

        //--------------------------------------------------------------------------------------------------------------

        public constructor(
            changeType: DataGridRowsChangeType,
            startIndex: number,
            count: number,
            virtualPage: DataGridVirtualPage = null)
        {
            this._changeType = changeType;
            this._startIndex = startIndex;
            this._count = count;
            this._virtualPage = virtualPage;
        }

        //--------------------------------------------------------------------------------------------------------------

        public changeType(): DataGridRowsChangeType {
            return this._changeType;
        }

        //--------------------------------------------------------------------------------------------------------------

        public startIndex(): number {
            return this._startIndex;
        }

        //--------------------------------------------------------------------------------------------------------------

        public count(): number {
            return this._count;
        }

        //--------------------------------------------------------------------------------------------------------------

        public virtualPage(): DataGridVirtualPage {
            return this._virtualPage;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export enum DataGridRowsChangeType {
        inserted,
        updated,
        deleted
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class DataGridVirtualPage {
        public constructor(private _startIndex: number, private _endIndex: number, private _realRowCount: number) {
        }
        public startIndex(): number {
            return this._startIndex;
        }
        public endIndex(): number {
            return this._endIndex;
        }
        public realRowCount(): number {
            return this._realRowCount;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export abstract class DataGridBindingBase implements IDataGridBinding {
        private _changed: UIDLEvent<DataGridRowsChangedEventArgs> = new UIDLEvent<DataGridRowsChangedEventArgs>();

        //-------------------------------------------------------------------------------------------------------------

        public abstract renderRow(index: number, el: HTMLTableRowElement): void;
        public abstract getRowCount(): number;
        public abstract getRealRowCount(): number;
        public abstract getRowDataAt(index: number): Object;
        public abstract getAllRowsData(): Object[];

        //-------------------------------------------------------------------------------------------------------------

        public expandRow(index: number, recursive = false): void {
        }

        //-------------------------------------------------------------------------------------------------------------

        public collapseRow(index: number): void {
        }

        //-------------------------------------------------------------------------------------------------------------

        public attachView(view: UIDLDataGrid): void {
        }

        //-------------------------------------------------------------------------------------------------------------

        public changed(): UIDLEvent<DataGridRowsChangedEventArgs> {
            return this._changed;
        }
    }
}