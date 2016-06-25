namespace UIDL
{
    export class Event<T> {
        private _handlers: ((args: T) => void)[] = [ ];

        //-------------------------------------------------------------------------------------------------------------

        bind(handler: (args: T) => void): void {
            this._handlers.push(handler);
        }

        //-------------------------------------------------------------------------------------------------------------

        unbind(handler: (args: T) => void): void {
            this._handlers = this._handlers.filter(h => h !== handler);
        }

        //-------------------------------------------------------------------------------------------------------------

        raise(args: T) {
            this._handlers.slice(0).forEach(h => h(args));
        }
    }
}

namespace UIDL.Widgets
{
    export class UIDLDataGrid {
        columns: UIDLDataGridColumn[];
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class UIDLDataGridColumn {
        title: string;
        size: number;
    }

    //-----------------------------------------------------------------------------------------------------------------

    export interface IDataGridBinding {
        attachView(view: UIDLDataGrid);
        renderRow(index: number, el: HTMLTableRowElement): void;
        expandRow(index: number, recursive: boolean): void;
        collapseRow(index: number): void;
        getRowCount(): number;
        getRowDataAt(index: number): Object;
        onChange(handler: (sender: IDataGridBinding , args: DataGridRowsChangedEventArgs) => void): void;
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class DataGridRowsChangedEventArgs {

    }

    //-----------------------------------------------------------------------------------------------------------------

    export class LocalDataGridBinding implements IDataGridBinding  {
        private _rows: Object[];

        //-------------------------------------------------------------------------------------------------------------

        constructor(rows: Object[]) {
            this._rows = rows.slice(0);
        }

        //-------------------------------------------------------------------------------------------------------------

        attachView(view: UIDLDataGrid) {}
        renderRow(index: number, el: HTMLTableRowElement): void {}
        expandRow(index: number, recursive: boolean): void {}
        collapseRow(index: number): void {}

        //-------------------------------------------------------------------------------------------------------------

        getRowCount(): number {
            return this._rows.length;
        }

        //-------------------------------------------------------------------------------------------------------------

        getRowDataAt(index: number): Object {
            return this._rows[index];
        }

        //-------------------------------------------------------------------------------------------------------------

        onChange(handler: (sender: IDataGridBinding, args: DataGridRowsChangedEventArgs) => void): void { }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class NestedSetTreeDataGridBinding implements IDataGridBinding {
        private _upper: IDataGridBinding;
        private _nestedSetProperty: string;

        //-------------------------------------------------------------------------------------------------------------

        constructor(upper: IDataGridBinding, nestedSetProperty: string) {
            this._upper = upper;
            this._nestedSetProperty = nestedSetProperty;
        }

        //-------------------------------------------------------------------------------------------------------------

        attachView(view: UIDLDataGrid) { }
        renderRow(index: number, el: HTMLTableRowElement): void { }
        expandRow(index: number, recursive: boolean): void { }
        collapseRow(index: number): void { }

        //-------------------------------------------------------------------------------------------------------------

        getRowCount(): number {
            return this._upper.getRowCount();
        }

        //-------------------------------------------------------------------------------------------------------------

        getRowDataAt(index: number): Object {
            return this._upper.getRowDataAt(index);
        }

        //-------------------------------------------------------------------------------------------------------------

        onChange(handler: (sender: IDataGridBinding, args: DataGridRowsChangedEventArgs) => void): void { }
    }

    //-----------------------------------------------------------------------------------------------------------------

}                                                                                                                                   