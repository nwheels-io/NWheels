interface IDataLayer {
    getRowCount(): number;
    
}

class StaticDataLayer implements IDataLayer {
    private _rows: any[];
    constructor(rows: any[]) {
        this._rows = rows;
    }
    getRowCount(): number {
        return this._rows.length;
    }

}