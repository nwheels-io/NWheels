///<reference path="lib/typings/jasmine/jasmine.d.ts" />
var UIDLDataGrid = (function () {
    function UIDLDataGrid() {
    }
    return UIDLDataGrid;
}());
//---------------------------------------------------------------------------------------------------------------------
var UIDLDataGridColumn = (function () {
    function UIDLDataGridColumn() {
    }
    return UIDLDataGridColumn;
}());
//---------------------------------------------------------------------------------------------------------------------
var RowsChangedEventArgs = (function () {
    function RowsChangedEventArgs() {
    }
    return RowsChangedEventArgs;
}());
//---------------------------------------------------------------------------------------------------------------------
var LocalDataTableBinding = (function () {
    function LocalDataTableBinding(rows) {
        this._rows = rows.slice(0);
    }
    LocalDataTableBinding.prototype.attachView = function (view) { };
    LocalDataTableBinding.prototype.renderRow = function (index, el) { };
    LocalDataTableBinding.prototype.expandRow = function (index, recursive) { };
    LocalDataTableBinding.prototype.collapseRow = function (index) { };
    LocalDataTableBinding.prototype.getRowCount = function () {
        return this._rows.length;
    };
    LocalDataTableBinding.prototype.getRowDataAt = function (index) {
        return this._rows[index];
    };
    LocalDataTableBinding.prototype.onChange = function (handler) { };
    return LocalDataTableBinding;
}());
//---------------------------------------------------------------------------------------------------------------------
/*
class TreeTableBinding implements ITableBinding {
    private _upper: ITableBinding;
    private _rowCount: number;

    //-----------------------------------------------------------------------------------------------------------------

    constructor(upper: ITableBinding, ) {
        this._upper = upper;
    }

    //-----------------------------------------------------------------------------------------------------------------

    getRowCount(): number {
        return this._rows.length;
    }

    //-----------------------------------------------------------------------------------------------------------------

    onChange(handler: (sender: ITableBinding, args: RowsChangedEventArgs) => void): void {
        // nothing
    }
}
*/
//---------------------------------------------------------------------------------------------------------------------
//# sourceMappingURL=bigtable.js.map