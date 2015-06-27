'use strict';

var theApp = angular.module('theApp', []);

//---------------------------------------------------------------------------------------------------------------------

theApp.service('uidlService', ['$q', '$http', '$rootScope', function ($q, $http, $rootScope) {
    
    var m_uidl = null;
    var m_app = null; 
    var m_index = {
        screens: { },
        screenParts: { },
    };
    var m_currentScreen = null;
    var m_behaviorImplementations = { };
    var m_controllerImplementations = { };
    
    //-----------------------------------------------------------------------------------------------------------------
    
    function setDocument(uidlDocument) {
        m_uidl = uidlDocument;
        m_app = m_uidl.applications[0];
        
        for (var i = 0; i < m_app.screens.length; i++) {
            m_index.screens[m_app.screens[i].qualifiedName] = m_app.screens[i];
        }
        
        for (var i = 0; i < m_app.screenParts.length; i++) {
            m_index.screenParts[m_app.screenParts[i].qualifiedName] = m_app.screenParts[i];
        }
        
        m_currentScreen = m_index.screens[m_app.defaultInitialScreenQualifiedName];
    }

    //-----------------------------------------------------------------------------------------------------------------

    function toCamelCase(s) {
        return s.charAt(0).toLowerCase() + s.slice(1);
    }

    //-----------------------------------------------------------------------------------------------------------------

    function translate(stringId) {
        if (!stringId) {
            return '';
        }
        var localizedString = getCurrentLocale().translations[toCamelCase(stringId)];
        if (localizedString) {
            return localizedString;
        }
        return stringId;
    }
    
    //-----------------------------------------------------------------------------------------------------------------
    /*
    function executeNotification(scope, notification, direction) {
        if (direction.indexOf('BubbleUp') > -1) {
            scope.$emit(notification.qualifiedName);
        }
        if (direction.indexOf('TunnelDown') > -1) {
            scope.$broadcast(notification.qualifiedName);
        }
    };
    */
    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['Navigate'] = {
        returnsPromise: false,
        execute: function(scope, behavior, input) {
            switch(behavior.targetType) {
                case 'Screen':
                    var screen = m_index.screens[toCamelCase(behavior.targetQualifiedName)];
                    $rootScope.currentScreen = screen;
                    $rootScope.$broadcast(screen.qualifiedName + ':NavigatedHere', input);
                    break;
                case 'ScreenPart':
                    var screenPart = m_index.screenParts[toCamelCase(targetQualifiedName)];
                    $rootScope.$broadcast(behavior.containerQualifiedName + ':NavReq', {
                        screenPart: screenPart,
                        input: input
                    });
                    break;
            }
        },
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['InvokeCommand'] = {
        returnsPromise: false,
        execute: function(scope, behavior, input) {
            scope.$emit(behavior.commandQualifiedName + ':Executing', input);
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['Broadcast'] = {
        returnsPromise: false,
        execute: function(scope, behavior, input) {
            if (behavior.direction.indexOf('BubbleUp') > -1) {
                scope.$emit(behavior.notificationQualifiedName, input);
            }
            if (behavior.direction.indexOf('TunnelDown') > -1) {
                scope.$broadcast(behavior.notificationQualifiedName, input);
            }
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['CallApi'] = {
        returnsPromise: true,
        execute: function(scope, behavior, input) {
            var requestData = { };
            var parameterContext = {
                data: scope.model.data,
                state: scope.model.state,
                input: input
            };
            for (var i = 0; i < behavior.parameterNames.length; i++) {
                var parameterValue = Enumerable.Return(parameterContext).Select('ctx=>ctx.' + behavior.parameterExpressions[i]).Single();
                requestData[behavior.parameterNames[i]] = parameterValue;
            }
            return $http.post('api/' + behavior.contractName + '/' + behavior.operationName, requestData.request);
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['AlertUser'] = {
        returnsPromise: true,
        execute: function(scope, behavior, input) {
            return $q(function(resolve, reject) {
                var uidlAlert = m_app.userAlerts[toCamelCase(behavior.alertQualifiedName)];
                scope.userAlert = {
                    uidl: uidlAlert,
                    answer: function(choice) {
                        resolve(choice);
                        scope.userAlert = null;
                    }
                };
            });            
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['AlterModel'] = {
        returnsPromise: false,
        execute: function(scope, behavior, input) {
            //TBD
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['ScreenPartContainer'] = {
        implement: function(scope) {
            scope.$on(scope.uidl.qualifiedName + ':NavReq', function(event, data) {
                scope.currentScreenPart = data.screenPart;
                scope.$broadcast(data.screenPart.qualifiedName + ':NavigatedHere', data.input);
            });
        },
    };

    //-----------------------------------------------------------------------------------------------------------------
    /*
    function executeBehavior(scope, behavior, eventArgs) {
        switch(behavior.behaviorType) {
            case 'Navigate':
                return $q(function(resolve, reject) {
                    switch(behavior.targetType) {
                        case 'Screen':
                            var screen = m_index.screens[toCamelCase(targetQualifiedName)];
                            $rootScope.currentScreen = screen;
                            break;
                        case 'ScreenPart':
                            var screenPart = m_index.screenParts[toCamelCase(targetQualifiedName)];
                            $rootScope.$broadcast(behavior.containerQualifiedName + '.NavReq', screenPart);
                            break;
                    }
                    resolve(eventArgs);
                });
            case 'InvokeCommand':
                return $q(function(resolve, reject) {
                    $rootScope.$broadcast(behavior.commandQualifiedName + '_Executing');
                    resolve(eventArgs);
                });
            case 'Broadcast':
                return $q(function(resolve, reject) {
                    if (behavior.direction.indexOf('BubbleUp') > -1) {
                        scope.$emit(behavior.notificationQualifiedName);
                    }
                    if (behavior.direction.indexOf('TunnelDown') > -1) {
                        scope.$broadcast(behavior.notificationQualifiedName);
                    }
                    resolve(eventArgs);
                });
            case 'CallApi':
                return $http.post('api/' + behavior.contractName + '/' + behavior.operationName);
        }
    };
    */
    //-----------------------------------------------------------------------------------------------------------------

    function implementBehavior(scope, behavior, input) {
        var impl = m_behaviorImplementations[behavior.behaviorType];
        var implResult = impl.execute(scope, behavior, input);
        var promise = null;
        
        if (impl.returnsPromise) {
            promise = implResult;
        }
        else {
            promise = $q(function(resolve, reject) { 
                resolve(input); 
            });
        }

        promise.then(
            function(result) {
                if(behavior.onSuccess) {
                    implementBehavior(scope, behavior.onSuccess, result);
                }
            },
            function(error) {
                if(behavior.onFailure) {
                    implementBehavior(scope, behavior.onFailure, error);
                }
            });
    }

    //-----------------------------------------------------------------------------------------------------------------

    function implementSubscription(scope, behavior) {
        scope.$on(behavior.subscription.notificationQualifiedName, function(event, input) {
            implementBehavior(scope, behavior, input);
        });
    }
    
    //-----------------------------------------------------------------------------------------------------------------

    function implementController(scope) {
        scope.translate = translate;
        scope.model = {
            data: { },
            state: { }
        };
        
        if (scope.uidl) {

            if (scope.uidl.widgetType && m_controllerImplementations[scope.uidl.widgetType]) {
                m_controllerImplementations[scope.uidl.widgetType].implement(scope);
            }

            for (var i = 0; i < scope.uidl.behaviors.length; i++) {
                var behavior = scope.uidl.behaviors[i];
                if (behavior.subscription) {
                    implementSubscription(scope, behavior);
                    //scope.$on(behavior.subscription.notificationQualifiedName, function(event, input) {
                    //    implementBehavior(scope, behavior, input);
                    //});
                }
            }
        }
    }
    
    //-----------------------------------------------------------------------------------------------------------------

    function getApp() {
        return m_app;
    }
    function getCurrentScreen() {
        return m_currentScreen;
    }
    function getCurrentLocale() {
        return m_uidl.locales['en-US'];
    }

    //-----------------------------------------------------------------------------------------------------------------

    return {
        setDocument: setDocument,
        getApp: getApp,
        getCurrentScreen: getCurrentScreen,
        getCurrentLocale: getCurrentLocale,
        implementController: implementController,
    };
    
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.controller('appStart', ['$http', '$scope', '$rootScope', 'uidlService', function ($http, $scope, $rootScope, uidlService) {

    $scope.pageTitle = 'LOADING . . .';

    $http.get('uidl.json').then(function(httpResult) {
        uidlService.setDocument(httpResult.data);
        
        $rootScope.app = uidlService.getApp();
        $rootScope.uidl = uidlService.getApp();

        uidlService.implementController($scope);

        $rootScope.currentScreen = uidlService.getCurrentScreen();
        $rootScope.currentLocale = uidlService.getCurrentLocale();
        $scope.pageTitle = $scope.translate($scope.app.text) + ' - ' + $scope.translate($scope.currentScreen.text);
    });

    /*
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
    */
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlScreen', ['uidlService', function(uidlService) {
    return {
        scope: {
            uidl: '='
        },
        restrict: 'E',
        replace: true,
        link: function(scope, elem, attrs) { },
        template: '<ng-include src="\'uidl-screen\'"></ng-include>',
        controller: function ($scope) {
            uidlService.implementController($scope);
        }        
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlScreenPart', ['uidlService', function(uidlService) {
    return {
        scope: {
            uidl: '='
        },
        restrict: 'E',
        replace: true,
        link: function(scope, elem, attrs) { },
        template: '<ng-include src="\'uidl-screen-part\'"></ng-include>',
        controller: function ($scope) {
            uidlService.implementController($scope);
        }        
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlWidget', ['uidlService', function(uidlService) {
    return {
        scope: {
            uidl: '='
        },
        restrict: 'E',
        replace: true,
        link: function(scope, elem, attrs) { },
        template: '<ng-include src="\'uidl-element-template-\' + uidl.templateName"></ng-include>',
        controller: function ($scope) {
            uidlService.implementController($scope);
        }        
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlUserAlertInline', ['uidlService', function(uidlService) {
    return {
        scope: {
            alert: '='
        },
        restrict: 'E',
        replace: false,
        templateUrl: 'uidl-user-alert-inline',
        controller: function($scope) {
            uidlService.implementController($scope);
        },
    } 
}]);

//---------------------------------------------------------------------------------------------------------------------

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

//---------------------------------------------------------------------------------------------------------------------

theApp.filter('localized', ['$scope', function($scope) {
    return function (stringId) {
        var localizedString = $scope.currentLocale.translations[stringId];
        if (localizedString) {
            return localizedString;
        }
        else {
            return stringId;
        }
    };
}]);

