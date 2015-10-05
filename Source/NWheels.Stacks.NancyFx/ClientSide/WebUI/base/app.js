'use strict';

var theApp = angular.module('theApp', ['breeze.angular']);

//---------------------------------------------------------------------------------------------------------------------

theApp.service('commandService',
['$http', '$q', '$interval', '$timeout',
function ($http, $q, $interval, $timeout) {

    var m_pendingCommands = {};
    var m_pollTimer = null;

    //-----------------------------------------------------------------------------------------------------------------

    function sendCommand(callType, requestPath, requestData) {
        var commandCompletion = $q.defer();

        $http.post(requestPath, requestData).then(
            function (response) {
                if (callType === 'OneWay') {
                    commandCompletion.resolve({ success: true });
                } else {
                    var resultMessage = response.data;
                    //m_pendingCommands[response.data.commandMessageId] = commandCompletion;
                    if (resultMessage.success === true) {
                        commandCompletion.resolve(resultMessage.result);
                    } else {
                        commandCompletion.reject(resultMessage);
                    }
                }
            },
            function (response) {
                commandCompletion.reject({
                    success: false,
                    faultCode: response.status,
                    faultReason: response.statusText
                });
            }
        );

        return commandCompletion.promise;
    }

    //-----------------------------------------------------------------------------------------------------------------

    function receiveMessages() {
        $http.post('takeMessages').then(
            function (response) {
                for (var i = 0 ; i < response.data.length ; i++) {
                    var message = response.data[i];
                    if (message.type === 'Commands.CommandResultMessage') {
                        var commandCompletion = m_pendingCommands[message.commandMessageId];
                        if (commandCompletion) {
                            if (message.success === true) {
                                commandCompletion.resolve(message.result);
                            } else {
                                commandCompletion.reject(message);
                            }
                            delete m_pendingCommands[message.commandMessageId];
                        }
                    }
                    //TODO: dispatch received push messages other than command completions
                }
            },
            function (response) {
                //TODO: alert user there is a connectivity problem with the server
            }
        );
    }

    //-----------------------------------------------------------------------------------------------------------------

    function startPollingMessages() {
        if (!m_pollTimer) {
            m_pollTimer = $interval(receiveMessages, 2000);
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    function stopPollingMessages() {
        if (m_pollTimer) {
            $interval.cancel(m_pollTimer);
            m_pollTimer = null;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    return {
        sendCommand: sendCommand,
        receiveMessages: receiveMessages,
        startPollingMessages: startPollingMessages,
        stopPollingMessages: stopPollingMessages
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.service('uidlService',
['$q', '$http', '$rootScope', '$timeout', 'commandService',
function ($q, $http, $rootScope, $timeout, commandService) {

    var m_uidl = null;
    var m_app = null;
    var m_index = {
        screens: {},
        screenParts: {},
    };
    var m_currentScreen = null;
    var m_behaviorImplementations = {};
    var m_controllerImplementations = {};

    //var m_pendingCommands = { };

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
            promise = $q(function (resolve, reject) {
                resolve(input);
            });
        }

        promise.then(
            function (result) {
                if (behavior.onSuccess) {
                    implementBehavior(scope, behavior.onSuccess, result);
                }
            },
            function (error) {
                if (behavior.onFailure) {
                    implementBehavior(scope, behavior.onFailure, error);
                }
            });
    }

    //-----------------------------------------------------------------------------------------------------------------

    function implementSubscription(scope, behavior) {
        scope.$on(behavior.subscription.notificationQualifiedName, function (event, input) {
            console.log('uidlService::on-behavior', behavior.qualifiedName);
            implementBehavior(scope, behavior, input);
        });
    }

    //-----------------------------------------------------------------------------------------------------------------

    function implementController(scope) {
        scope.translate = translate;
        scope.model = {
            data: {},
            state: {}
        };
        scope.appScope = $rootScope.appScope;

        if (scope.uidl) {
            console.log('uidlService::implementController', scope.uidl.qualifiedName);

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
    function getMetaType(name) {
        return m_uidl.metaTypes[toCamelCase(name)];
    }

    //-----------------------------------------------------------------------------------------------------------------
    /*
        function takeMessagesFromServer() {
            $http.post('takeMessages').then(
                function(response) {
                    for ( var i = 0 ; i < response.data.length ; i++ )
                    {
                        var message = response.data[i];
                        if (message.type==='Commands.CommandResultMessage') {
                            var commandCompletion = m_pendingCommands[message.commandMessageId];
                            if (commandCompletion) {
                                if (message.success === true) {
                                    commandCompletion.resolve(message.result);
                                } else {
                                    commandCompletion.reject(message);
                                }
                                delete m_pendingCommands[message.commandMessageId];
                            }
                        }
                        //TODO: dispatch received push messages other than command completions
                    }
                },
                function(response) {
                    //TODO: alert user there is a connectivity problem with the server
                }
            );
        }
    */
    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['Navigate'] = {
        returnsPromise: false,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > navigate', behavior.targetType, behavior.targetQualifiedName);
            switch (behavior.targetType) {
                case 'Screen':
                    var screen = m_index.screens[behavior.targetQualifiedName];
                    $rootScope.currentScreen = screen;
                    location.hash = screen.qualifiedName;
                    $timeout(function() {
                        $rootScope.$broadcast(screen.qualifiedName + ':NavigatedHere', input);
                    });
                    break;
                case 'ScreenPart':
                    var screenPart = m_index.screenParts[behavior.targetQualifiedName];
                    $rootScope.$broadcast(behavior.targetContainerQualifiedName + ':NavReq', {
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
        execute: function (scope, behavior, input) {
            console.log('run-behavior > invokeCommand', behavior.commandQualifiedName);
            if (scope.$parent) {
                scope.$parent.$emit(behavior.commandQualifiedName + ':Executing', input);
            }
            scope.$broadcast(behavior.commandQualifiedName + ':Executing', input);
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['Broadcast'] = {
        returnsPromise: false,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > broadcast', behavior.notificationQualifiedName, behavior.direction);
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
        execute: function (scope, behavior, input) {
            console.log('run-behavior > callApi', behavior.callTargetType, behavior.contractName, behavior.operationName);
            var requestData = {};
            var parameterContext = {
                data: scope.model.data,
                state: scope.model.state,
                input: input
            };
            for (var i = 0; i < behavior.parameterNames.length; i++) {
                var parameterValue = Enumerable.Return(parameterContext).Select('ctx=>ctx.' + behavior.parameterExpressions[i]).Single();
                requestData[behavior.parameterNames[i]] = parameterValue;
            }
            var requestPath = 
                'command/' + behavior.callType + 
                '/' + behavior.callTargetType + 
                '/' + behavior.contractName + 
                '/' + behavior.operationName;
            
            return commandService.sendCommand(behavior.callType, requestPath, requestData);
            /*            
                        var commandCompletion = $q.defer();
                        
                        $http.post(requestPath, requestData).then(
                            function(response) {
                                if (behavior.callType==='OneWay') {
                                    commandCompletion.resolve({ success: true });
                                } else {
                                    m_pendingCommands[response.data.commandMessageId] = commandCompletion;
                                }
                            },
                            function(response) {
                                commandCompletion.reject({
                                    success: false,
                                    faultCode: response.status,
                                    faultReason: 'Error: ' + response.statusText  
                                });
                            }
                        );
                        
                        return commandCompletion.promise;
            */
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['AlertUser'] = {
        returnsPromise: true,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > alertUser');
            return $q(function (resolve, reject) {
                var uidlAlert = m_app.userAlerts[toCamelCase(behavior.alertQualifiedName)];
                scope.userAlert = {
                    uidl: uidlAlert,
                    answer: function (choice) {
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
        execute: function (scope, behavior, input) {
            scope.model.input = input;
            var context = {
                model: scope.model
            };
            for (var i = 0; i < behavior.alterations.length; i++) {
                var alteration = behavior.alterations[i];
                switch (alteration.type) {
                    case 'Copy':
                        var value = (
                            alteration.sourceExpression==='null' || !alteration.sourceExpression
                            ? null
                            : Enumerable.Return(context).Select('ctx=>ctx.' + alteration.sourceExpression).Single());
                        var target = context;
                        for (var j = 0; j < alteration.destinationNavigations.length - 1; j++) {
                            target = target[alteration.destinationNavigations[j]];
                        }
                        var lastNavigation = alteration.destinationNavigations[alteration.destinationNavigations.length-1];
                        target[lastNavigation] = value;
                        break;
                }
            }
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['ScreenPartContainer'] = {
        implement: function (scope) {
            scope.$on(scope.uidl.qualifiedName + ':NavReq', function (event, data) {
                console.log('screenPartContainer::on-NavReq', scope.uidl.qualifiedName, '->', data.screenPart.qualifiedName);
                scope.currentScreenPart = data.screenPart;
                location.hash = data.screenPart.qualifiedName;
                $timeout(function() {
                    scope.$broadcast(data.screenPart.qualifiedName + ':NavigatedHere', data.input);
                });
            });
            if (scope.uidl.initalScreenPartQualifiedName) {
                scope.currentScreenPart = m_index.screenParts[scope.uidl.initalScreenPartQualifiedName];
                $timeout(function() {
                    scope.$broadcast(scope.uidl.initalScreenPartQualifiedName + ':NavigatedHere');
                });
            }
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['ManagementConsole'] = {
        implement: function (scope) {
            function implementMenuItems(items) {
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    for (var j = 0; j < item.behaviors.length; j++) {
                        var behavior = item.behaviors[j];
                        if (behavior.subscription) {
                            implementSubscription(scope, behavior);
                        }
                    }
                    implementMenuItems(item.subItems);
                }
            }

            implementMenuItems(scope.uidl.mainMenu.items);
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['Report'] = {
        implement: function (scope) {
            var metaType = scope.uidlService.getMetaType(scope.uidl.entityMetaType);

            scope.displayProperties = Enumerable.From(scope.uidl.displayColumns).Select(function (name) {
                return metaType.properties[toCamelCase(name)];
            }).ToArray();

            scope.queryEntities = function () {
                scope.entityService.queryEntity(scope.uidl.entityName).then(function (data) {
                    scope.resultSet = data.results;
                });
            };

            scope.viewEntity = function (entity) {
                alert('VIEW ENTITY ' + entity);
            };

            scope.queryEntities();
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['Crud'] = {
        implement: function (scope) {
            var metaType = scope.uidlService.getMetaType(scope.uidl.entityMetaType);

            scope.displayProperties = Enumerable.From(scope.uidl.displayColumns).Select(function (name) {
                return metaType.properties[toCamelCase(name)];
            }).ToArray();

            scope.queryEntities = function () {
                scope.entityService.queryEntity(scope.uidl.entityName).then(function (data) {
                    scope.resultSet = data.results;
                });
            };

            scope.editEntity = function (entity) {
                scope.model.entity = entity;
                scope.uiShowCrudForm = true;
            };

            scope.newEntity = function () {
                scope.model.entity = scope.entityService.createEntity(metaType.restTypeName, {});
                scope.uiShowCrudForm = true;
            };

            scope.deleteEntity = function (entity) {
                scope.entityService.deleteEntityAndSave(entity);
            };

            scope.$on(scope.uidl.form.qualifiedName + ':Closing', function (input) {
                scope.uiShowCrudForm = false;
            });

            scope.queryEntities();
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['CrudForm'] = {
        implement: function (scope) {
            var metaType = scope.uidlService.getMetaType(scope.uidl.entityMetaType);

            scope.notifyFormClosing = function () {
                scope.$emit(scope.uidl.qualifiedName + ':Closing');
            };

            scope.saveChanges = function () {
                scope.entityService.saveChanges();
                scope.notifyFormClosing();
            };

            scope.cancelEdit = function () {
                scope.entityService.rejectChanges();
                scope.notifyFormClosing();
            };
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    return {
        setDocument: setDocument,
        getApp: getApp,
        getCurrentScreen: getCurrentScreen,
        getCurrentLocale: getCurrentLocale,
        getMetaType: getMetaType,
        //takeMessagesFromServer: takeMessagesFromServer,
        implementController: implementController,
    };

}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.factory('entityManagerFactory', ['breeze', function (breeze) {
    configureBreeze();
    var serviceRoot = window.location.protocol + '//' + window.location.host + '/';
    var serviceName = serviceRoot + 'rest/';
    var factory = {
        newManager: newManager,
        serviceName: serviceName
    };

    return factory;

    function configureBreeze() {
        // use Web API OData to query and save
        breeze.config.initializeAdapterInstance('dataService', 'webApiOData', true);

        // convert between server-side PascalCase and client-side camelCase
        breeze.NamingConvention.camelCase.setAsDefault();
    }

    function newManager() {
        var mgr = new breeze.EntityManager(serviceName);
        return mgr;
    }
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.service('entityService',
['$http', '$q', '$timeout', 'breeze', 'logger',
function ($http, $q, $timeout, breeze, logger) {

    var serviceRoot = window.location.protocol + '//' + window.location.host + '/';
    var serviceName = serviceRoot + 'rest/';
    //var serviceUrl = 'http://localhost:8900/rest/UserAccounts/'; // route to the same origin Web Api controller

    // *** Cross origin service example  ***
    // When data server and application server are in different origins
    //var serviceName = 'http://sampleservice.breezejs.com/api/todos/';

    //breeze.config.initializeAdapterInstance('dataService', 'webApiOData', true);
    //breeze.NamingConvention.camelCase.setAsDefault();

    var manager = new breeze.EntityManager(serviceName);
    manager.enableSaveQueuing(true);

    var service = {
        createEntity: createEntity,
        queryEntity: queryEntity,
        hasChanges: hasChanges,
        saveChanges: saveChanges,
        rejectChanges: rejectChanges,
        deleteEntityAndSave: deleteEntityAndSave,
        showLocalStateDump: showLocalStateDump
    };
    return service;

    //-----------------------------------------------------------------------------------------------------------------

    function createEntity(entityName, initialValues) {
        var entity = manager.createEntity(entityName, initialValues);
        manager.addEntity(entity);
        return entity;
    }

    //-----------------------------------------------------------------------------------------------------------------

    function queryEntity(entityName, queryBuilderCallback) {
        var query = breeze.EntityQuery.from(entityName);

        if (queryBuilderCallback) {
            query = queryBuilderCallback(query);
        }

        var promise = manager.executeQuery(query).catch(queryFailed);
        return promise;

        function queryFailed(error) {
            logger.error(error.message, "Query failed");
            return $q.reject(error); // so downstream promise users know it failed
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    function deleteEntityAndSave(entity) {
        if (entity) {
            var aspect = entity.entityAspect;
            if (aspect.isBeingSaved && aspect.entityState.isAdded()) {
                // wait to delete added entity while it is being saved  
                setTimeout(function () { deleteEntityAndSave(entity); }, 100);
                return;
            }
            aspect.setDeleted();
            saveChanges();
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    function hasChanges() {
        return manager.hasChanges();
    }

    //-----------------------------------------------------------------------------------------------------------------

    function handleSaveValidationError(error) {
        var message = "Not saved due to validation error";
        try { // fish out the first error
            var firstErr = error.entityErrors[0];
            message += ": " + firstErr.errorMessage;
        } catch (e) { /* eat it for now */ }
        return message;
    }

    //-----------------------------------------------------------------------------------------------------------------

    function saveChanges() {
        return manager.saveChanges()
            .then(saveSucceeded)
            .catch(saveFailed);

        function saveSucceeded(saveResult) {
            logger.success("# of entities saved = " + saveResult.entities.length);
            logger.log(saveResult);
        }

        function saveFailed(error) {
            var reason = error.message;
            var detail = error.detail;

            if (error.entityErrors) {
                reason = handleSaveValidationError(error);
            } else if (detail && detail.ExceptionType &&
                detail.ExceptionType.indexOf('OptimisticConcurrencyException') !== -1) {
                // Concurrency error 
                reason =
                    "Another user, perhaps the server, " +
                    "may have deleted one or all of the entities." +
                    " You may have to restart the app.";
            } else {
                reason = "Failed to save changes: " + reason +
                    " You may have to restart the app.";
            }

            logger.error(error, reason);
            return $q.reject(error); // so downstream promise users know it failed
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    function rejectChanges() {
        return manager.rejectChanges();
    }

    //-----------------------------------------------------------------------------------------------------------------

    function showLocalStateDump(elementId) {
        var exportedData = JSON.parse(manager.exportEntities());
        exportedData.metadataStore = JSON.parse(exportedData.metadataStore);
        document.getElementById(elementId).innerHTML = JSON.stringify(exportedData, undefined, 2);
    }

}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.factory('logger', ['$log', function ($log) {

    // This logger wraps the toastr logger and also logs to console using ng $log
    // toastr.js is library by John Papa that shows messages in pop up toast.
    // https://github.com/CodeSeven/toastr

    toastr.options.timeOut = 2000; // 2 second toast timeout
    toastr.options.positionClass = 'toast-bottom-right';

    var logger = {
        error: error,
        info: info,
        log: log,  // straight to console; bypass toast
        success: success,
        warning: warning
    };

    return logger;

    function error(message, title) {
        toastr.error(message, title);
        $log.error("Error: " + message);
    }

    function info(message, title) {
        toastr.info(message, title);
        $log.info("Info: " + message);
    }

    function log(message) {
        $log.log(message);
    }

    function success(message, title) {
        toastr.success(message, title);
        $log.info("Success: " + message);
    }

    function warning(message, title) {
        toastr.warning(message, title);
        $log.warn("Warning: " + message);
    }

}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.controller('appStart',
['$http', '$scope', '$rootScope', 'uidlService', 'entityService', 'commandService',
function ($http, $scope, $rootScope, uidlService, entityService, commandService) {

    $scope.pageTitle = 'LOADING . . .';

    $http.get('uidl.json').then(function (httpResult) {
        uidlService.setDocument(httpResult.data);

        $rootScope.app = uidlService.getApp();
        $rootScope.uidl = uidlService.getApp();
        $rootScope.entityService = entityService;
        $rootScope.uidlService = uidlService;
        $rootScope.commandService = commandService;
        $rootScope.appScope = $scope;

        uidlService.implementController($scope);

        $rootScope.currentScreen = uidlService.getCurrentScreen();
        $rootScope.currentLocale = uidlService.getCurrentLocale();
        $scope.pageTitle = $scope.translate($scope.app.text) + ' - ' + $scope.translate($scope.currentScreen.text);

        commandService.startPollingMessages();
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

theApp.directive('uidlScreen', ['uidlService', 'entityService', function (uidlService, entityService) {
    return {
        scope: {
            uidl: '='
        },
        restrict: 'E',
        replace: true,
        link: function (scope, elem, attrs) {
            //console.log('uidlScreen::link', scope.uidl.qualifiedName);
            //uidlService.implementController(scope);
        },
        template: '<ng-include src="\'uidl-screen\'"></ng-include>',
        controller: function ($scope) {
            $scope.uidlService = uidlService;
            $scope.entityService = entityService;
            //console.log('uidlScreen::controller', $scope.uidl.qualifiedName);
            //uidlService.implementController($scope);
            $scope.$watch('uidl', function (newValue, oldValue) {
                console.log('uidlScreen::watch(uidl)', oldValue.qualifiedName, '->', $scope.uidl.qualifiedName);
                uidlService.implementController($scope);
            });
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlScreenPart', ['uidlService', 'entityService', function (uidlService, entityService) {
    return {
        scope: {
            uidl: '='
        },
        restrict: 'E',
        replace: true,
        link: function (scope, elem, attrs) {
            //console.log('uidlScreenPart::link', scope.uidl.qualifiedName);
            //uidlService.implementController(scope);
        },
        template: '<ng-include src="\'uidl-screen-part\'"></ng-include>',
        controller: function ($scope) {
            $scope.uidlService = uidlService;
            $scope.entityService = entityService;
            //console.log('uidlScreenPart::controller', $scope.uidl.qualifiedName);
            //uidlService.implementController($scope);
            $scope.$watch('uidl', function (newValue, oldValue) {
                console.log('uidlScreenPart::watch(uidl)', oldValue.qualifiedName, '->', $scope.uidl.qualifiedName);
                uidlService.implementController($scope);
            });
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlWidget', ['uidlService', 'entityService', function (uidlService, entityService) {
    return {
        scope: {
            uidl: '=',
            parentUidl: '=',
            parentModel: '='
        },
        restrict: 'E',
        replace: true,
        link: function (scope, elem, attrs) {
            //console.log('uidlWidget::link', scope.uidl.qualifiedName);
            //uidlService.implementController(scope);
        },
        template: '<ng-include src="\'uidl-element-template-\' + uidl.templateName"></ng-include>',
        controller: function ($scope) {
            $scope.uidlService = uidlService;
            $scope.entityService = entityService;
            //console.log('uidlWidget::controller', $scope.uidl.qualifiedName);
            //uidlService.implementController($scope);
            $scope.$watch('uidl', function (newValue, oldValue) {
                console.log('uidlWidget::watch(uidl)', oldValue ? oldValue.qualifiedName : '0', '->', $scope.uidl ? $scope.uidl.qualifiedName : '0');
                uidlService.implementController($scope);
            });
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlUserAlertInline', ['uidlService', 'entityService', function (uidlService, entityService) {
    return {
        scope: {
            alert: '='
        },
        restrict: 'E',
        replace: false,
        templateUrl: 'uidl-user-alert-inline',
        link: function (scope, elem, attrs) {
            //console.log('uidlUserAlertInline::link');
            //uidlService.implementController(scope);
        },
        controller: function ($scope) {
            $scope.uidlService = uidlService;
            $scope.entityService = entityService;
            //console.log('uidlUserAlertInline::controller');
            uidlService.implementController($scope);
        },
    }
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlController', ['$compile', '$parse', function ($compile, $parse) {
    return {
        scope: true,
        restrict: 'A',
        terminal: true,
        priority: 100000,
        link: function (scope, elem, attrs) {
            elem.attr('ng-controller', scope.uidl.qualifiedName);
            elem.removeAttr('uidl-controller');
            $compile(elem)(scope);
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.filter('localized', ['$scope', function ($scope) {
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

//---------------------------------------------------------------------------------------------------------------------

theApp.filter('reverse', function () {
    return function (items) {
        return items.slice().reverse();
    };
});
