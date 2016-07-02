namespace UIDL.Widgets
{
    export class UIDLDataGrid {
        private _binding: DataGrid.IDataGridBinding;
        private _verticalScroll: UIDLEvent<ScrollEventArgs>;

        //-------------------------------------------------------------------------------------------------------------

        public constructor() {
            this._verticalScroll = new UIDLEvent<ScrollEventArgs>();
        }

        //-------------------------------------------------------------------------------------------------------------

        public setBinding(binding: DataGrid.IDataGridBinding): void {
            this._binding = binding;
            binding.attachView(this);
        }

        //-------------------------------------------------------------------------------------------------------------

        public verticalScroll(): UIDLEvent<ScrollEventArgs> {
            return this._verticalScroll;
        }

        //-------------------------------------------------------------------------------------------------------------

        columns: UIDLDataGridColumn[];
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class UIDLDataGridColumn {
        title: string;
        size: number;
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class ScrollEventArgs {
        public constructor(private _oldPosition: number, private _newPosition: number) {
        }
        public oldPosition(): number {
            return this._oldPosition;
        }
        public newPosition(): number {
            return this._newPosition;
        }
    }
}