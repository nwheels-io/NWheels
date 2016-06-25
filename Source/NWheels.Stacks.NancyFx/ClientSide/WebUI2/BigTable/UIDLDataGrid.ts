namespace UIDL
{
    export class Event<T> {
        private _handlers: ((args: T) => void)[] = [ ];

        //-------------------------------------------------------------------------------------------------------------

        bind(handler: (args: T) => void): void {
            this._handlers.push(handler);
        }

        //-------------------------------------------------------------------------------------------------------------

        unbind(handler: (args: T) => void): void {
            this._handlers = this._handlers.filter(h => h !== handler);
        }

        //-------------------------------------------------------------------------------------------------------------

        raise(args: T) {
            this._handlers.slice(0).forEach(h => h(args));
        }
    }
}

//---------------------------------------------------------------------------------------------------------------------

namespace UIDL.Widgets
{
    export class UIDLDataGrid {
        columns: UIDLDataGridColumn[];
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class UIDLDataGridColumn {
        title: string;
        size: number;
    }

    //-----------------------------------------------------------------------------------------------------------------

    export interface IDataGridBinding {
        attachView(view: UIDLDataGrid);
        renderRow(index: number, el: HTMLTableRowElement): void;
        expandRow(index: number, recursive?: boolean): void;
        collapseRow(index: number): void;
        getRowCount(): number;
        getRowDataAt(index: number): Object;
        changed(): Event<DataGridBindingChangedEventArgs>;
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class DataGridBindingChangedEventArgs {
    }

    //-----------------------------------------------------------------------------------------------------------------

    export abstract class DataGridBindingBase implements IDataGridBinding {
        private _changed: Event<DataGridBindingChangedEventArgs> = new Event<DataGridBindingChangedEventArgs>();

        //-------------------------------------------------------------------------------------------------------------

        public abstract renderRow(index: number, el: HTMLTableRowElement): void;
        public abstract getRowCount(): number;
        public abstract getRowDataAt(index: number): Object;

        //-------------------------------------------------------------------------------------------------------------

        public expandRow(index: number, recursive = false): void { }
        public collapseRow(index: number): void { }
        public attachView(view: UIDLDataGrid): void { }

        //-------------------------------------------------------------------------------------------------------------

        public changed(): Event<DataGridBindingChangedEventArgs> {
            return this._changed;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class LocalDataGridBinding extends DataGridBindingBase  {
        private _rows: Object[];

        //-------------------------------------------------------------------------------------------------------------

        public constructor(rows: Object[]) {
            super();
            this._rows = rows.slice(0);
        }

        //-------------------------------------------------------------------------------------------------------------

        public renderRow(index: number, el: HTMLTableRowElement): void {
            // nothing
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowCount(): number {
            return this._rows.length;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowDataAt(index: number): Object {
            return this._rows[index];
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class NestedSetTreeDataGridBinding extends DataGridBindingBase {
        private _upper: IDataGridBinding;
        private _nestedSetProperty: string;
        private _treeRoot: NodeSubTreeState;

        //-------------------------------------------------------------------------------------------------------------

        public constructor(upper: IDataGridBinding, nestedSetProperty: string) {
            super();
            this._upper = upper;
            this._nestedSetProperty = nestedSetProperty;
            this._treeRoot = new NodeSubTreeState(this, 0, null);
        }

        //-------------------------------------------------------------------------------------------------------------

        public renderRow(index: number, el: HTMLTableRowElement): void { }

        //-------------------------------------------------------------------------------------------------------------

        public getRowCount(): number {
            return this._upper.getRowCount();
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowDataAt(index: number): Object {
            return this._treeRoot.tryGetRowDataAt(index, -1);
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowDataSubNodes(rowData: Object): Object[] {
            if (rowData == null) {
                const topLevelRowData: Object[] = [];
                const topLevelRowCount = this._upper.getRowCount(); 
                for (let i = 0; i < topLevelRowCount; i++) {
                    topLevelRowData.push(this._upper.getRowDataAt(i));
                }
                return topLevelRowData;
            } else if (rowData.hasOwnProperty(this._nestedSetProperty)) {
                //let rowDataAsAny = (rowData as any);
                return rowData[this._nestedSetProperty];
            } else {
                return null;
            }
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    class NodeSubTreeState {
        private _ownerBinding: NestedSetTreeDataGridBinding;
        private _childIndex: number;
        private _nodeData: Object;
        private _isExpanded: boolean;
        private _visibleSubTreeSize: number;
        private _subTrees: NodeSubTreeState[];

        //-------------------------------------------------------------------------------------------------------------

        public constructor(ownerBinding: NestedSetTreeDataGridBinding, childIndex: number, nodeData: Object) {
            this._ownerBinding = ownerBinding;
            this._childIndex = childIndex;
            this._nodeData = nodeData;
            this._visibleSubTreeSize = 0;
        }

        //-------------------------------------------------------------------------------------------------------------

        public tryGetRowDataAt(rowIndex: number, thisVisibleAtIndex: number): Object {
            if (rowIndex === thisVisibleAtIndex) {
                return this._nodeData;
            }

            if (rowIndex < thisVisibleAtIndex || rowIndex > thisVisibleAtIndex + this._visibleSubTreeSize) {
                return null;
            }

            const subTreeSizeToSkip = rowIndex - thisVisibleAtIndex - 1;
            let subTreeSizeSkipped = 0;
            let lastSubtree: NodeSubTreeState = null;

            for (let i = 0; i < this._subTrees.length; i++) {
                let currentSubree = this._subTrees[i];
                let siblingsSkippedToCurrentChild = (
                    lastSubtree !== null
                    ? currentSubree._childIndex - lastSubtree._childIndex
                    : currentSubree._childIndex);

                if (subTreeSizeSkipped + siblingsSkippedToCurrentChild >= subTreeSizeToSkip - subTreeSizeSkipped) {
                    let rowDataSubNodes = this._ownerBinding.getRowDataSubNodes(this._nodeData);
                    let rowDataIndex = (lastSubtree ? lastSubtree._childIndex + subTreeSizeToSkip - subTreeSizeSkipped : subTreeSizeToSkip);
                    return rowDataSubNodes[rowDataIndex];
                }

                subTreeSizeSkipped += siblingsSkippedToCurrentChild;

                var subTreeResult = currentSubree.tryGetRowDataAt(rowIndex, thisVisibleAtIndex + 1 + subTreeSizeSkipped);
                if (subTreeResult !== null) {
                    return subTreeResult;
                }

                subTreeSizeSkipped += (currentSubree._visibleSubTreeSize + 1);
                lastSubtree = currentSubree;
            }

            let rowDataSubNodes = this._ownerBinding.getRowDataSubNodes(this._nodeData);
            let rowDataIndex = (lastSubtree ? lastSubtree._childIndex + subTreeSizeToSkip - subTreeSizeSkipped : subTreeSizeToSkip);
            return rowDataSubNodes[rowDataIndex];
        }
    }
}                                                                                                                                   