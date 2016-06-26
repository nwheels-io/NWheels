///<reference path="UIDLDataGrid.ts" />
///<reference path="lib/typings/jasmine/jasmine.d.ts" />

namespace UIDL.Widgets.Tests
{
    describe("Event", () => {
        it('CanInvokeSingleHandler', () => {
            //- arrange

            let log: string[] = [];
            let event = new Event<string>();

            event.bind((arg: string) => {
                log.push(`test-handler(${arg})`);
            });

            //- act

            event.raise('ABC');

            //- assert

            expect(log).toEqual(['test-handler(ABC)']);
        });

        //-------------------------------------------------------------------------------------------------------------

        it('CanInvokeMultipleHandlers', () => {
            //- arrange

            let log: string[] = [];
            let event = new Event<string>();

            event.bind((arg: string) => {
                log.push(`test-handler-1(${arg})`);
            });

            event.bind((arg: string) => {
                log.push(`test-handler-2(${arg})`);
            });

            //- act

            event.raise('ABC');

            //- assert

            expect(log).toEqual([
                'test-handler-1(ABC)',
                'test-handler-2(ABC)',
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it('CanUnbindHandler', () => {
            //- arrange

            let log: string[] = [];
            let event = new Event<string>();

            var handler1 = ((arg: string) => {
                log.push(`test-handler-1(${arg})`);
            });
            var handler2 = ((arg: string) => {
                log.push(`test-handler-2(${arg})`);
            });
            var handler3 = ((arg: string) => {
                log.push(`test-handler-3(${arg})`);
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

    describe("LocalDataGridBinding", () => {

        it("CanGetRowCount", () => {
            //- arrange

            let dataRows = ["AAA", "BBB", "CCC"];

            //- act

            let binding = new LocalDataGridBinding(dataRows);

            //- assert

            expect(binding.getRowCount()).toBe(3);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanGetRowAtIndex", () => {
            //- arrange

            let dataRows = ["AAA", "BBB", "CCC"];
            let binding = new LocalDataGridBinding(dataRows);

            //- act

            var rowReturned = binding.getRowDataAt(1);

            //- assert

            expect(rowReturned).toEqual("BBB");
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanSubscribeChangeHandler", () => {
            //- arrange

            let dataRows = ["AAA", "BBB", "CCC"];
            let binding = new LocalDataGridBinding(dataRows);
            let handler = (args: DataGridBindingChangedEventArgs) => {
                fail("onChange handler should never be invoked by LocalDataTableBinding!");
            };

            //- act & assert

            binding.changed().bind(handler);
            dataRows.push("DDD"); // nothing should happen here
        });

        //-------------------------------------------------------------------------------------------------------------

        it("IsNotAffectedByChangesInOriginalArray", () => {
            //- arrange

            let dataRows = ["AAA", "BBB", "CCC"];
            let binding = new LocalDataGridBinding(dataRows);
            let handler = (args: DataGridBindingChangedEventArgs) => {
                fail("onChange handler should never be invoked by LocalDataTableBinding!");
            };
            binding.changed().bind(handler);

            //- act

            dataRows[1] = "ZZZ";
            dataRows.push("DDD");

            //- assert

            expect(binding.getRowCount()).toBe(3);
            expect(binding.getRowDataAt(1)).toBe("BBB");
        });

        //-------------------------------------------------------------------------------------------------------------

        it("DoesNotChangeOriginalArray", () => {
            //- arrange

            let dataRows = ["AAA", "BBB", "CCC"];

            //- act

            let binding = new LocalDataGridBinding(dataRows);

            //- assert

            expect(dataRows).toEqual(["AAA", "BBB", "CCC"]);
        });
    });

    //-----------------------------------------------------------------------------------------------------------------

    describe("NestedSetTreeDataGridBinding", () => {

        class TestTreeNode {
            constructor(public value: string, public subNodes?: TestTreeNode[]) { }
        }

        //-------------------------------------------------------------------------------------------------------------

        function createTestTreeData(): TestTreeNode[] {
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

        function selectVisibleNodeValues(binding: IDataGridBinding): string[] {
            let values: string[] = [];

            for (let i = 0; i < binding.getRowCount(); i++) {
                var rowData = binding.getRowDataAt(i);
                var value = (<TestTreeNode>rowData).value;
                values.push(value);
            }

            return values;
        }

        //-------------------------------------------------------------------------------------------------------------

        it("IsInitiallyCollapsedToRoots", () => {
            //- arrange

            let nodes = createTestTreeData();
            let localBinding = new LocalDataGridBinding(nodes);

            //- act

            let binding = new NestedSetTreeDataGridBinding(localBinding, 'subNodes');

            //- assert

            expect(binding.getRowCount()).toBe(3);
            expect((binding.getRowDataAt(0) as any).value).toBe('A1');
            expect((binding.getRowDataAt(1) as any).value).toBe('A2');
            expect((binding.getRowDataAt(2) as any).value).toBe('A3');
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanExpandNodeInTheMiddle", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            //- act

            binding.expandRow(1);

            //- assert

            expect(binding.getRowCount()).toBe(5);

            let visibleNodeValues = selectVisibleNodeValues(binding);

            expect(visibleNodeValues).toEqual(['A1', 'A2', 'A2B1', 'A2B2', 'A3']);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanExpandNodeInTheMiddleRecursive", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            //- act

            binding.expandRow(1, true);

            //- assert

            expect(binding.getRowCount()).toBe(9);

            let visibleNodeValues = selectVisibleNodeValues(binding);

            expect(visibleNodeValues).toEqual([
                'A1', 'A2', 'A2B1', 'A2B1C1', 'A2B1C2', 'A2B2', 'A2B2C1', 'A2B2C1D1', 'A3'
            ]);
        });
    });
}