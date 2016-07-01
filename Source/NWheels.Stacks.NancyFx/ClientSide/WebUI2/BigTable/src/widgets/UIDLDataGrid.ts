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
        getAllRowsData(): Object[];
        changed(): UIDLEvent<DataGridRowsChangedEventArgs>;
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class DataGridRowsChangedEventArgs {
        private _changeType: DataGridRowsChangeType;
        private _startIndex: number;
        private _count: number;

        //--------------------------------------------------------------------------------------------------------------

        public constructor(changeType: DataGridRowsChangeType, startIndex: number, count: number) {
            this._changeType = changeType;
            this._startIndex = startIndex;
            this._count = count;
        }

        //--------------------------------------------------------------------------------------------------------------

        public changeType(): DataGridRowsChangeType {
            return this._changeType;
        }

        //--------------------------------------------------------------------------------------------------------------

        public startIndex(): number {
            return this._startIndex;
        }

        //--------------------------------------------------------------------------------------------------------------

        public count(): number {
            return this._count;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export enum DataGridRowsChangeType {
        inserted,
        updated,
        deleted
    }

    //-----------------------------------------------------------------------------------------------------------------

    export abstract class DataGridBindingBase implements IDataGridBinding {
        private _changed: UIDLEvent<DataGridRowsChangedEventArgs> = new UIDLEvent<DataGridRowsChangedEventArgs>();

        //-------------------------------------------------------------------------------------------------------------

        public abstract renderRow(index: number, el: HTMLTableRowElement): void;
        public abstract getRowCount(): number;
        public abstract getRowDataAt(index: number): Object;
        public abstract getAllRowsData(): Object[];

        //-------------------------------------------------------------------------------------------------------------

        public expandRow(index: number, recursive = false): void { }
        public collapseRow(index: number): void { }
        public attachView(view: UIDLDataGrid): void { }

        //-------------------------------------------------------------------------------------------------------------

        public changed(): UIDLEvent<DataGridRowsChangedEventArgs> {
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

        //-------------------------------------------------------------------------------------------------------------

        public getAllRowsData(): Object[] {
            return this._rows;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class NestedSetTreeDataGridBinding extends DataGridBindingBase {
        private _upstream: IDataGridBinding;
        private _nestedSetProperty: string;
        private _treeRoot: SubTreeState;
        private _lastRootNodeCount: number;
        private _lastGetRowDataIndex: number;
        private _lastGetRowDataNode: LocatedTreeNode;

        //-------------------------------------------------------------------------------------------------------------

        public constructor(upstream: IDataGridBinding, nestedSetProperty: string) {
            super();
            this._upstream = upstream;
            this._nestedSetProperty = nestedSetProperty;
            this._treeRoot = new SubTreeState(this, null, -1, null);
            this._treeRoot.expand(false);
            this._lastRootNodeCount = upstream.getRowCount();
            this._lastGetRowDataIndex = -1;
            this._lastGetRowDataNode = null;

            upstream.changed().bind((args) => this.onUpstreamBindingChanged(args));
        }

        //-------------------------------------------------------------------------------------------------------------

        public renderRow(index: number, el: HTMLTableRowElement): void { }

        //-------------------------------------------------------------------------------------------------------------

        public getRowCount(): number {
            return this._treeRoot.getVisibleSubTreeSize();
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowDataAt(index: number): Object {
            let locatedNode: LocatedTreeNode;

            if (index === this._lastGetRowDataIndex + 1 && this._lastGetRowDataNode) {
                locatedNode = this._lastGetRowDataNode.walkOneStep();
            } else {
                locatedNode = this._treeRoot.tryLocateTreeNode(index, -1);
            }

            this._lastGetRowDataIndex = index;
            this._lastGetRowDataNode = locatedNode;

            if (locatedNode) {
                return locatedNode.getNodeData();
            } else {
                return null;
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        public getRowDataSubNodes(rowData: Object): Object[] {
            if (rowData == null) {
                return this._upstream.getAllRowsData();
            } else {
                //let rowDataAsAny = (rowData as any);
                return rowData[this._nestedSetProperty];
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        public getAllRowsData(): Object[] {
            var data: Object[] = [];
            this._treeRoot.listAllNodesData(data);
            return data;
        }

        //-------------------------------------------------------------------------------------------------------------

        public expandRow(index: number, recursive = false): void {
            let locatedNode = this._treeRoot.tryLocateTreeNode(index, -1);
            if (locatedNode) {
                locatedNode.expand(recursive);
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        public collapseRow(index: number): void {
            let locatedNode = this._treeRoot.tryLocateTreeNode(index, -1);
            if (locatedNode) {
                locatedNode.collapse();
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        private onUpstreamBindingChanged(args: DataGridRowsChangedEventArgs) {
            if (args.changeType() !== DataGridRowsChangeType.inserted ||
                args.startIndex() !== this._lastRootNodeCount)
            {
                throw UIDLError.factory().generalNotSupported(
                    'NestedSetTreeDataGridBinding',
                    'UpstreamDeleteUpdateInsertAtNonTail');
            }

            this._lastRootNodeCount = this._upstream.getRowCount();

            const downstreamArgs = new DataGridRowsChangedEventArgs(
                args.changeType(),
                this._treeRoot.getVisibleSubTreeSize(),
                args.count());

            this.changed().raise(downstreamArgs);
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    class SubTreeState {
        private _ownerBinding: NestedSetTreeDataGridBinding;
        private _parentSubTree: SubTreeState;
        private _childIndex: number;
        private _nodeData: Object;
        private _isExpanded: boolean;
        private _visibleSubTreeSize: number;
        private _subTrees: { [index: number]: SubTreeState };
        private _subTreeCount: number;
        private _firstSubTree: SubTreeState;
        private _lastSubTree: SubTreeState;
        private _nextSibling: SubTreeState;
        private _prevSibling: SubTreeState;
        private _updateCount: number;
        private _visibleSubTreeSizeBeforeUpdate: number;

        //-------------------------------------------------------------------------------------------------------------

        public constructor(
            ownerBinding: NestedSetTreeDataGridBinding,
            parentSubTree: SubTreeState,
            childIndex: number,
            nodeData: Object)
        {
            this._ownerBinding = ownerBinding;
            this._parentSubTree = parentSubTree;
            this._childIndex = childIndex;
            this._nodeData = nodeData;
            this._visibleSubTreeSize = 0;
            this._subTrees = {};
            this._subTreeCount = 0;
            this._firstSubTree = null;
            this._lastSubTree = null;
            this._nextSibling = null;
            this._prevSibling = null;
            this._updateCount = 0;

            if (!parentSubTree) {
                ownerBinding.changed().bind(args => {
                    this._visibleSubTreeSize += args.count();
                });
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        public tryLocateTreeNode(rowIndex: number, thisVisibleAtIndex: number): LocatedTreeNode {
            if (rowIndex === thisVisibleAtIndex) {
                return new LocatedTreeNode(this._parentSubTree, this._childIndex, this);
            }

            if (thisVisibleAtIndex >= 0 &&
                (rowIndex < thisVisibleAtIndex || rowIndex > thisVisibleAtIndex + this._visibleSubTreeSize))
            {
                return null;
            }

            const subTreeSizeToSkip = rowIndex - thisVisibleAtIndex - 1;
            let subTreeSizeSkipped = 0;//(thisVisibleAtIndex >= 0 ? 1 : 0);
            let lastSubtree: SubTreeState = null;

            for (let currentSubtree = this._firstSubTree; currentSubtree != null; currentSubtree = currentSubtree._nextSibling) {
                const siblingsSkippedToCurrentChild = (
                    lastSubtree !== null
                    ? currentSubtree._childIndex - lastSubtree._childIndex - 1
                    : currentSubtree._childIndex);

                if (subTreeSizeSkipped + siblingsSkippedToCurrentChild > subTreeSizeToSkip) {
                    const childIndex = (lastSubtree ? lastSubtree._childIndex + 1 + subTreeSizeToSkip - subTreeSizeSkipped : subTreeSizeToSkip);
                    return new LocatedTreeNode(this, childIndex);
                }

                subTreeSizeSkipped += siblingsSkippedToCurrentChild;

                var subTreeResult = currentSubtree.tryLocateTreeNode(rowIndex, thisVisibleAtIndex + 1 + subTreeSizeSkipped);
                if (subTreeResult !== null) {
                    return subTreeResult;
                }

                subTreeSizeSkipped += (currentSubtree._visibleSubTreeSize + 1);
                lastSubtree = currentSubtree;
            }

            const childIndex = (lastSubtree ? lastSubtree._childIndex + 1 + subTreeSizeToSkip - subTreeSizeSkipped : subTreeSizeToSkip);
            return new LocatedTreeNode(this, childIndex);
        }

        //-------------------------------------------------------------------------------------------------------------

        public getChildNodeData(childIndex: number): Object  {
            let rowDataSubNodes = this._ownerBinding.getRowDataSubNodes(this._nodeData);
            return rowDataSubNodes[childIndex];
        }

        //-------------------------------------------------------------------------------------------------------------

        public getChildNodeCount(): number {
            let rowDataSubNodes = this._ownerBinding.getRowDataSubNodes(this._nodeData);
            return rowDataSubNodes.length;
        }

        //-------------------------------------------------------------------------------------------------------------

        public isLeafNode(nodeData: Object): boolean {
            let rowDataSubNodes = this._ownerBinding.getRowDataSubNodes(nodeData);
            return (!rowDataSubNodes || rowDataSubNodes.length === 0);
        }

        //-------------------------------------------------------------------------------------------------------------

        public listAllNodesData(destination: Object[]): void {
            destination.push(this._nodeData);

            let subNodesData = this._ownerBinding.getRowDataSubNodes(this._nodeData);
            let nextChildIndex: number = 0;

            for (let subTree = this._firstSubTree; subTree != null; subTree = subTree._nextSibling) {
                for ( ; nextChildIndex < subTree._childIndex ; nextChildIndex++) {
                    destination.push(subNodesData[nextChildIndex]);
                }

                subTree.listAllNodesData(destination);
                nextChildIndex = subTree._childIndex + 1;
            }

            for ( ; nextChildIndex < subNodesData.length ; nextChildIndex++) {
                destination.push(subNodesData[nextChildIndex]);
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        public createSubTree(childIndex: number): SubTreeState {
            let lastSubTree = null;

            for (let subTree = this._firstSubTree; subTree != null; subTree = subTree._nextSibling) {
                if (subTree._childIndex === childIndex) {
                    return subTree;
                }

                if (subTree._childIndex > childIndex) {
                    return this.createSubTreeChecked(childIndex, undefined, subTree);
                }

                lastSubTree = subTree;
            }

            return this.createSubTreeChecked(childIndex, lastSubTree, undefined);
        }

        //-------------------------------------------------------------------------------------------------------------

        public expand(recursive: boolean): void {
            this.beginUpdate();

            try {
                if (!this._isExpanded) {
                    let subNodesData = this._ownerBinding.getRowDataSubNodes(this._nodeData);
                    let visibleSubTreeSize = subNodesData.length;

                    for (let subTree = this._firstSubTree; subTree != null; subTree = subTree._nextSibling) {
                        visibleSubTreeSize += subTree.getVisibleSubTreeSize();
                    }

                    this._visibleSubTreeSize = visibleSubTreeSize;
                    this._isExpanded = true;
                }

                if (recursive) {
                    this.expandChildren(true);
                }
            } finally  {
                this.endUpdate();
            } 
        }

        //-------------------------------------------------------------------------------------------------------------

        public collapse(): void {
            if (this._isExpanded) {
                this.visibleSubTreeSizeChanged(-this._visibleSubTreeSize);
                this._isExpanded = false;
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        public getVisibleSubTreeSize(): number {
            return this._visibleSubTreeSize;
        }

        //-------------------------------------------------------------------------------------------------------------

        public isExpanded(): boolean {
            return this._isExpanded;
        }

        //-------------------------------------------------------------------------------------------------------------

        //public handleUpstreamChange(changeType: DataGridRowsChangeType, startIndex: number, count: number): void {
        //    let subNodesData = this._ownerBinding.getRowDataSubNodes(this._nodeData);

        //    if (this._parentSubTree != null ||
        //        changeType !== DataGridRowsChangeType.inserted ||
        //        startIndex != subNodesData.length)
        //    {
        //        return; //TODO: support more cases
        //    }

            
        //}

        //-------------------------------------------------------------------------------------------------------------

        public walkOneStep(originChildIndex: number): LocatedTreeNode {
            const childNodeCount = this.getChildNodeCount();

            if (originChildIndex < childNodeCount - 1) {
                const nextSubTree = this._subTrees[originChildIndex + 1];
                return new LocatedTreeNode(this, originChildIndex + 1, nextSubTree);
            } else if (this._parentSubTree) {
                return this._parentSubTree.walkOneStep(this._childIndex);
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        private visibleSubTreeSizeChanged(delta: number): void {
            this._visibleSubTreeSize += delta;

            if (this._parentSubTree && !this.isUpdating()) {
                this._parentSubTree.visibleSubTreeSizeChanged(delta);
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        private expandChildren(recursive: boolean): void {
            let subNodesData = this._ownerBinding.getRowDataSubNodes(this._nodeData);

            if (!subNodesData || subNodesData.length === 0) {
                return;
            }

            let nextChildIndex = subNodesData.length - 1;

            for (let subTree = this._lastSubTree; subTree != null; subTree = subTree._prevSibling) {
                for (; nextChildIndex > subTree._childIndex; nextChildIndex--) {
                    if (!this.isLeafNode(subNodesData[nextChildIndex])) {
                        const childSubTree = this.createSubTree(nextChildIndex);
                        if (childSubTree) {
                            childSubTree.expand(recursive);
                        }
                    }
                }

                subTree.expand(recursive);
                nextChildIndex--;
            }

            for (; nextChildIndex >= 0; nextChildIndex--) {
                if (!this.isLeafNode(subNodesData[nextChildIndex])) {
                    const childSubTree = this.createSubTree(nextChildIndex);
                    if (childSubTree) {
                        childSubTree.expand(recursive);
                    }
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        private createSubTreeChecked(childIndex: number, prevSibling?: SubTreeState, nextSibling?: SubTreeState): SubTreeState {
            const childNodeData = this.getChildNodeData(childIndex);
            if (this.isLeafNode(childNodeData)) {
                return null;
            }

            const newSubTree = new SubTreeState(this._ownerBinding, this, childIndex, childNodeData);

            if (prevSibling) {
                newSubTree._prevSibling = prevSibling;
                newSubTree._nextSibling = prevSibling._nextSibling;
                prevSibling._nextSibling = newSubTree;
                if (newSubTree._nextSibling) {
                    newSubTree._nextSibling._prevSibling = newSubTree;
                } else {
                    this._lastSubTree = newSubTree;
                }
            } else if (nextSibling) {
                newSubTree._nextSibling = nextSibling;
                newSubTree._prevSibling = nextSibling._prevSibling;
                nextSibling._prevSibling = newSubTree;
                if (newSubTree._prevSibling) {
                    newSubTree._prevSibling._nextSibling = newSubTree;
                } else {
                    this._firstSubTree = newSubTree;
                }
            } else {
                this._firstSubTree = newSubTree;
                this._lastSubTree = newSubTree;
            }

            this._subTrees[childIndex] = newSubTree;
            this._subTreeCount++;

            return newSubTree;
        }

        //-------------------------------------------------------------------------------------------------------------

        private beginUpdate(): void {
            if (this._updateCount === 0) {
                this._visibleSubTreeSizeBeforeUpdate = this._visibleSubTreeSize;
            }

            this._updateCount++;
        }

        //-------------------------------------------------------------------------------------------------------------

        private endUpdate(): void {
            this._updateCount--;

            if (this._updateCount === 0) {
                if (this._parentSubTree && this._visibleSubTreeSize !== this._visibleSubTreeSizeBeforeUpdate) {
                    this._parentSubTree.visibleSubTreeSizeChanged(this._visibleSubTreeSize - this._visibleSubTreeSizeBeforeUpdate);
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        private isUpdating(): boolean {
            return (this._updateCount > 0);
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    class LocatedTreeNode {
        private _parentSubTree: SubTreeState;
        private _nodeChildIndex: number;
        private _nodeSubTree: SubTreeState;
        private _nodeSubTreeInitialized: boolean;

        //-------------------------------------------------------------------------------------------------------------

        public constructor(
            parentSubTree: SubTreeState,
            nodeChildIndex: number,
            nodeSubTree?: SubTreeState)
        {
            this._parentSubTree = parentSubTree;
            this._nodeChildIndex = nodeChildIndex;
            this._nodeSubTree = nodeSubTree;
            this._nodeSubTreeInitialized = false;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getParentSubTree(): SubTreeState {
            return this._parentSubTree;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getNodeChildIndex(): number {
            return this._nodeChildIndex;
        }

        //-------------------------------------------------------------------------------------------------------------

        public getNodeData(): Object {
            return this._parentSubTree.getChildNodeData(this._nodeChildIndex);
        }

        //-------------------------------------------------------------------------------------------------------------

        public getNodeSubTree(): SubTreeState {
            return this._nodeSubTree;
        }

        //-------------------------------------------------------------------------------------------------------------

        public expand(recursive: boolean): void {
            if (!this._nodeSubTreeInitialized && !this._nodeSubTree) {
                this._nodeSubTree = this._parentSubTree.createSubTree(this._nodeChildIndex);
                this._nodeSubTreeInitialized = true;
            }

            if (this._nodeSubTree) {
                this._nodeSubTree.expand(recursive);
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        public collapse(): void {
            if (this._nodeSubTree) {
                this._nodeSubTree.collapse();
            }
        }

        //-------------------------------------------------------------------------------------------------------------

        public walkOneStep(): LocatedTreeNode {
            let nextNode: LocatedTreeNode = null;

            if (this._nodeSubTree && this._nodeSubTree.isExpanded()) {
                nextNode = this._nodeSubTree.tryLocateTreeNode(1, 0);
            }

            if (!nextNode && this._parentSubTree) {
                nextNode = this._parentSubTree.walkOneStep(this._nodeChildIndex);
            }

            return nextNode;
        }
    }
}