 /*
  * Copyright (c) 2010 James Brantly
  * 
  * Permission is hereby granted, free of charge, to any person
  * obtaining a copy of this software and associated documentation
  * files (the "Software"), to deal in the Software without
  * restriction, including without limitation the rights to use,
  * copy, modify, merge, publish, distribute, sublicense, and/or sell
  * copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following
  * conditions:
  *
  * The above copyright notice and this permission notice shall be
  * included in all copies or substantial portions of the Software.
  *
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
  * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
  * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
  * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
  * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
  * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
  * OTHER DEALINGS IN THE SOFTWARE.
  */

var bigtable = function(id, columns, numRows) {
	this._containerEl = document.getElementById(id);
	this._columns = columns;
	this._numRows = numRows;
	
	this._rowHeight = 20;
	
	this.createHTML();
	this.createEventHandlers();
	
	this.initializeTable();
	this.renderVisibleRows();
};

bigtable.prototype.createHTML = function() {
	// This creates HTML with the following structure:
	
	//<div class="bigtable-container">
	//	<div class="bigtable-headers">
	//		<table cellspacing="0" cellpadding="0" border="0">
	//			<thead>
	// 				<tr>
	// 					<th style="width: 100px;">Column Label</th>
	// 					...
	// 				</tr>
	// 			</thead>
	// 		</table>
	// 	</div>
	// 	<div class="bigtable-body">
	// 		<table cellspacing="0" cellpadding="0" border="0">
	// 			<thead>
	// 				<tr>
	// 					<th style="width: 100px;"></th>
	// 					...
	// 				</tr>
	// 			</thead>
	// 			<tbody>
	// 			</tbody>
	// 		</table>
	// 	</div>
	//</div>
	
	this._containerEl.className += ' bigtable-container';
	
	var headersEl = document.createElement('div');
	headersEl.className = 'bigtable-headers';
	
	var headersTable = document.createElement('table');
	headersTable.setAttribute('cellspacing', '0');
	headersTable.setAttribute('cellpadding', '0');
	headersTable.setAttribute('border', '0');
	
	var headersTHead = document.createElement('thead');
	
	var headersTHeadTr = document.createElement('tr');
	
	for (var i = 0, n = this._columns.length; i<n; i++) {
		var th = document.createElement('th');
		th.appendChild(document.createTextNode(this._columns[i]));
		th.style.width = '100px';
		headersTHeadTr.appendChild(th);
	}
	
	var bodyEl = document.createElement('div');
	bodyEl.className = 'bigtable-body';
	
	var bodyTable = document.createElement('table');
	bodyTable.setAttribute('cellspacing', '0');
	bodyTable.setAttribute('cellpadding', '0');
	bodyTable.setAttribute('border', '0');
	
	var bodyTHead = document.createElement('thead');
	var bodyTHeadTr = document.createElement('tr');
	
	for (var i = 0, n = this._columns.length; i<n; i++) {
		var th = document.createElement('th');
		th.style.width = '100px';
		bodyTHeadTr.appendChild(th);
	}
	
	var bodyTBody = document.createElement('tbody');

	headersTHead.appendChild(headersTHeadTr);
	headersTable.appendChild(headersTHead);
	headersEl.appendChild(headersTable);
	
	bodyTHead.appendChild(bodyTHeadTr);
	bodyTable.appendChild(bodyTHead);
	bodyTable.appendChild(bodyTBody);
	bodyEl.appendChild(bodyTable);
	
	this._containerEl.appendChild(headersEl);
	this._containerEl.appendChild(bodyEl);
	
	this._headersEl = headersEl;
	this._bodyEl = bodyEl;
	this._bodyTable = bodyTable;
	this._bodyTBody = bodyTBody;
};

bigtable.prototype.createEventHandlers = function() {
	this._curScrollLeft = null;
	this._curScrollTop = null;
	
	var headersEl = this._headersEl;
	var bodyEl = this._bodyEl;
	
	var self = this;

	Event.observe(bodyEl, 'scroll', function() {
		// Handle left/right scroll
		var scrollLeft = bodyEl.scrollLeft;
		if (self._curScrollLeft != scrollLeft) {
			headersEl.scrollLeft = scrollLeft;
			self._curScrollLeft = scrollLeft;
		}
		
		// Handle up/down scroll
		var scrollTop = bodyEl.scrollTop;
		if (self._curScrollTop != scrollTop) {
			if (self._verticalScrollHandle != null) {
				clearTimeout(self._verticalScrollHandle);
			}
			
			self._verticalScrollHandle = setTimeout(function() {
				self.renderVisibleRows();
			}, 200);
			self._curScrollTop = scrollTop;
		}
	});
};

bigtable.prototype.initializeTable = function() {
	// Setup table height
	var rowHeight = this._rowHeight;
	var numRows = this._numRows;
	
	// Set the tbody height to the total height
	this._bodyTable.style.height = numRows > 0 ? (numRows*rowHeight)+'px' : '1px';
	
	// Create the initial filler tr
	var filler = document.createElement('tr');
	filler.style.height = (numRows*rowHeight)+'px';
	this._bodyTBody.appendChild(filler);

	// Leftover space filler (to keep explicit height rows from stretching)
	var tr = document.createElement('tr');
	this._bodyTBody.appendChild(tr);
	
	// Create some metadata for each row to keep track of whether or not the row has been rendered yet
	var rows = this._rows = [];
	
	// Create & initialize the filler metadata
	var fillers = this._fillers = [];
	fillers.push({
		startIndex: 0,
		endIndex: numRows-1,
		tr: filler
	});
};

bigtable.prototype.renderVisibleRows = function() {
	var visibleIndexes = this.getVisibleIndexes();
	
	if (visibleIndexes.startIndex == null || visibleIndexes.endIndex == null) { return; }
	
	var rows = this._rows;

	var columns = this._columns;
	var numColumns = columns.length;
	
	this.createRowElementsAt(visibleIndexes.startIndex, visibleIndexes.endIndex);
	
	for (var i = visibleIndexes.startIndex; i<=visibleIndexes.endIndex; i++) {
		var rowMeta = rows[i];
		var tr = rowMeta.tr;
		
		// Create cells if not already created
		if (rowMeta.cellsCreated !== true) {
			for (var x = 0; x<numColumns; x++) {
				var td = document.createElement('td');
				td.appendChild(document.createTextNode('('+x+', '+i+')'));
				tr.appendChild(td);
			}
			rowMeta.cellsCreated = true;
		}
	}
	
	// fix for ie
	this._bodyTable.className = this._bodyTable.className;
};

bigtable.prototype.createRowElementsAt = function(startIndex, endIndex) {
	var rows = this._rows;			

	// Initialize row metadata
	for (var i = startIndex; i<=endIndex; i++) {
		if (rows[i] == null) {
			rows[i] = {};
		}
	}
	
	// quick sanity check
	var atLeastOne = false;
	for (var i = startIndex; i<=endIndex; i++) {
		if (rows[i].tr == null) {
			atLeastOne = true;
			break;
		}
	}
	if (!atLeastOne) { return; }
	
	var tbodyEl = this._bodyTBody;
	var rowHeight = this._rowHeight;
	var fillers = this._fillers;
	
	// filler has following structure:
	//   startIndex
	//   endIndex
	//   tr
	
	// Find relevant fillers
	var relevantFillers = [];
	for (var i = fillers.length; i--;) {
		var filler = fillers[i];
		if (startIndex <= filler.endIndex && endIndex >= filler.startIndex) {
			relevantFillers.push(filler);
		}
	}

	// Handle each filler in turn
	for (var i = relevantFillers.length; i--;) {
		var filler = relevantFillers[i];
		
		// 1 of 4 possibilities
		// the entire filler is being filled
		// the first portion is being filled
		// the last portion is being filled
		// the middle portion is being filled
		
		// 0, 1, or 2 fillers "leftover"
		
		
		// figure out which indices in this filler are being filled
		var fillerStart = startIndex >= filler.startIndex ? startIndex : filler.startIndex; // max
		var fillerEnd = endIndex <= filler.endIndex ? endIndex : filler.endIndex; // min
		
		// fill the appropriate indices
		for (var x = fillerStart; x<=fillerEnd; x++) {
			var rowMeta = rows[x];
			var tr = document.createElement('tr');
			tr.className = 'bigtable-row';
			tr.style.height = rowHeight+'px';
			tbodyEl.insertBefore(tr, filler.tr);
			rowMeta.tr = tr;
		}
		
		// 0, 1, or 2 fillers "leftover"
		// filler at the beginning
		// filler at the end
		// filler at the beginning and end
		// no filler
		
		// reorganize fillers
		var usedFiller = false;
		var oldFillerStartIndex = filler.startIndex;
		
		// if filler at end, reuse existing filler w/o moving
		if (fillerEnd < filler.endIndex) {
			filler.startIndex = fillerEnd+1;
			filler.tr.style.height = ((filler.endIndex-filler.startIndex+1)*rowHeight)+'px';
			usedFiller = true;
		}
		
		if (fillerStart > oldFillerStartIndex) {
			if (usedFiller) { // create new filler
				var tr = document.createElement('tr');
				var filler = {
					startIndex: oldFillerStartIndex,
					endIndex: fillerStart-1,
					tr: tr
				};
				filler.tr.style.height = ((filler.endIndex-filler.startIndex+1)*rowHeight)+'px';
				tbodyEl.insertBefore(tr, rows[fillerStart].tr);
				fillers.push(filler);
				
			}
			else { // reuse existing filler w/ moving
				tbodyEl.insertBefore(filler.tr, rows[fillerStart].tr);
				filler.endIndex = fillerStart-1;
				filler.tr.style.height = ((filler.endIndex-filler.startIndex+1)*rowHeight)+'px';
				usedFiller = true;
			}
		}
		
		// remove filler if completely filled
		if (!usedFiller) {
			for (var x = fillers.length; x--;) {
				if (filler == fillers[x]) {
					fillers.splice(x, 0);
					break;
				}
			}
			tbodyEl.removeChild(filler.tr)
		}
	}
};

bigtable.prototype.getVisibleIndexes = function() {
	var numRows = this._numRows

	if (numRows == 0) {
		return {startIndex: null, endIndex: null};
	}
	
	// get visible pixel coordinates
	var startY = this._bodyEl.scrollTop;
	var endY = startY+this._bodyEl.offsetHeight;
	
	// translate pixels into row indices
	var startIndex = Math.floor(startY/this._rowHeight);
	var endIndex = Math.floor(endY/this._rowHeight);
	
	// sanity check
	if (endIndex>numRows-1) {
		endIndex = numRows-1;
	}
	
	return {startIndex: startIndex, endIndex: endIndex};
};