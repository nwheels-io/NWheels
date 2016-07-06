namespace UIDL.Widgets.DataGrid
{
    export interface IDataGridUpstreamLayer {
        attachDownstreamLayer(layer: IDataGridDownstreamLayer);
        renderRow(index: number, el: HTMLTableRowElement): void;
        expandRow(index: number, recursive?: boolean): void;
        collapseRow(index: number): void;
        getRowCount(): number;
        getRealRowCount(): number;
        getRowDataAt(index: number): Object;
        getAllRowsData(): Object[];
        changed(): UIDLEvent<RowsChangedEventArgs>;
    }

    //-----------------------------------------------------------------------------------------------------------------

    export interface IDataGridDownstreamLayer {
        attachUpstreamLayer(layer: IDataGridUpstreamLayer);
        scrolled(): UIDLEvent<ScrollEventArgs>;
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class RowsChangedEventArgs {
        private _changeType: RowsChangeType;
        private _startIndex: number;
        private _count: number;
        private _virtualPage: RowsVirtualPage;

        //--------------------------------------------------------------------------------------------------------------

        public constructor(
            changeType: RowsChangeType,
            startIndex: number,
            count: number,
            virtualPage: RowsVirtualPage = null)
        {
            this._changeType = changeType;
            this._startIndex = startIndex;
            this._count = count;
            this._virtualPage = virtualPage;
        }

        //--------------------------------------------------------------------------------------------------------------

        public changeType(): RowsChangeType {
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

        public virtualPage(): RowsVirtualPage {
            return this._virtualPage;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export enum RowsChangeType {
        inserted,
        updated,
        deleted
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class RowsVirtualPage {
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

    export abstract class DataGridLayerBase implements IDataGridLayer {
        private _changed: UIDLEvent<RowsChangedEventArgs> = new UIDLEvent<RowsChangedEventArgs>();
        private _scrolled: UIDLEvent<ScrollEventArgs> = new UIDLEvent<ScrollEventArgs>();

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

        public attachDownstreamLayer(layer: IDataGridLayer): void {
        }

        //-------------------------------------------------------------------------------------------------------------

        public changed(): UIDLEvent<RowsChangedEventArgs> {
            return this._changed;
        }

        //-------------------------------------------------------------------------------------------------------------

        public scrolled(): UIDLEvent<ScrollEventArgs> {
            return this._scrolled;
        }
    }
}