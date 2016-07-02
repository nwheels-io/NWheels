///<reference path="../../../lib/typings/jasmine/jasmine.d.ts" />

namespace UIDL.Widgets.DataGrid.Tests
{
    describe("VirtualDataGridBinding", () => {

        function createTestDataSet(start: number, end: number): string[] {
            let dataSet: string[] = [];

            for (let i = start; i <= end; i++) {
                dataSet.push(i.toString());
            }

            return dataSet;
        }

        //-------------------------------------------------------------------------------------------------------------

        it("projects entire upstream data when less than page size", () => {
            //- arrange

            const upstream = new TestDataGridBinding<string>(createTestDataSet(1, 5));
            const pageSize = 10;

            //- act

            const binding = new VirtualDataGridBinding(upstream, pageSize);

            //- assert

            expect(binding.getRowCount()).toBe(5);
            expect(binding.getAllRowsData()).toEqual(['1', '2', '3', '4', '5']);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("initially projects first page", () => {
            //- arrange

            const upstream = new TestDataGridBinding<string>(createTestDataSet(1, 100));
            const pageSize = 10;

            //- act

            const binding = new VirtualDataGridBinding(upstream, pageSize);

            //- assert

            expect(binding.getRowCount()).toBe(10);
            expect(binding.getAllRowsData()).toEqual(createTestDataSet(1, 10));
        });

        //-------------------------------------------------------------------------------------------------------------

        it("moves page forward when view scrolls down", () => {
            //- arrange

            const upstream = new TestDataGridBinding<string>(createTestDataSet(1, 100));
            const pageSize = 10;
            const binding = new VirtualDataGridBinding(upstream, pageSize);
            const view = new UIDLDataGrid();

            binding.attachView(view);

            //- act

            view.verticalScroll().raise(new ScrollEventArgs(0, 5));

            //- assert

            expect(binding.getRowCount()).toBe(10);
            expect(binding.getAllRowsData()).toEqual(createTestDataSet(6, 15));
        });

        //-------------------------------------------------------------------------------------------------------------

        it("moves page backward when view scrolls up", () => {
            //- arrange

            const upstream = new TestDataGridBinding<string>(createTestDataSet(1, 100));
            const pageSize = 10;
            const binding = new VirtualDataGridBinding(upstream, pageSize);

            const view = new UIDLDataGrid();
            binding.attachView(view);

            view.verticalScroll().raise(new ScrollEventArgs(0, 10));

            //- act

            view.verticalScroll().raise(new ScrollEventArgs(0, -3));

            //- assert

            expect(binding.getRowCount()).toBe(10);
            expect(binding.getAllRowsData()).toEqual(createTestDataSet(8, 17));
        });

        //-------------------------------------------------------------------------------------------------------------

        it("translates upstream change to virtual index", () => {
            //- arrange

            const upstream = new TestDataGridBinding<string>(createTestDataSet(1, 100));
            const pageSize = 10;
            const binding = new VirtualDataGridBinding(upstream, pageSize);

            const view = new TestDataGridView();
            view.setBinding(binding);
            view.verticalScroll().raise(new ScrollEventArgs(0, 10));

            //- act

            upstream.insertRows(12, ['X1', 'X2', 'X3', 'X4', 'X5']);

            //- assert

            expect(view.changeLog.length).toEqual(1);
            expect(view.changeLog[0].changeType()).toEqual(DataGridRowsChangeType.inserted);
            expect(view.changeLog[0].startIndex()).toEqual(2);
            expect(view.changeLog[0].count()).toEqual(5);

            expect(binding.getRowCount()).toBe(10);
            expect(binding.getAllRowsData()).toEqual(['11', '12', 'X1', 'X2', 'X3', 'X4', 'X5', '13', '14', '15']);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("updates real row count after upstream rows insertion", () => {
            //- arrange

            const upstream = new TestDataGridBinding<string>(createTestDataSet(1, 100));
            const pageSize = 10;
            const binding = new VirtualDataGridBinding(upstream, pageSize);

            const view = new TestDataGridView();
            view.setBinding(binding);
            view.verticalScroll().raise(new ScrollEventArgs(0, 10));

            //- act

            upstream.insertRows(12, ['X1', 'X2', 'X3']);

            //- assert

            expect(binding.getRowCount()).toBe(10);
            expect(binding.getRealRowCount()).toBe(103);
            expect(binding.getAllRowsData()).toEqual(['11', '12', 'X1', 'X2', 'X3', '13', '14', '15', '16', '17']);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("updates real row count after upstream rows deletion", () => {
            //- arrange

            const upstream = new TestDataGridBinding<string>(createTestDataSet(1, 100));
            const pageSize = 10;
            const binding = new VirtualDataGridBinding(upstream, pageSize);

            const view = new TestDataGridView();
            view.setBinding(binding);
            view.verticalScroll().raise(new ScrollEventArgs(0, 10));

            //- act

            upstream.deleteRows(12, 3);

            //- assert

            expect(binding.getRowCount()).toBe(10);
            expect(binding.getRealRowCount()).toBe(97);
            expect(binding.getAllRowsData()).toEqual(['11', '12', '16', '17', '18', '19', '20', '21', '22', '23']);
        });
    });
}