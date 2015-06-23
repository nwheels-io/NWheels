'use strict';

var theApp = angular.module('theApp', []);

theApp.service('uidlService', ['$rootScope', function ($rootScope) {
    
    var m_uidl = null;
    var m_index = {
        app: null,
        screens: { },
        screenParts: { },
        widgets: { },
        commands: { },
        notifications: { },
        behaviors: { }
    };
    
    $http.get('uidl.json').then(function(httpResult) {
        m_uidl = httpResult.data;
        $rootScope.uidl = m_uidl;
    });
    
    
    
}]);

theApp.controller('appStart', ['$q', '$http', '$scope', '$rootScope', function ($q, $http, $scope, $rootScope) {

    var uidl = null;
    
    $http.get('meta.json').then(function(httpResult) {
        uidl = httpResult.data;
        $rootScope.uidl = uidl;
        $rootScope.currentScreen = uidl.screens[0];
    });
    
    $rootScope.executeNotification = function(notification, callingScope) {
        for (var i = 0; i < notification.subscribers.length; i++) {
            $rootScope.executeBehavior(notification.subscribers[i], callingScope);
        }
    };

    $rootScope.executeBehavior = function(behavior, callingScope) {
        switch(behavior.behaviorType) {
            case 'Navigate':
                switch(behavior.targetType) {
                    case 'Screen':
                        var screen = Enumerable.From(uidl.screens).First(function(s) { return s.idName === behavior.targetIdName });
                        $rootScope.currentScreen = screen;
                        break;
                    case 'ScreenPart':
                        var screenPart = Enumerable.From(uidl.screenParts).First(function(s) { return s.idName === behavior.targetIdName });
                        $rootScope.$broadcast(behavior.containerQualifiedName + '.NavReq', screenPart);
                        break;
                }
                break;
            case 'InvokeCommand':
                $rootScope.$broadcast(behavior.commandQualifiedName + '_Executing');
                break;
        }
    };
    
}]);



theApp.directive('uidlScreen', function() {
    return {
        scope: {
            uidl: '='
        },
        restrict: 'E',
        replace: true,
        link: function(scope, elem, attrs) { },
        template: '<ng-include src="\'uidl-screen\'"></ng-include>'
    };
});

theApp.directive('uidlScreenPart', function() {
    return {
        scope: {
            uidl: '='
        },
        restrict: 'E',
        replace: true,
        link: function(scope, elem, attrs) { },
        template: '<ng-include src="\'uidl-screen-part\'"></ng-include>'
    };
});

theApp.directive('uidlWidget', function() {
    return {
        scope: {
            uidl: '='
        },
        restrict: 'E',
        replace: true,
        link: function(scope, elem, attrs) { },
        template: '<ng-include src="\'uidl-element-template-\' + uidl.elementType"></ng-include>'
    };
});

theApp.directive('uidlUserAlertInline', function() {
   return {
       scope: {
           alert: '='
       },
       restrict: 'E',
       replace: false,
       controller: function($scope) {
           $scope.hideAlert = function() {
               $scope.alert = null;
           }
       },
       templateUrl: 'uidl-user-alert-inline'
   } 
});

theApp.directive('uidlController', ['$compile', '$parse', function($compile, $parse) {
    return {
        scope: true,
        restrict: 'A',
        terminal: true,
        priority: 100000,
        link: function(scope, elem, attrs) {
            elem.attr('ng-controller', scope.uidl.qualifiedName);
            elem.removeAttr('uidl-controller');
            $compile(elem)(scope);
        }
    };
}]);
