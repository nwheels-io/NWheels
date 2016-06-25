var Greeter = (function () {
    function Greeter(element) {
        this.element = element;
        this.element.innerHTML += "The time is: ";
        this.span = document.createElement('span');
        this.element.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
    }
    Greeter.prototype.start = function () {
        var _this = this;
        this.timerToken = setInterval(function () { return _this.span.innerHTML = new Date().toUTCString(); }, 500);
    };
    Greeter.prototype.stop = function () {
        clearTimeout(this.timerToken);
    };
    return Greeter;
}());
window.onload = function () {
    var el = document.getElementById('content');
    var greeter = new Greeter(el);
    greeter.start();
};
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
///<reference path="UIDLDataGrid.ts" />
///<reference path="lib/typings/jasmine/jasmine.d.ts" />
describe("LocalDataGridBinding", function () {
    it("CanGetRowCount", function () {
        //- arrange
        var dataRows = ["AAA", "BBB", "CCC"];
        //- act
        var binding = new UIDL.Widgets.LocalDataGridBinding(dataRows);
        //- assert
        expect(binding.getRowCount()).toBe(3);
    });
    //-------------------------------------------------------------------------------------------------------------
    it("CanGetRowAtIndex", function () {
        //- arrange
        var dataRows = ["AAA", "BBB", "CCC"];
        var binding = new UIDL.Widgets.LocalDataGridBinding(dataRows);
        //- act
        var rowReturned = binding.getRowDataAt(1);
        //- assert
        expect(rowReturned).toEqual("BBB");
    });
    //-------------------------------------------------------------------------------------------------------------
    it("CanSubscribeChangeHandler", function () {
        //- arrange
        var dataRows = ["AAA", "BBB", "CCC"];
        var binding = new UIDL.Widgets.LocalDataGridBinding(dataRows);
        var handler = function (sender, args) {
            fail("onChange handler should never be invoked by LocalDataTableBinding!");
        };
        //- act & assert
        binding.onChange(handler);
        dataRows.push("DDD"); // nothing should happen here
    });
    //-------------------------------------------------------------------------------------------------------------
    it("IsNotAffectedByChangesInOriginalArray", function () {
        //- arrange
        var dataRows = ["AAA", "BBB", "CCC"];
        var binding = new UIDL.Widgets.LocalDataGridBinding(dataRows);
        var handler = function (sender, args) {
            fail("onChange handler should never be invoked by LocalDataTableBinding!");
        };
        binding.onChange(handler);
        //- act
        dataRows[1] = "ZZZ";
        dataRows.push("DDD");
        //- assert
        expect(binding.getRowCount()).toBe(3);
        expect(binding.getRowDataAt(1)).toBe("BBB");
    });
    //-------------------------------------------------------------------------------------------------------------
    it("DoesNotChangeOriginalArray", function () {
        //- arrange
        var dataRows = ["AAA", "BBB", "CCC"];
        //- act
        var binding = new UIDL.Widgets.LocalDataGridBinding(dataRows);
        //- assert
        expect(dataRows).toEqual(["AAA", "BBB", "CCC"]);
    });
});
//-----------------------------------------------------------------------------------------------------------------
describe("NestedSetTreeDataGridBinding", function () {
    var TestTreeNode = (function () {
        function TestTreeNode(value, subNodes) {
            this.value = value;
            this.subNodes = subNodes;
        }
        return TestTreeNode;
    }());
    //-------------------------------------------------------------------------------------------------------------
    function createTestTreeData() {
        return [
            new TestTreeNode('A1', [
                new TestTreeNode('A1B1'),
                new TestTreeNode('A1B2')]),
            new TestTreeNode('A2', [
                new TestTreeNode('A2B1', [
                    new TestTreeNode('A2B1C1'),
                    new TestTreeNode('A2B1C2')]),
                new TestTreeNode('A2B2', [
                    new TestTreeNode('A2B2C1', [
                        new TestTreeNode('A2B2C1D1')])])]),
            new TestTreeNode('A3', [
                new TestTreeNode('A3B1', [
                    new TestTreeNode('A3B1C1'),
                    new TestTreeNode('A3B1C2')]),
                new TestTreeNode('A3B2', [
                    new TestTreeNode('A3B2C1')])]),
        ];
    }
    //-------------------------------------------------------------------------------------------------------------
    it("IsInitiallyCollapsedToRoots", function () {
        //- arrange
        var nodes = createTestTreeData();
        var localBinding = new UIDL.Widgets.LocalDataGridBinding(nodes);
        //- act
        var binding = new UIDL.Widgets.NestedSetTreeDataGridBinding(localBinding, 'subNodes');
        //- assert
        expect(binding.getRowCount()).toBe(3);
        expect(binding.getRowDataAt(0).value).toBe('A1');
        expect(binding.getRowDataAt(1).value).toBe('A2');
        expect(binding.getRowDataAt(2).value).toBe('A3');
    });
});
//# sourceMappingURL=bigtable-out.js.map