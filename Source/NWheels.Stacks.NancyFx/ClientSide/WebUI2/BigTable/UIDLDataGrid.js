var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
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
//---------------------------------------------------------------------------------------------------------------------
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
        var DataGridBindingChangedEventArgs = (function () {
            function DataGridBindingChangedEventArgs() {
            }
            return DataGridBindingChangedEventArgs;
        }());
        Widgets.DataGridBindingChangedEventArgs = DataGridBindingChangedEventArgs;
        //-----------------------------------------------------------------------------------------------------------------
        var DataGridBindingBase = (function () {
            function DataGridBindingBase() {
                this._changed = new UIDL.Event();
            }
            //-------------------------------------------------------------------------------------------------------------
            DataGridBindingBase.prototype.expandRow = function (index, recursive) {
                if (recursive === void 0) { recursive = false; }
            };
            DataGridBindingBase.prototype.collapseRow = function (index) { };
            DataGridBindingBase.prototype.attachView = function (view) { };
            //-------------------------------------------------------------------------------------------------------------
            DataGridBindingBase.prototype.changed = function () {
                return this._changed;
            };
            return DataGridBindingBase;
        }());
        Widgets.DataGridBindingBase = DataGridBindingBase;
        //-----------------------------------------------------------------------------------------------------------------
        var LocalDataGridBinding = (function (_super) {
            __extends(LocalDataGridBinding, _super);
            //-------------------------------------------------------------------------------------------------------------
            function LocalDataGridBinding(rows) {
                _super.call(this);
                this._rows = rows.slice(0);
            }
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.renderRow = function (index, el) {
                // nothing
            };
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.getRowCount = function () {
                return this._rows.length;
            };
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.getRowDataAt = function (index) {
                return this._rows[index];
            };
            return LocalDataGridBinding;
        }(DataGridBindingBase));
        Widgets.LocalDataGridBinding = LocalDataGridBinding;
        //-----------------------------------------------------------------------------------------------------------------
        var NestedSetTreeDataGridBinding = (function (_super) {
            __extends(NestedSetTreeDataGridBinding, _super);
            //-------------------------------------------------------------------------------------------------------------
            function NestedSetTreeDataGridBinding(upper, nestedSetProperty) {
                _super.call(this);
                this._upper = upper;
                this._nestedSetProperty = nestedSetProperty;
            }
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.renderRow = function (index, el) { };
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.getRowCount = function () {
                return this._upper.getRowCount();
            };
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.getRowDataAt = function (index) {
                return this._upper.getRowDataAt(index);
            };
            return NestedSetTreeDataGridBinding;
        }(DataGridBindingBase));
        Widgets.NestedSetTreeDataGridBinding = NestedSetTreeDataGridBinding;
        //-----------------------------------------------------------------------------------------------------------------
        var ExpandedTreeNodeState = (function () {
            function ExpandedTreeNodeState() {
            }
            return ExpandedTreeNodeState;
        }());
    })(Widgets = UIDL.Widgets || (UIDL.Widgets = {}));
})(UIDL || (UIDL = {}));
//# sourceMappingURL=UIDLDataGrid.js.map