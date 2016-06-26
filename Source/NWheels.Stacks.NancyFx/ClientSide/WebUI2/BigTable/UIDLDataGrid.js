var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var UIDL;
(function (UIDL) {
    var Event = (function () {
        function Event() {
            this._handlers = [];
        }
        //-------------------------------------------------------------------------------------------------------------
        Event.prototype.bind = function (handler) {
            this._handlers.push(handler);
        };
        //-------------------------------------------------------------------------------------------------------------
        Event.prototype.unbind = function (handler) {
            this._handlers = this._handlers.filter(function (h) { return h !== handler; });
        };
        //-------------------------------------------------------------------------------------------------------------
        Event.prototype.raise = function (args) {
            this._handlers.slice(0).forEach(function (h) { return h(args); });
        };
        return Event;
    }());
    UIDL.Event = Event;
})(UIDL || (UIDL = {}));
//---------------------------------------------------------------------------------------------------------------------
var UIDL;
(function (UIDL) {
    var Widgets;
    (function (Widgets) {
        var UIDLDataGrid = (function () {
            function UIDLDataGrid() {
            }
            return UIDLDataGrid;
        }());
        Widgets.UIDLDataGrid = UIDLDataGrid;
        //-----------------------------------------------------------------------------------------------------------------
        var UIDLDataGridColumn = (function () {
            function UIDLDataGridColumn() {
            }
            return UIDLDataGridColumn;
        }());
        Widgets.UIDLDataGridColumn = UIDLDataGridColumn;
        //-----------------------------------------------------------------------------------------------------------------
        var DataGridBindingChangedEventArgs = (function () {
            function DataGridBindingChangedEventArgs() {
            }
            return DataGridBindingChangedEventArgs;
        }());
        Widgets.DataGridBindingChangedEventArgs = DataGridBindingChangedEventArgs;
        //-----------------------------------------------------------------------------------------------------------------
        var DataGridBindingBase = (function () {
            function DataGridBindingBase() {
                this._changed = new UIDL.Event();
            }
            //-------------------------------------------------------------------------------------------------------------
            DataGridBindingBase.prototype.expandRow = function (index, recursive) {
                if (recursive === void 0) { recursive = false; }
            };
            DataGridBindingBase.prototype.collapseRow = function (index) { };
            DataGridBindingBase.prototype.attachView = function (view) { };
            //-------------------------------------------------------------------------------------------------------------
            DataGridBindingBase.prototype.changed = function () {
                return this._changed;
            };
            return DataGridBindingBase;
        }());
        Widgets.DataGridBindingBase = DataGridBindingBase;
        //-----------------------------------------------------------------------------------------------------------------
        var LocalDataGridBinding = (function (_super) {
            __extends(LocalDataGridBinding, _super);
            //-------------------------------------------------------------------------------------------------------------
            function LocalDataGridBinding(rows) {
                _super.call(this);
                this._rows = rows.slice(0);
            }
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.renderRow = function (index, el) {
                // nothing
            };
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.getRowCount = function () {
                return this._rows.length;
            };
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.getRowDataAt = function (index) {
                return this._rows[index];
            };
            //-------------------------------------------------------------------------------------------------------------
            LocalDataGridBinding.prototype.getAllRowsData = function () {
                return this._rows;
            };
            return LocalDataGridBinding;
        }(DataGridBindingBase));
        Widgets.LocalDataGridBinding = LocalDataGridBinding;
        //-----------------------------------------------------------------------------------------------------------------
        var NestedSetTreeDataGridBinding = (function (_super) {
            __extends(NestedSetTreeDataGridBinding, _super);
            //-------------------------------------------------------------------------------------------------------------
            function NestedSetTreeDataGridBinding(upper, nestedSetProperty) {
                _super.call(this);
                this._upper = upper;
                this._nestedSetProperty = nestedSetProperty;
                this._treeRoot = new SubTreeState(this, null, -1, null);
                this._treeRoot.expand(false);
            }
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.renderRow = function (index, el) { };
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.getRowCount = function () {
                return this._treeRoot.getVisibleSubTreeSize();
            };
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.getRowDataAt = function (index) {
                var locatedNode = this._treeRoot.tryLocateTreeNode(index, -1);
                if (locatedNode) {
                    return locatedNode.getNodeData();
                }
                else {
                    return null;
                }
            };
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.getRowDataSubNodes = function (rowData) {
                if (rowData == null) {
                    return this._upper.getAllRowsData();
                }
                else {
                    //let rowDataAsAny = (rowData as any);
                    return rowData[this._nestedSetProperty];
                }
            };
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.getAllRowsData = function () {
                var data = [];
                this._treeRoot.listAllNodesData(data);
                return data;
            };
            //-------------------------------------------------------------------------------------------------------------
            NestedSetTreeDataGridBinding.prototype.expandRow = function (index, recursive) {
                if (recursive === void 0) { recursive = false; }
                var locatedNode = this._treeRoot.tryLocateTreeNode(index, -1);
                if (locatedNode) {
                    locatedNode.expand(recursive);
                }
            };
            return NestedSetTreeDataGridBinding;
        }(DataGridBindingBase));
        Widgets.NestedSetTreeDataGridBinding = NestedSetTreeDataGridBinding;
        //-----------------------------------------------------------------------------------------------------------------
        var SubTreeState = (function () {
            //-------------------------------------------------------------------------------------------------------------
            function SubTreeState(ownerBinding, parentSubTree, childIndex, nodeData) {
                this._ownerBinding = ownerBinding;
                this._parentSubTree = parentSubTree;
                this._childIndex = childIndex;
                this._nodeData = nodeData;
                this._visibleSubTreeSize = 0;
                this._subTrees = [];
                this._updateCount = 0;
            }
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.tryLocateTreeNode = function (rowIndex, thisVisibleAtIndex) {
                if (rowIndex === thisVisibleAtIndex) {
                    return new LocatedTreeNode(this._parentSubTree, this._childIndex, this);
                }
                if (thisVisibleAtIndex >= 0 &&
                    (rowIndex < thisVisibleAtIndex || rowIndex > thisVisibleAtIndex + this._visibleSubTreeSize)) {
                    return null;
                }
                var subTreeSizeToSkip = rowIndex - thisVisibleAtIndex - 1;
                var subTreeSizeSkipped = 0; //(thisVisibleAtIndex >= 0 ? 1 : 0);
                var lastSubtree = null;
                for (var _i = 0, _a = this._subTrees; _i < _a.length; _i++) {
                    var currentSubree = _a[_i];
                    var siblingsSkippedToCurrentChild = (lastSubtree !== null
                        ? currentSubree._childIndex - lastSubtree._childIndex - 1
                        : currentSubree._childIndex);
                    if (subTreeSizeSkipped + siblingsSkippedToCurrentChild >= subTreeSizeToSkip) {
                        var childIndex_1 = (lastSubtree ? lastSubtree._childIndex + 1 + subTreeSizeToSkip - subTreeSizeSkipped : subTreeSizeToSkip);
                        return new LocatedTreeNode(this, childIndex_1);
                    }
                    subTreeSizeSkipped += siblingsSkippedToCurrentChild;
                    var subTreeResult = currentSubree.tryLocateTreeNode(rowIndex, thisVisibleAtIndex + 1 + subTreeSizeSkipped);
                    if (subTreeResult !== null) {
                        return subTreeResult;
                    }
                    subTreeSizeSkipped += (currentSubree._visibleSubTreeSize + 1);
                    lastSubtree = currentSubree;
                }
                var childIndex = (lastSubtree ? lastSubtree._childIndex + 1 + subTreeSizeToSkip - subTreeSizeSkipped : subTreeSizeToSkip);
                return new LocatedTreeNode(this, childIndex);
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.getChildNodeData = function (childIndex) {
                var rowDataSubNodes = this._ownerBinding.getRowDataSubNodes(this._nodeData);
                return rowDataSubNodes[childIndex];
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.isLeafNode = function (nodeData) {
                var rowDataSubNodes = this._ownerBinding.getRowDataSubNodes(nodeData);
                return (!rowDataSubNodes || rowDataSubNodes.length === 0);
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.listAllNodesData = function (destination) {
                destination.push(this._nodeData);
                var subNodesData = this._ownerBinding.getRowDataSubNodes(this._nodeData);
                var nextChildIndex = 0;
                for (var _i = 0, _a = this._subTrees; _i < _a.length; _i++) {
                    var subTree = _a[_i];
                    for (; nextChildIndex < subTree._childIndex; nextChildIndex++) {
                        destination.push(subNodesData[nextChildIndex]);
                    }
                    subTree.listAllNodesData(destination);
                    nextChildIndex = subTree._childIndex + 1;
                }
                for (; nextChildIndex < subNodesData.length; nextChildIndex++) {
                    destination.push(subNodesData[nextChildIndex]);
                }
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.createSubTree = function (childIndex) {
                for (var i = 0; i < this._subTrees.length; i++) {
                    var subTree = this._subTrees[i];
                    if (subTree._childIndex === childIndex) {
                        return subTree;
                    }
                    if (subTree._childIndex > childIndex) {
                        var newSubTree_1 = new SubTreeState(this._ownerBinding, this, childIndex, this.getChildNodeData(childIndex));
                        this._subTrees.splice(i, 0, newSubTree_1);
                        return newSubTree_1;
                    }
                }
                var newSubTree = new SubTreeState(this._ownerBinding, this, childIndex, this.getChildNodeData(childIndex));
                this._subTrees.push(newSubTree);
                return newSubTree;
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.expand = function (recursive) {
                this.beginUpdate();
                try {
                    if (!this._isExpanded) {
                        var subNodesData = this._ownerBinding.getRowDataSubNodes(this._nodeData);
                        var visibleSubTreeSize = subNodesData.length;
                        for (var _i = 0, _a = this._subTrees; _i < _a.length; _i++) {
                            var subTree = _a[_i];
                            visibleSubTreeSize += subTree.getVisibleSubTreeSize();
                        }
                        this._visibleSubTreeSize = visibleSubTreeSize;
                        this._isExpanded = true;
                    }
                    if (recursive) {
                        this.expandChildren(true);
                    }
                }
                finally {
                    this.endUpdate();
                }
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.getVisibleSubTreeSize = function () {
                return this._visibleSubTreeSize;
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.visibleSubTreeSizeChanged = function (delta) {
                this._visibleSubTreeSize += delta;
                if (this._parentSubTree && !this.isUpdating()) {
                    this._parentSubTree.visibleSubTreeSizeChanged(delta);
                }
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.expandChildren = function (recursive) {
                var subNodesData = this._ownerBinding.getRowDataSubNodes(this._nodeData);
                if (!subNodesData || subNodesData.length === 0) {
                    return;
                }
                var nextChildIndex = subNodesData.length - 1;
                for (var subTreeIndex = this._subTrees.length - 1; subTreeIndex >= 0; subTreeIndex--) {
                    var subTree = this._subTrees[subTreeIndex];
                    for (; nextChildIndex > subTree._childIndex; nextChildIndex--) {
                        if (!this.isLeafNode(subNodesData[nextChildIndex])) {
                            var childSubTree = this.createSubTree(nextChildIndex);
                            childSubTree.expand(recursive);
                        }
                    }
                    subTree.expand(recursive);
                    nextChildIndex--;
                }
                for (; nextChildIndex >= 0; nextChildIndex--) {
                    if (!this.isLeafNode(subNodesData[nextChildIndex])) {
                        var childSubTree = this.createSubTree(nextChildIndex);
                        childSubTree.expand(recursive);
                    }
                }
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.beginUpdate = function () {
                if (this._updateCount === 0) {
                    this._visibleSubTreeSizeBeforeUpdate = this._visibleSubTreeSize;
                }
                this._updateCount++;
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.endUpdate = function () {
                this._updateCount--;
                if (this._updateCount === 0) {
                    if (this._parentSubTree && this._visibleSubTreeSize !== this._visibleSubTreeSizeBeforeUpdate) {
                        this._parentSubTree.visibleSubTreeSizeChanged(this._visibleSubTreeSize - this._visibleSubTreeSizeBeforeUpdate);
                    }
                }
            };
            //-------------------------------------------------------------------------------------------------------------
            SubTreeState.prototype.isUpdating = function () {
                return (this._updateCount > 0);
            };
            return SubTreeState;
        }());
        //-----------------------------------------------------------------------------------------------------------------
        var LocatedTreeNode = (function () {
            //-------------------------------------------------------------------------------------------------------------
            function LocatedTreeNode(parentSubTree, nodeChildIndex, nodeSubTree) {
                this._parentSubTree = parentSubTree;
                this._nodeChildIndex = nodeChildIndex;
                this._nodeSubTree = nodeSubTree;
            }
            //-------------------------------------------------------------------------------------------------------------
            LocatedTreeNode.prototype.getParentSubTree = function () {
                return this._parentSubTree;
            };
            //-------------------------------------------------------------------------------------------------------------
            LocatedTreeNode.prototype.getNodeChildIndex = function () {
                return this._nodeChildIndex;
            };
            //-------------------------------------------------------------------------------------------------------------
            LocatedTreeNode.prototype.getNodeData = function () {
                return this._parentSubTree.getChildNodeData(this._nodeChildIndex);
            };
            //-------------------------------------------------------------------------------------------------------------
            LocatedTreeNode.prototype.getNodeSubTree = function () {
                return this._nodeSubTree;
            };
            //-------------------------------------------------------------------------------------------------------------
            LocatedTreeNode.prototype.expand = function (recursive) {
                if (!this._nodeSubTree) {
                    this._nodeSubTree = this._parentSubTree.createSubTree(this._nodeChildIndex);
                }
                this._nodeSubTree.expand(recursive);
            };
            return LocatedTreeNode;
        }());
    })(Widgets = UIDL.Widgets || (UIDL.Widgets = {}));
})(UIDL || (UIDL = {}));
//# sourceMappingURL=UIDLDataGrid.js.map