(function () {
    'use strict';

    angular.module('logViewerApp', ['ui.tree'])
    .controller('threadLogController', ['$http', '$location', '$scope', function ($http, $location, $scope) {

        $http.get($location.absUrl() + '/json').then(function (result) {
            $scope.rootActivity = result.data.rootActivity;
        });

        $scope.treeOptions = {
        };

        $scope.toggle = function (scope) {
            scope.toggle();
        };
    }]);

})();