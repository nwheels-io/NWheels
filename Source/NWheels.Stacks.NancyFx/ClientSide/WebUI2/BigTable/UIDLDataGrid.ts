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

//---------------------------------------------------------------------------------------------------------------------

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
        expandRow(index: number, recursive?: boolean): void;
        collapseRow(index: number): void;
        getRowCount(): number;
        getRowDataAt(index: number): Object;
        changed(): Event<DataGridBindingChangedEventArgs>;
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class DataGridBindingChangedEventArgs {
    }

    //-----------------------------------------------------------------------------------------------------------------

    export abstract class DataGridBindingBase implements IDataGridBinding {
        private _changed: Event<DataGridBindingChangedEventArgs> = new Event<DataGridBindingChangedEventArgs>();

        //-------------------------------------------------------------------------------------------------------------

        public abstract renderRow(index: number, el: HTMLTableRowElement): void;
        public abstract getRowCount(): number;
        public abstract getRowDataAt(index: number): Object;

        //-------------------------------------------------------------------------------------------------------------

        public expandRow(index: number, recursive = false): void { }
        public collapseRow(index: number): void { }
        public attachView(view: UIDLDataGrid): void { }

        //-------------------------------------------------------------------------------------------------------------

        public changed(): Event<DataGridBindingChangedEventArgs> {
            return this._changed;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class LocalDataGridBinding extends DataGridBindingBase  {
        private _rows: Object[];

        //-------------------------------------------------------------------------------------------------------------

        constructor(rows: Object[]) {
            super();
            this._rows = rows.slice(0);
        }

        //-------------------------------------------------------------------------------------------------------------

        renderRow(index: number, el: HTMLTableRowElement): void {
            // nothing
        }

        //-------------------------------------------------------------------------------------------------------------

        getRowCount(): number {
            return this._rows.length;
        }

        //-------------------------------------------------------------------------------------------------------------

        getRowDataAt(index: number): Object {
            return this._rows[index];
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class NestedSetTreeDataGridBinding extends DataGridBindingBase {
        private _upper: IDataGridBinding;
        private _nestedSetProperty: string;

        //-------------------------------------------------------------------------------------------------------------

        constructor(upper: IDataGridBinding, nestedSetProperty: string) {
            super();
            this._upper = upper;
            this._nestedSetProperty = nestedSetProperty;
        }

        //-------------------------------------------------------------------------------------------------------------

        renderRow(index: number, el: HTMLTableRowElement): void { }

        //-------------------------------------------------------------------------------------------------------------

        getRowCount(): number {
            return this._upper.getRowCount();
        }

        //-------------------------------------------------------------------------------------------------------------

        getRowDataAt(index: number): Object {
            return this._upper.getRowDataAt(index);
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    class ExpandedTreeNodeState {
        
    }
}                                                                                                                                   