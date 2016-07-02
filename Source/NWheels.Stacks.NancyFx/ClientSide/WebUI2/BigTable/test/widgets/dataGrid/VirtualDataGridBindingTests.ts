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

        it("bring all upstream data when size is less than page", () => {
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

        it("initially maps first page", () => {
            //- arrange

            const upstream = new TestDataGridBinding<string>(createTestDataSet(1, 100));
            const pageSize = 10;

            //- act

            const binding = new VirtualDataGridBinding(upstream, pageSize);

            //- assert

            expect(binding.getRowCount()).toBe(10);
            expect(binding.getAllRowsData()).toEqual(createTestDataSet(1, 10));
        });

    });
}