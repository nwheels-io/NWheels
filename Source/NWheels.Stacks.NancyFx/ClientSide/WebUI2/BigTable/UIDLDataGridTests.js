///<reference path="UIDLDataGrid.ts" />
///<reference path="lib/typings/jasmine/jasmine.d.ts" />
var UIDL;
(function (UIDL) {
    var Widgets;
    (function (Widgets) {
        var Tests;
        (function (Tests) {
            describe("Event", function () {
                it('CanInvokeSingleHandler', function () {
                    //- arrange
                    var log = [];
                    var event = new UIDL.Event();
                    event.bind(function (arg) {
                        log.push("test-handler(" + arg + ")");
                    });
                    //- act
                    event.raise('ABC');
                    //- assert
                    expect(log).toEqual(['test-handler(ABC)']);
                });
                it('CanInvokeMultipleHandlers', function () {
                    //- arrange
                    var log = [];
                    var event = new UIDL.Event();
                    event.bind(function (arg) {
                        log.push("test-handler-1(" + arg + ")");
                    });
                    event.bind(function (arg) {
                        log.push("test-handler-2(" + arg + ")");
                    });
                    //- act
                    event.raise('ABC');
                    //- assert
                    expect(log).toEqual([
                        'test-handler-1(ABC)',
                        'test-handler-2(ABC)',
                    ]);
                });
                it('CanUnbindHandler', function () {
                    //- arrange
                    var log = [];
                    var event = new UIDL.Event();
                    var handler1 = (function (arg) {
                        log.push("test-handler-1(" + arg + ")");
                    });
                    var handler2 = (function (arg) {
                        log.push("test-handler-2(" + arg + ")");
                    });
                    var handler3 = (function (arg) {
                        log.push("test-handler-3(" + arg + ")");
                    });
                    event.bind(handler1);
                    event.bind(handler2);
                    event.bind(handler3);
                    //- act
                    event.unbind(handler2);
                    event.raise('ABC');
                    //- assert
                    expect(log).toEqual([
                        'test-handler-1(ABC)',
                        'test-handler-3(ABC)',
                    ]);
                });
            });
            //-----------------------------------------------------------------------------------------------------------------
            describe("LocalDataGridBinding", function () {
                it("CanGetRowCount", function () {
                    //- arrange
                    var dataRows = ["AAA", "BBB", "CCC"];
                    //- act
                    var binding = new Widgets.LocalDataGridBinding(dataRows);
                    //- assert
                    expect(binding.getRowCount()).toBe(3);
                });
                //-------------------------------------------------------------------------------------------------------------
                it("CanGetRowAtIndex", function () {
                    //- arrange
                    var dataRows = ["AAA", "BBB", "CCC"];
                    var binding = new Widgets.LocalDataGridBinding(dataRows);
                    //- act
                    var rowReturned = binding.getRowDataAt(1);
                    //- assert
                    expect(rowReturned).toEqual("BBB");
                });
                //-------------------------------------------------------------------------------------------------------------
                it("CanSubscribeChangeHandler", function () {
                    //- arrange
                    var dataRows = ["AAA", "BBB", "CCC"];
                    var binding = new Widgets.LocalDataGridBinding(dataRows);
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
                    var binding = new Widgets.LocalDataGridBinding(dataRows);
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
                    var binding = new Widgets.LocalDataGridBinding(dataRows);
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
                    var localBinding = new Widgets.LocalDataGridBinding(nodes);
                    //- act
                    var binding = new Widgets.NestedSetTreeDataGridBinding(localBinding, 'subNodes');
                    //- assert
                    expect(binding.getRowCount()).toBe(3);
                    expect(binding.getRowDataAt(0).value).toBe('A1');
                    expect(binding.getRowDataAt(1).value).toBe('A2');
                    expect(binding.getRowDataAt(2).value).toBe('A3');
                });
            });
        })(Tests = Widgets.Tests || (Widgets.Tests = {}));
    })(Widgets = UIDL.Widgets || (UIDL.Widgets = {}));
})(UIDL || (UIDL = {}));
//# sourceMappingURL=UIDLDataGridTests.js.map