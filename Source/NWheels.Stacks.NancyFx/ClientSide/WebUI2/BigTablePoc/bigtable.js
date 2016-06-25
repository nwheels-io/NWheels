var StaticDataLayer = (function () {
    function StaticDataLayer(rows) {
        this._rows = rows;
    }
    StaticDataLayer.prototype.getRowCount = function () {
        return this._rows.length;
    };
    return StaticDataLayer;
}());
//# sourceMappingURL=bigtable.js.map