///<reference path="bigtable.ts" />
///<reference path="lib/typings/jasmine/jasmine.d.ts" />
describe("LocalDataTableBindingTests", function () {
    it("CanGetRowCount", function () {
        //- arrange
        var dataRows = ["AAA", "BBB", "CCC"];
        //- act
        var binding = new LocalDataTableBinding(dataRows);
        //- assert
        expect(binding.getRowCount()).toBe(3);
    });
    it("CanGetRowAtIndex", function () {
        //- arrange
        var dataRows = ["AAA", "BBB", "CCC"];
        var binding = new LocalDataTableBinding(dataRows);
        //- act
        var rowReturned = binding.getRowDataAt(1);
        //- assert
        expect(rowReturned).toEqual("BBB");
    });
    it("CanSubscribeChangeHandler", function () {
        //- arrange
        var dataRows = ["AAA", "BBB", "CCC"];
        var binding = new LocalDataTableBinding(dataRows);
        var handler = function (sender, args) {
            fail("onChange handler should never be invoked by LocalDataTableBinding!");
        };
        //- act & assert
        binding.onChange(handler);
        dataRows.push("DDD"); // nothing should happen here
    });
    it("IsNotAffectedByChangesInOriginalArray", function () {
        //- arrange
        var dataRows = ["AAA", "BBB", "CCC"];
        var binding = new LocalDataTableBinding(dataRows);
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
    it("DoesNotChangeOriginalArray", function () {
        //- arrange
        var dataRows = ["AAA", "BBB", "CCC"];
        //- act
        var binding = new LocalDataTableBinding(dataRows);
        //- assert
        expect(dataRows).toEqual(["AAA", "BBB", "CCC"]);
    });
});
//---------------------------------------------------------------------------------------------------------------------
//# sourceMappingURL=bigtable.tests.js.map