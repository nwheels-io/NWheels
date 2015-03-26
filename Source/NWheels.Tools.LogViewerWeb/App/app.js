(function () {
    'use strict';

    var app = angular.module('logViewerApp', ['ui.tree', 'isteven-multi-select', 'toggle-switch']);

	//-----------------------------------------------------------------------------------------------------------------
    
	app.controller('threadLogController', 
	['$http', '$location', '$document', '$scope', '$timeout', '$interval', '$anchorScroll',
	function ($http, $location, $document, $scope, $timeout, $interval, $anchorScroll) {

        $scope.treeOptions = {
			dataDragEnabled: false
        };

        $scope.toggle = function (scope) {
            scope.toggle();
        };

        $scope.toggleDetails = function (scope) {
            if(scope.$parent.expandedFullDetailsIndex===scope.$parent.$id) {
				scope.$parent.expandedFullDetailsIndex = -1;
			}
			else {
				scope.$parent.expandedFullDetailsIndex = scope.$parent.$id;
			}
        };

        $scope.fullDetailsVisible = function (scope) {
            return(scope.$parent.expandedFullDetailsIndex===scope.$parent.$id);
        };

        $scope.availableEnvironments = [
			{name:'QA2', type:'QA'},
			{name:'QA1', type:'QA'},
			{name:'PROD', type:'PROD'},
			{name:'Local', type:'DEV'}
		];
        $scope.availableThreads = [
			{name:'StartUp', value:1},
			{name:'ShutDown', value:2},
			{name:'IncomingRequest', value:3},
			{name:'QueuedWorkItem', value:4},
			{name:'ScheduledJob', value:5},
			{name:'LogProcessing', value:6},
			{name:'Unspecified', value:0}
		];
        $scope.availableNodes = [
			{name:'Backend'},
			{name:'AdvertiserWebApp'}
		];
        $scope.availableLogLevels = [
			{name:'Debug'},
			{name:'Verbose'},
			{name:'Info'},
			{name:'Warning'},
			{name:'Error'},
			{name:'Critical'}
		];
        $scope.selectedEnvironments = [];
        $scope.selectedThreads = [];
        $scope.selectedNodes = [];
        $scope.selectedLogLevels = [];

		$scope.filterSelectorLabels = {
			selectAll       : "Any",
			selectNone      : "Any",
			reset           : "Undo",
			search          : "Type here to search...",
			nothingSelected : "Any"
		};
		
		$scope.isCaptureOn = false;
		$scope.rootNodes = [];

		var captureTimer;
		var lastCaptureId = 0;
		var scrollPending = false;

		$scope.postRefresh = function() {
			if (scrollPending) {
				scrollPending = false;
				$('body').animate({ scrollTop: $(document).height() - $(window).height() }, 300);
			}
			if ($scope.isCaptureOn) {
				captureTimer = $timeout($scope.runRequest, 300);
			}
		};

		$scope.runRequest = function() {
			captureTimer = undefined;
			var request = {
				lastCaptureId: lastCaptureId,
				environmentNames: $linq($scope.selectedEnvironments).select(function(x) { return x.name; }).toArray(),
				nodeNames: $linq($scope.selectedNodes).select(function(x) { return x.name; }).toArray(),
				threadTypes: $linq($scope.selectedThreads).select(function(x) { return x.value; }).toArray(),
				logLevels: $linq($scope.selectedLogLevels).select(function(x) { return x.name; }).toArray(),
				maxNumberOfCaptures: 10
			};
			$http.post('/capture', request).then(function(result) {
				console.log(result.data);
				lastCaptureId = result.data.lastCaptureId;
				
				if (result.data.logs.length > 0) {
					scrollPending = true;
					for (var i in result.data.logs) {
						$scope.rootNodes.push(result.data.logs[i]);
					}
					captureTimer = $timeout($scope.postRefresh, 0);
				} else if ($scope.isCaptureOn) {
					captureTimer = $timeout($scope.runRequest, 1000);
				};
			});
		};
		
		$scope.startCapture = function() {
			if(angular.isDefined(captureTimer)) {
				return;
			}
			$scope.runRequest();
		};
		
		$scope.stopCapture = function() {
			if (angular.isDefined(captureTimer)) {
				$interval.cancel(captureTimer);
				$scope.isCaptureOn = false;
				captureTimer = undefined;
			}
		};

		$scope.$on('$destroy', function() {
			$scope.stopCapture();
        });

		$scope.$watch('isCaptureOn', function (newVal, oldVal) {
			if(newVal===true) {
				$scope.startCapture();
			}
			else {
				$scope.stopCapture();
			}
		});

	    if ($location.absUrl().toUpperCase().indexOf('/THREADLOG/') > 0) {
	        $http.get($location.absUrl() + '/json').then(function(result) {
	            $scope.rootNodes.push(result.data);
	        });
	    }
		
	    //$http.get($location.absUrl() + '/json').then(function(result) {
		//	$document[0].title = 'Thread ' + result.data.logId;
        //    $scope.threadLog = result.data;
        //    $scope.rootNodes.push(result.data.rootActivity);
        //});
    }]);
	
	app.filter('threadType', function() {
		return function (value) {
			switch(value) {
				case 1:return 'START-UP';
				case 2:return 'SHUT-DOWN';
				case 3:return 'REQ';
				case 4:return 'QUE';
				case 5:return 'JOB';
				case 6:return 'LOG';
				default:return '???';
			}
		};
	});
	
})();
