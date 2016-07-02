namespace UIDL.Widgets.DataGrid
{
    export class VirtualDataGridBinding extends DataGridBindingBase  {
        private _upstream: IDataGridBinding;
        private _pageSize: number;
        private _realRowCount: number;
        private _pageStart: number;

        //-------------------------------------------------------------------------------------------------------------

        public constructor(upstream: IDataGridBinding, pageSize: number) {
            super();
            this._upstream = upstream;
            this._pageSize = pageSize;
            this._pageStart = 0;
            this._realRowCount = upstream.getRealRowCount();

            upstream.changed().bind((args) => this.onUpstreamBindingChanged(args));
        }

        //-------------------------------------------------------------------------------------------------------------

        public attachView(view: UIDLDataGrid): void {
            super.attachView(view);
            view.verticalScroll().bind((args) => this.onViewVerticalScroll(args));
        }

        //-------------------------------------------------------------------------------------------------------------

        public renderRow(index: number, el: HTMLTableRowElement): void {
            // nothing
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowCount(): number {
            return Math.min(this._pageSize, this._realRowCount);
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRealRowCount(): number {
            return this._realRowCount;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowDataAt(index: number): Object {
            return this._upstream.getRowDataAt(this._pageStart + index);
        }

        //-------------------------------------------------------------------------------------------------------------

        public getAllRowsData(): Object[] {
            const data: Object[] = [];
            const count = this.getRowCount();
            const upstream = this._upstream;
            const upstreamStart = this._pageStart; 

            for (let i = 0; i < count; i++) {
                data.push(upstream.getRowDataAt(upstreamStart + i));
            }

            return data;
        }

        //-------------------------------------------------------------------------------------------------------------

        private onUpstreamBindingChanged(args: DataGridRowsChangedEventArgs): void {
            this._realRowCount = this._upstream.getRealRowCount();

            const downstreamArgs = new DataGridRowsChangedEventArgs(
                args.changeType(),
                args.startIndex() - this._pageStart,
                args.count());

            this.changed().raise(downstreamArgs);
        }

        //-------------------------------------------------------------------------------------------------------------

        private onViewVerticalScroll(args: ScrollEventArgs): void {
            this._pageStart += (args.newPosition() - args.oldPosition());
        }
    }
}