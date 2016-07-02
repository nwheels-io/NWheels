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

        private onUpstreamBindingChanged(args: DataGridRowsChangedEventArgs) {
            //this._realRowCount = (args.virtualPage ? args.virtualPage().);

            //const downstreamArgs = new DataGridRowsChangedEventArgs(
            //    args.changeType(),
            //    args.startIndex,
            //    args.count());

            //this.changed().raise(downstreamArgs);
        }
    }
}