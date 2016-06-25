var UIDL;
(function (UIDL) {
    var Event = (function () {
        function Event() {
            this._handlers = [];
        }
        //-------------------------------------------------------------------------------------------------------------
        Event.prototype.bind = function (handler) {
            this._handlers.push(handler);
        };
        //-------------------------------------------------------------------------------------------------------------
        Event.prototype.unbind = function (handler) {
            this._handlers = this._handlers.filter(function (h) { return h !== handler; });
        };
        //-------------------------------------------------------------------------------------------------------------
        Event.prototype.raise = function (args) {
            this._handlers.slice(0).forEach(function (h) { return h(args); });
        };
        return Event;
    }());
    UIDL.Event = Event;
})(UIDL || (UIDL = {}));
var UIDL;
(function (UIDL) {
    var Widgets;
    (function (Widgets) {
        var UIDLDataGrid = (function () {
            function UIDLDataGrid() {
            }
            return UIDLDataGrid;
        }());
        Widgets.UIDLDataGrid = UIDLDataGrid;
        //-----------------------------------------------------------------------------------------------------------------
        var UIDLDataGridColumn = (function () {
            function UIDLDataGridColumn() {
            }
            return UIDLDataGridColumn;
        }());
        Widgets.UIDLDataGridColumn = UIDLDataGridColumn;
        //-----------------------------------------------------------------------------------------------------------------
        var DataGridRowsChangedEventArgs = (function () {
            function DataGridRowsChangedEventArgs() {
            }
            return DataGridRowsChangedEventArgs;
        }());
        Widgets.DataGridRowsChangedEventArgs = DataGridRowsChangedEventArgs;
        //-----------------------------------------------------------------------------------------------------------------
        var LocalDataGridBinding = (function () {
            //-------------------------------------------------------------------------------------------------------------
            function LocalDataGridBinding(rows) {
                this._rows = rows.slice(0);
            }
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.attachView = function (view) { };
            LocalDataGridBinding.prototype.renderRow = function (index, el) { };
            LocalDataGridBinding.prototype.expandRow = function (index, recursive) { };
            LocalDataGridBinding.prototype.collapseRow = function (index) { };
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.getRowCount = function () {
                return this._rows.length;
            };
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.getRowDataAt = function (index) {
                return this._rows[index];
            };
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.onChange = function (handler) { };
            return LocalDataGridBinding;
        }());
        Widgets.LocalDataGridBinding = LocalDataGridBinding;
        //-----------------------------------------------------------------------------------------------------------------
        var NestedSetTreeDataGridBinding = (function () {
            //-------------------------------------------------------------------------------------------------------------
            function NestedSetTreeDataGridBinding(upper, nestedSetProperty) {
                this._upper = upper;
                this._nestedSetProperty = nestedSetProperty;
            }
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.attachView = function (view) { };
            NestedSetTreeDataGridBinding.prototype.renderRow = function (index, el) { };
            NestedSetTreeDataGridBinding.prototype.expandRow = function (index, recursive) { };
            NestedSetTreeDataGridBinding.prototype.collapseRow = function (index) { };
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.getRowCount = function () {
                return this._upper.getRowCount();
            };
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.getRowDataAt = function (index) {
                return this._upper.getRowDataAt(index);
            };
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.onChange = function (handler) { };
            return NestedSetTreeDataGridBinding;
        }());
        Widgets.NestedSetTreeDataGridBinding = NestedSetTreeDataGridBinding;
    })(Widgets = UIDL.Widgets || (UIDL.Widgets = {}));
})(UIDL || (UIDL = {}));
//# sourceMappingURL=UIDLDataGrid.js.map