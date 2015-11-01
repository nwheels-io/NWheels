'use strict';

var theApp = angular.module('theApp', []);

//-----------------------------------------------------------------------------------------------------------------

function toCamelCase(s) {
    return s.charAt(0).toLowerCase() + s.slice(1);
}

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

	function getRelatedMetaType(entityName, propertyName) {
        var fromMetaType = m_uidl.metaTypes[toCamelCase(entityName)];
		var fromMetaProperty = fromMetaType.properties[toCamelCase(propertyName)];
		
		if (fromMetaProperty.relation && fromMetaProperty.relation.relatedPartyMetaTypeName) {
			return m_uidl.metaTypes[toCamelCase(fromMetaProperty.relation.relatedPartyMetaTypeName)];
		}
		else {
			return null;
		}
	};
	
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
			if(window.appInit)
						window.appInit();
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['Report'] = {
        implement: function (scope) {
            var metaType = scope.uidlService.getMetaType(scope.uidl.entityName);

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

			scope.getPropertyRelatedMetaType = function(propertyName) {
			    return scope.uidlService.getRelatedMetaType(scope.uidl.entityName, propertyName);
			};

            scope.queryEntities();
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

	m_controllerImplementations['Chart'] = {
        implement: function(scope) {
			$timeout(function() {
				scope.$emit(scope.uidl.qualifiedName + ':RequestingData');
			});
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['Crud'] = {
        implement: function (scope) {
            var metaType = scope.uidlService.getMetaType(scope.uidl.entityName);

            scope.metaType = metaType;
            
            if (!scope.uidl.displayColumns || !scope.uidl.displayColumns.length) {
                scope.uidl.displayColumns = scope.uidl.defaultDisplayColumns;
            }
            
            scope.displayProperties = Enumerable.From(scope.uidl.displayColumns).Select(function (name) {
                return metaType.properties[toCamelCase(name)];
            }).ToArray();

            scope.refresh = function () {
                scope.queryEntities();
                scope.uiShowCrudForm = false;
            };

            scope.queryEntities = function () {
                scope.resultSet = null;
                if (scope.uidl.mode !== 'Inline') {
                    scope.entityService.queryEntity(scope.uidl.entityName).then(function (data) {
                        scope.resultSet = data.ResultSet;
                    });
                } else {
                    scope.resultSet = scope.parentModel.entity[scope.parentUidl.propertyName];
                }
            };

            scope.editEntity = function (entity) {
                scope.model.entity = entity;
                scope.model.isNew = false;
                scope.uiShowCrudForm = true;
            };

            scope.newEntity = function () {
                scope.model.entity = scope.entityService.newDomainObject(metaType.name);
                scope.model.isNew = true;
                scope.uiShowCrudForm = true;
            };

            scope.deleteEntity = function (entity) {
                if (scope.uidl.mode !== 'Inline') {
                    scope.entityService.deleteEntity(entity).then(function(result) {
                        scope.queryEntities();
                        scope.uiShowCrudForm = false;
                    });
                } else {
                    for (var i = 0; i < scope.resultSet.length; i++) {
                        if(scope.resultSet[i] === entity) {
                           scope.resultSet.splice(i, 1);
                           break;
                        }
                    }                
                }
                scope.refresh();
            };

            scope.saveChanges = function (entity) {
                if (scope.uidl.mode !== 'Inline') {
                    scope.entityService.storeEntity(entity);
                } else if (scope.model.isNew === true) {
                    scope.ResultSet.push(entity._backingStore);
                }
                scope.refresh();
            };

            scope.rejectChanges = function (entity) {
                scope.refresh();
            };
            
            if (scope.uidl.form) {
                subscribeToFormNotifiations(scope.uidl.form);
            }

            if (scope.uidl.formTypeSelector) {
                for (var i = 0; i < scope.uidl.formTypeSelector.selections.length; i++) {
                    var selection = scope.uidl.formTypeSelector.selections[i];
                    subscribeToFormNotifiations(selection.widget);
                }
            }

            scope.queryEntities();
            
            function subscribeToFormNotifiations(form) {
                scope.$on(form.qualifiedName + ':Closing', function (event) {
                    scope.refresh();
                });
                scope.$on(form.qualifiedName + ':Saving', function (event) {
                    scope.saveChanges(scope.model.entity);
                });
                scope.$on(form.qualifiedName + ':Rejecting', function (event) {
                    scope.rejectChanges(scope.model.entity);
                });
                scope.$on(form.qualifiedName + ':Deleting', function (event) {
                    scope.deleteEntity(scope.model.entity);
                });
            }
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['CrudForm'] = {
        implement: function (scope) {
            scope.metaType = scope.uidlService.getMetaType(scope.uidl.entityName);

            scope.tabSetIndex = 0;
            scope.plainFields = Enumerable.From(scope.uidl.fields).Where("$.modifiers!='Tab' && $.modifiers!='Section'").ToArray();
            scope.sectionFields = Enumerable.From(scope.uidl.fields).Where("$.modifiers=='Section'").ToArray();
            scope.tabSetFields = Enumerable.From(scope.uidl.fields).Where("$.modifiers=='Tab'").ToArray();

            scope.saveChanges = function () {
                scope.$emit(scope.uidl.qualifiedName + ':Saving');
            };

            scope.rejectChanges = function () {
                scope.$emit(scope.uidl.qualifiedName + ':Rejecting');
            };
			
			if (scope.uidl.mode === 'StandaloneCreate') {
				scope.parentModel = {
				    entity: scope.entityService.newDomainObject(scope.metaType.name)
				};
			}
			
			scope.executeSearch = function () {
				scope.entityService.queryEntity(scope.uidl.entityName, function(query) {
					// build query
					return query;
				}).then(function (data) {
					scope.$emit(scope.uidl.qualifiedName + ':SearchResultsReceived', data);	
                });
			};

            scope.selectTab = function(index) {
                scope.tabSetIndex = index;
            };
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['TypeSelector'] = {
        implement: function (scope) {
			if (scope.parentModel) {
				scope.model = { 
					entity: (
						scope.parentUidl ? // parentUidl exists when nested in CrudForm
						scope.parentModel.entity[scope.parentUidl.propertyName] :   // when nested in CrudForm widget
						scope.parentModel.entity)                                   // when nested in Crud widget
				};
			} else if(scope.uidl.selections.length > 0) {
				scope.model = { 
					entity: scope.entityService.newDomainObject(scope.uidl.selections[0].typeName)
				};
			}
			
			if (scope.model) {
				scope.selectedType = scope.model.entity['$type'];
			}
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    return {
        setDocument: setDocument,
        getApp: getApp,
        getCurrentScreen: getCurrentScreen,
        getCurrentLocale: getCurrentLocale,
        getMetaType: getMetaType,
        getRelatedMetaType: getRelatedMetaType,
        //takeMessagesFromServer: takeMessagesFromServer,
        implementController: implementController,
    };

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

		//commandService.startPollingMessages();
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

theApp.directive('uidlWidget', ['uidlService', 'entityService', '$timeout', function (uidlService, entityService, $timeout) {
    return {
        scope: {
            uidl: '=',
            parentUidl: '=',
            parentModel: '=?'
        },
        restrict: 'E',
        replace: true,
        link: function (scope, elem, attrs) {
            //console.log('uidlWidget::link', scope.uidl.qualifiedName);
            //uidlService.implementController(scope);
        },
        template: '<ng-include src="\'uidl-element-template/\' + uidl.templateName"></ng-include>',
        controller: function ($scope) {
            $scope.uidlService = uidlService;
            $scope.entityService = entityService;
            //console.log('uidlWidget::controller', $scope.uidl.qualifiedName);
            //uidlService.implementController($scope);
            $scope.$watch('uidl', function (newValue, oldValue) {
                console.log('uidlWidget::watch(uidl)', oldValue ? oldValue.qualifiedName : '0', '->', $scope.uidl ? $scope.uidl.qualifiedName : '0');

                if ($scope.uidl) {
                    uidlService.implementController($scope);

                    $timeout(function() {
                        var initFuncName = 'initWidget_' + $scope.uidl.widgetType;
                        var initFunc = window[initFuncName];
                        if (typeof initFunc === 'function') {
                            initFunc($scope.uidl);
                        }
                    });
                }
            });
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlGridField', ['uidlService', 'entityService', function (uidlService, entityService) {
    return {
        scope: {
            property: '=',
            value: '='
        },
        restrict: 'E',
        replace: true,
        link: function (scope, elem, attrs) {
        },
        templateUrl: 'uidl-element-template/GridField',
        controller: function ($scope) {
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlFormField', ['uidlService', 'entityService', function (uidlService, entityService) {
    return {
        scope: {
            uidl: '=',
            entity: '='
        },
        restrict: 'E',
        replace: true,
        link: function (scope, elem, attrs) {
            //console.log('uidlWidget::link', scope.uidl.qualifiedName);
            //uidlService.implementController(scope);
        },
        templateUrl: 'uidl-element-template/FormField', 
        controller: function ($scope) {
            $scope.uidlService = uidlService;
            $scope.entityService = entityService;
            
            if ($scope.uidl.fieldType==='Lookup') {
                var metaType = uidlService.getMetaType($scope.uidl.lookupEntityName);

                $scope.lookupMetaType = metaType;
                $scope.lookupValueProperty = ($scope.uidl.lookupValueProperty ? $scope.uidl.lookupValueProperty : metaType.primaryKey.propertyNames[0]);
                $scope.lookupTextProperty = ($scope.uidl.lookupDisplayProperty ? $scope.uidl.lookupDisplayProperty : metaType.defaultDisplayPropertyNames[0]);
                $scope.lookupForeignKeyProperty = $scope.uidl.propertyName + '_FK';
             
                $scope.entityService.queryEntity($scope.uidl.lookupEntityName).then(function (data) {
                    $scope.lookupResultSet = data.results;
					
					if ($scope.uidl.applyDistinctToLookup) {
						$scope.lookupResultSet = Enumerable.From($scope.lookupResultSet).Distinct('$.' + $scope.lookupTextProperty).ToArray();
					}
                });
            }
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

theApp.directive('uidlReportLookup', ['entityService', function(entityService) {
	return {
		scope: {
			entityMetaType: '='
		},
		templateUrl: 'uidl-element-template-report-lookup',
		controller: function($scope) {
			if ($scope.entityMetaType == null)
				return;
			
			entityService.queryEntity($scope.entityMetaType.name).then(function (data) {
				$scope.resultSet = data.results;
			});
		}
	}
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
