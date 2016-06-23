'use strict';

//---------------------------------------------------------------------------------------------------------------------

function BigTable(elementId, uidl, binding) {
    
    var _rowHeight = 25;
    var _rowCount = 100000;

    var _lastScrollLeft = -1;
    var _lastScrollTop = -1;
    var _lastOffsetWidth = -1;
    var _lastOffsetHeight = -1;

    var _pageSize = -1;
    var _visiblePageSize = -1;
    var _pageTopRowIndex = -1;
    var _totalHeight = -1;
    var _pageTrEls = [ ];
    var _topScrollTrEls = [ ];
    var _bottomScrollTrEls = [ ];

    var _containerElQ;
    var _tableEl;
    var _tableBodyEl;
    var _headersTrEl;
    var _topFillerTrEl;
    var _bottomFillerTrEl;

    var _scrollHandlerTimer = null;
    var _pageRenderCount = 0;
    var _pageScrollCount = 0;
        
    var _alloweNextRenderPage = true;
    var _domSupportsNodeRemove = false;

    
    
}

//---------------------------------------------------------------------------------------------------------------------

function StaticDataLayer(dataRows) {
    
    var _dataRows = dataRows;
    
    //-----------------------------------------------------------------------------------------------------------------

    function getRowCount() {
        return _dataRows.length;
    }

    //-----------------------------------------------------------------------------------------------------------------

    function populateRow(rowIndex, trEl) {
        var indent = (rowIndex % 10) * 24;
        return {
            first: '<img src="icon.png" style="margin-left:' + indent + 'px;"> (' + rowIndex + ') first firt first first first first first first first first first first first',
            second: '<b>(' + rowIndex + ', second)</b>',
            third: '<u>(' + rowIndex + ', third)</u>',
            fourth: '(' + rowIndex + ', fourth)',
            fifth: '<font color="red">(' + rowIndex + ', fifth)</font>',
            sixth: '(' + rowIndex + ', sixth)',
            seventh: '(' + rowIndex + ', seventh)',
            eighth: '(' + rowIndex + ', eighth)',
            ninth: '(' + rowIndex + ', ninth)',
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    return {
        getRowCount: getRowCount,
        populateRow: populateRow,
        onRowsAdded: function() { },
        onRowsRemoved: function() { },
        onRowsUpdated: function() { }
    };
}

//---------------------------------------------------------------------------------------------------------------------

function TreeDataLayer(upperLayer) {
    var _upperLayer = upperLayer;
    var _rowCount = dataRows.length;
    var _onRowsAdded = [ ];
    var _onRowsRemoved = [ ];
    var _onRowsUpdated = [ ];
    
    //-----------------------------------------------------------------------------------------------------------------

    function getRowCount() {
        return _rowCount;
    }

    //-----------------------------------------------------------------------------------------------------------------

    function populateRow(rowIndex, trEl) {
        var indent = (rowIndex % 10) * 24;
        return {
            first: '<img src="icon.png" style="margin-left:' + indent + 'px;"> (' + rowIndex + ') first firt first first first first first first first first first first first',
            second: '<b>(' + rowIndex + ', second)</b>',
            third: '<u>(' + rowIndex + ', third)</u>',
            fourth: '(' + rowIndex + ', fourth)',
            fifth: '<font color="red">(' + rowIndex + ', fifth)</font>',
            sixth: '(' + rowIndex + ', sixth)',
            seventh: '(' + rowIndex + ', seventh)',
            eighth: '(' + rowIndex + ', eighth)',
            ninth: '(' + rowIndex + ', ninth)',
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    function onRowsAdded() {
    }
    
    //-----------------------------------------------------------------------------------------------------------------

    function onRowsRemoved() {
    }
    
    //-----------------------------------------------------------------------------------------------------------------

    function onRowsUpdated() {
    }

    //-----------------------------------------------------------------------------------------------------------------
    
    return {
        getRowCount: getRowCount,
        populateRow: populateRow,
        onRowsAdded: onRowsAdded,
        onRowsRemoved: onRowsRemoved,
        onRowsUpdated: onRowsUpdated
    };
}
