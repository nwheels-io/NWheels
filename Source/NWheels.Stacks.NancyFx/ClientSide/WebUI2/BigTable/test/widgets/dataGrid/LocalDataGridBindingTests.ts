///<reference path="../../../lib/typings/jasmine/jasmine.d.ts" />

namespace UIDL.Widgets.DataGrid.Tests
{
    describe("LocalDataGridBinding", () => {

        it("CanGetRowCount", () => {
            //- arrange

            let dataRows = ["AAA", "BBB", "CCC"];

            //- act

            let binding = new LocalDataGridLayer(dataRows);

            //- assert

            expect(binding.getRowCount()).toBe(3);
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanGetRowAtIndex", () => {

            //- arrange

            let dataRows = ["AAA", "BBB", "CCC"];
            let binding = new LocalDataGridLayer(dataRows);

            //- act

            var rowReturned = binding.getRowDataAt(1);

            //- assert

            expect(rowReturned).toEqual("BBB");
        });

        //-------------------------------------------------------------------------------------------------------------

        it("CanSubscribeChangeHandler", () => {
            //- arrange

            let dataRows = ["AAA", "BBB", "CCC"];
            let binding = new LocalDataGridLayer(dataRows);
            let handler = (args: RowsChangedEventArgs) => {
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
            let binding = new LocalDataGridLayer(dataRows);
            let handler = (args: RowsChangedEventArgs) => {
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

            let binding = new LocalDataGridLayer(dataRows);

            //- assert

            expect(dataRows).toEqual(["AAA", "BBB", "CCC"]);
        });
    });
}