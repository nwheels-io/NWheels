(function () {
    'use strict';

    var app = angular.module('logViewerApp', ['ui.tree', 'isteven-multi-select', 'toggle-switch']);

	//-----------------------------------------------------------------------------------------------------------------
    
	app.controller('threadLogController', 
	['$http', '$location', '$document', '$scope', '$interval', '$anchorScroll',
	function ($http, $location, $document, $scope, $interval, $anchorScroll) {

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
			{name:'QA2', type:'QA', selected: true},
			{name:'QA1', type:'QA', selected: true},
			{name:'PROD', type:'PROD', selected: true}
		];
        $scope.availableThreads = [
			{name:'StartUp', value:1},
			{name:'ShutDown', value:2},
			{name:'IncomingRequest', value:3, selected:true},
			{name:'QueuedWorkItem', value:4, selected:true},
			{name:'ScheduledJob', value:5, selected:true},
			{name:'LogProcessing', value:6},
			{name:'Unspecified', value:0}
		];
        $scope.availableNodes = [
			{name:'Backend', selected: true},
			{name:'AdvertiserApp'}
		];
        $scope.availableLogLevels = [
			{name:'Debug'},
			{name:'Verbose'},
			{name:'Info', selected: true},
			{name:'Warning', selected: true},
			{name:'Error', selected: true},
			{name:'Critical', selected: true}
		];
        $scope.selectedEnvironments = [];
        $scope.selectedThreads = [];
        $scope.selectedNodes = [];
        $scope.selectedLogLevels = [];

		$scope.logLevelSelectorLabels = {
			selectAll       : "All",
			selectNone      : "None",
			reset           : "Undo",
			search          : "Type here to search...",
			nothingSelected : "All Threads"
		};
		
		$scope.isCaptureOn = false;
		$scope.rootNodes = [];

		var captureTimer;
		var lastCaptureId = 0;
		
		$scope.startCapture = function() {
			if(angular.isDefined(captureTimer)) {
				return;
			}

			$location.hash('autoScroll');
			
			captureTimer = $interval(function() {
				var request = {
					lastCaptureId: lastCaptureId
				};
				$http.post('/capture', request).then(function(result) {
					console.log(result.data);
					
					lastCaptureId = result.data.lastCaptureId;
					
					for (var i in result.data.logs) {
						$scope.rootNodes.push(result.data.logs[i].rootActivity);
					}

					$anchorScroll();
					
					if(result.data.logs.length > 0) {
					}
				});
			}, 1000);
		};

		$scope.stopCapture = function() {
			if (angular.isDefined(captureTimer)) {
				$interval.cancel(captureTimer);
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

	    if ($location.path().toUpperCase().indexOf('/THREADLOG/') > 0) {
	        $http.get($location.absUrl() + '/json').then(function(result) {
	            $scope.rootNodes.push(result.data.rootActivity);
	        });
	    }
		
	    //$http.get($location.absUrl() + '/json').then(function(result) {
		//	$document[0].title = 'Thread ' + result.data.logId;
        //    $scope.threadLog = result.data;
        //    $scope.rootNodes.push(result.data.rootActivity);
        //});
    }]);
	
})();