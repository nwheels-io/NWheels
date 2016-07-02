///<reference path="../../../lib/typings/jasmine/jasmine.d.ts" />

namespace UIDL.Widgets.DataGrid.Tests
{
    describe("NestedSetTreeDataGridBinding", () => {

        class TestTreeNode {
            public constructor(public value: string, public subNodes?: TestTreeNode[]) { }
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

        //-------------------------------------------------------------------------------------------------------------

        it("CanExpandPathInTheMiddle", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            //- act

            binding.expandRow(1);
            binding.expandRow(3);
            binding.expandRow(4);

            //- assert

            expect(binding.getRowCount()).toBe(7);

            let visibleNodeValues = selectVisibleNodeValues(binding);

            expect(visibleNodeValues).toEqual([
                'A1', 'A2', 'A2B1', 'A2B2', 'A2B2C1', 'A2B2C1D1', 'A3'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanExpandRecursiveMultipleTimes", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            //- act

            binding.expandRow(1, true);
            binding.expandRow(2, true);
            binding.expandRow(5, true);
            binding.expandRow(6, true);

            //- assert

            expect(binding.getRowCount()).toBe(9);

            let visibleNodeValues = selectVisibleNodeValues(binding);

            expect(visibleNodeValues).toEqual([
                'A1', 'A2', 'A2B1', 'A2B1C1', 'A2B1C2', 'A2B2', 'A2B2C1', 'A2B2C1D1', 'A3'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("IgnoresAttemptToExpandLeafNode", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            //- act

            binding.expandRow(1, true);
            binding.expandRow(3);
            binding.expandRow(4, true);

            //- assert

            expect(binding.getRowCount()).toBe(9);

            let visibleNodeValues = selectVisibleNodeValues(binding);

            expect(visibleNodeValues).toEqual([
                'A1', 'A2', 'A2B1', 'A2B1C1', 'A2B1C2', 'A2B2', 'A2B2C1', 'A2B2C1D1', 'A3'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanFullyExpandPartiallyExpandedSubtree", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            //- act

            binding.expandRow(1);
            binding.expandRow(1, true);

            //- assert

            expect(binding.getRowCount()).toBe(9);

            let visibleNodeValues = selectVisibleNodeValues(binding);

            expect(visibleNodeValues).toEqual([
                'A1', 'A2', 'A2B1', 'A2B1C1', 'A2B1C2', 'A2B2', 'A2B2C1', 'A2B2C1D1', 'A3'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanExpandNodeInTheTop", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            //- act

            binding.expandRow(0);

            //- assert

            expect(binding.getRowCount()).toBe(5);

            let visibleNodeValues = selectVisibleNodeValues(binding);

            expect(visibleNodeValues).toEqual([
                'A1', 'A1B1', 'A1B2', 'A2', 'A3'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanExpandNodeInTheBottom", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            //- act

            binding.expandRow(2);

            //- assert

            expect(binding.getRowCount()).toBe(5);

            let visibleNodeValues = selectVisibleNodeValues(binding);

            expect(visibleNodeValues).toEqual([
                'A1', 'A2', 'A3', 'A3B1', 'A3B2'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanExpandSubtreeInTheBottomRecursively", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            //- act

            binding.expandRow(2, true);

            //- assert

            expect(binding.getRowCount()).toBe(8);

            let visibleNodeValues = selectVisibleNodeValues(binding);

            expect(visibleNodeValues).toEqual([
                'A1', 'A2', 'A3', 'A3B1', 'A3B1C1', 'A3B1C2', 'A3B2', 'A3B2C1'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanCollapseNodeInTheMiddle", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            binding.expandRow(1, true);

            //- act

            binding.collapseRow(2);

            //- assert

            expect(binding.getRowCount()).toBe(7);

            let visibleNodeValues = selectVisibleNodeValues(binding);

            expect(visibleNodeValues).toEqual([
                'A1', 'A2', 'A2B1', 'A2B2', 'A2B2C1', 'A2B2C1D1', 'A3'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanCollapseNodeInTheTop", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            binding.expandRow(1);
            binding.expandRow(0);

            let visibleNodeValuesBefore = selectVisibleNodeValues(binding);

            //- act

            binding.collapseRow(0);

            //- assert

            expect(visibleNodeValuesBefore).toEqual([
                'A1', 'A1B1', 'A1B2', 'A2', 'A2B1', 'A2B2', 'A3'
            ]);

            expect(binding.getRowCount()).toBe(5);

            let visibleNodeValuesAfter = selectVisibleNodeValues(binding);

            expect(visibleNodeValuesAfter).toEqual([
                'A1', 'A2', 'A2B1', 'A2B2', 'A3'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanCollapseNodeInTheBottom", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            binding.expandRow(2, true);
            binding.expandRow(1);

            let visibleNodeValuesBefore = selectVisibleNodeValues(binding);

            //- act

            binding.collapseRow(4);

            //- assert

            expect(visibleNodeValuesBefore).toEqual([
                'A1', 'A2', 'A2B1', 'A2B2', 'A3', 'A3B1', 'A3B1C1', 'A3B1C2', 'A3B2', 'A3B2C1'
            ]);

            expect(binding.getRowCount()).toBe(5);

            let visibleNodeValuesAfter = selectVisibleNodeValues(binding);

            expect(visibleNodeValuesAfter).toEqual([
                'A1', 'A2', 'A2B1', 'A2B2', 'A3'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("IgnoresAttemptToCollapseLeafNode", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            binding.expandRow(2, true);

            let visibleNodeValuesBefore = selectVisibleNodeValues(binding);

            //- act

            binding.collapseRow(7);

            //- assert

            let visibleNodeValuesAfter = selectVisibleNodeValues(binding);

            expect(visibleNodeValuesBefore).toEqual([
                'A1', 'A2', 'A3', 'A3B1', 'A3B1C1', 'A3B1C2', 'A3B2', 'A3B2C1'
            ]);

            expect(visibleNodeValuesAfter).toEqual([
                'A1', 'A2', 'A3', 'A3B1', 'A3B1C1', 'A3B1C2', 'A3B2', 'A3B2C1'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("PreservesExpandedStateOfNodesInCollapsedSubtree", () => {
            //- arrange

            let nodes = createTestTreeData();
            let binding = new NestedSetTreeDataGridBinding(new LocalDataGridBinding(nodes), 'subNodes');

            binding.expandRow(1); // A2
            binding.expandRow(2); // A2B1
            binding.expandRow(5); // A2B2
            binding.collapseRow(1);

            let visibleNodeValuesBefore = selectVisibleNodeValues(binding);

            //- act

            binding.expandRow(1);

            //- assert

            let visibleNodeValuesAfter = selectVisibleNodeValues(binding);

            expect(visibleNodeValuesBefore).toEqual([
                'A1', 'A2', 'A3'
            ]);

            expect(visibleNodeValuesAfter).toEqual([
                'A1', 'A2', 'A2B1', 'A2B1C1', 'A2B1C2', 'A2B2', 'A2B2C1', 'A3'
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanInsertNewRootNodesFromUpstreamBinding", () => {
            //- arrange

            const nodes = createTestTreeData();
            const upstreamBinding = new TestDataGridBinding(nodes);
            const binding = new NestedSetTreeDataGridBinding(upstreamBinding, 'subNodes');

            let visibleNodeValuesBefore = selectVisibleNodeValues(binding);

            //- act

            upstreamBinding.insertRows(3, [
                new TestTreeNode('A4', []),
                new TestTreeNode('A5', [])
            ]);

            //- assert

            let visibleNodeValuesAfter = selectVisibleNodeValues(binding);

            expect(visibleNodeValuesBefore).toEqual([
                'A1', 'A2', 'A3'
            ]);

            expect(visibleNodeValuesAfter).toEqual([
                'A1', 'A2', 'A3', 'A4', 'A5'
            ]);
        });
    });
}