(function () {
    'use strict';

    angular.module('logViewerApp', ['ui.tree'])
    .controller('threadLogController', ['$http', '$location', '$document', '$scope', function ($http, $location, $document, $scope) {

        $http.get($location.absUrl() + '/json').then(function(result) {
			$document[0].title = 'Thread ' + result.data.logId;
            $scope.threadLog = result.data;
            $scope.rootNodes = [result.data.rootActivity];
        });
        
        $scope.treeOptions = {
			dataDragEnabled: false
        };

        $scope.toggle = function (scope) {
            scope.toggle();
        };
    }]);

})();