'use strict';

var theApp = angular.module('theApp', ['ngSanitize']);

//-----------------------------------------------------------------------------------------------------------------

function toCamelCase(s) {
    return s.charAt(0).toLowerCase() + s.slice(1);
}

//-----------------------------------------------------------------------------------------------------------------

function getQueryStringValue(name) {
    var query = window.location.search.substring(1);
    var vars = query.split("&");
    for (var i=0;i<vars.length;i++) {
        var pair = vars[i].split("=");
        if(pair[0] == name){
            return pair[1];
        }
    }
    return(false);
}

//-----------------------------------------------------------------------------------------------------------------

function hasEnumFlag(value, flag) {
	if (value) {
		var members = value.split(',');
		for(var i = 0; i < members.length; i++)
		{
			if (members[i].trim() === flag) {
				return true;
			}
		}
	}

	return false;
};

//-----------------------------------------------------------------------------------------------------------------

function cleanDom(node) {
    for(var n = 0; n < node.childNodes.length; n ++) {
        var child = node.childNodes[n];
        
        if (child.nodeType === 8 || (child.nodeType === 3 && !/\S/.test(child.nodeValue))) {
            node.removeChild(child);
            n--;
        } else if (child.nodeType === 1) {
            cleanDom(child);
        }
    }
}

//-----------------------------------------------------------------------------------------------------------------

Number.prototype.formatMoney = function(c, d, t) {
    var n = this, 
        c = isNaN(c = Math.abs(c)) ? 2 : c, 
        d = d == undefined ? "." : d, 
        t = t == undefined ? "," : t, 
        s = n < 0 ? "-" : "", 
        i = parseInt(n = Math.abs(+n || 0).toFixed(c)) + "", 
        j = (j = i.length) > 3 ? j % 3 : 0;
    return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
};

//-----------------------------------------------------------------------------------------------------------------

theApp.factory('appHttpInterceptor', ['$rootScope', '$q', 'sessionService', function ($rootScope, $q, sessionService) {
    return {
        'response': function(response) {
            sessionService.slideExpiry();
            return response;
        },
        'responseError': function(rejection) {
            if (rejection.status===0) {
                sessionService.deactivateExpiry();
                $rootScope.$broadcast($rootScope.app.qualifiedName + ':ServerConnectionLost');
            } else {
                sessionService.slideExpiry();
            }
            return $q.reject(rejection);
        }
    };
}]);

//-----------------------------------------------------------------------------------------------------------------

theApp.config(['$httpProvider', function($httpProvider) {
    $httpProvider.interceptors.push('appHttpInterceptor');
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.service('commandService',
['$http', '$q', '$interval', '$timeout', '$rootScope', 'sessionService',
function ($http, $q, $interval, $timeout, $rootScope, sessionService) {

    var m_pendingCommands = {};
    var m_pollTimer = null;
	var m_sessionService = sessionService;

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
                var faultInfo = createFaultInfo(response);
                commandCompletion.reject(faultInfo);
            }
        );

        return commandCompletion.promise;
    }

    //-----------------------------------------------------------------------------------------------------------------

    function receiveMessages() {
        $http.post('app/takeMessages').then(
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
    
    function createFaultInfo(httpResponse) {
		var faultInfo = null;
		
        if (httpResponse.data && httpResponse.data.faultCode) {
            faultInfo = httpResponse.data;
        } else {
			faultInfo = {
				success: false,
				Success: false,
				faultCode: httpResponse.status,
				FaultCode: httpResponse.status,
				faultReason: httpResponse.statusText,
				FaultReason: httpResponse.statusText,
				technicalInfo: httpResponse.data,
				TechnicalInfo: httpResponse.data
			};
		}
		
		if (faultInfo.faultType === 'AuthorizationFault' && faultInfo.faultCode === 'AccessDenied' && faultInfo.faultSubCode === 'NotAuthenticated') {
			m_sessionService.notifySessionExpired();
		}
		
        return faultInfo;
    }

    //-----------------------------------------------------------------------------------------------------------------

    return {
        sendCommand: sendCommand,
        receiveMessages: receiveMessages,
        startPollingMessages: startPollingMessages,
        stopPollingMessages: stopPollingMessages,
        createFaultInfo: createFaultInfo
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.service('sessionService',
['$timeout', '$rootScope',
function ($timeout, $rootScope) {

    var m_sessionTimeout = null;
    var m_expiryMilliseconds = -1;
    var m_expiredNotificationId = null;
    
    //-----------------------------------------------------------------------------------------------------------------

    function activateExpiry(expiryMilliseconds, expiredNotificationId) {
        if (m_sessionTimeout) {
            $timeout.cancel(m_sessionTimeout);
        }
        m_expiryMilliseconds = expiryMilliseconds;
        m_expiredNotificationId = expiredNotificationId;
        m_sessionTimeout = $timeout(notifySessionExpired, expiryMilliseconds);
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    function slideExpiry() {
        if (m_sessionTimeout) {
            $timeout.cancel(m_sessionTimeout);
            m_sessionTimeout = $timeout(notifySessionExpired, m_expiryMilliseconds);
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    function deactivateExpiry() {
        if (m_sessionTimeout) {
            $timeout.cancel(m_sessionTimeout);
        }
        m_sessionTimeout = null;
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    function notifySessionExpired() {
        deactivateExpiry();
        $rootScope.$broadcast(m_expiredNotificationId);
    }

    //-----------------------------------------------------------------------------------------------------------------

    return {
        activateExpiry: activateExpiry,
        slideExpiry: slideExpiry,
        deactivateExpiry: deactivateExpiry,
		notifySessionExpired: notifySessionExpired
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.service('uidlService',
['$q', '$http', '$rootScope', '$timeout', '$location', '$templateCache', 'commandService', 'sessionService',
function ($q, $http, $rootScope, $timeout, $location, $templateCache, commandService, sessionService) {

    var m_uidl = null;
    var m_app = null;
    var m_index = {
        screens: {},
        screenParts: {},
    };
    var m_currentScreen = null;
    var m_initialScreenInput = null;
    var m_behaviorImplementations = {};
    var m_dataBindingImplementations = {};
    var m_controllerImplementations = {};

    var m_locationSearch = null;
    
    //var m_pendingCommands = { };

    //-----------------------------------------------------------------------------------------------------------------

    function getLocationSearchRaw() {
        if (!m_locationSearch) {
            var vars = { }, hash;
            var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
            for(var i = 0; i < hashes.length; i++)
            {
                hash = hashes[i].split('=');
                vars[hash[0]] = hash[1];
            }
            m_locationSearch = vars;
        }
        
        return m_locationSearch;
    }
    
    //-----------------------------------------------------------------------------------------------------------------

    function setDocument(uidlDocument) {
        m_uidl = uidlDocument;
        m_app = m_uidl.applications[0];

        for (var i = 0; i < m_app.screens.length; i++) {
            m_index.screens[m_app.screens[i].qualifiedName] = m_app.screens[i];
        }

        if (m_app.screenParts) {
            for (var i = 0; i < m_app.screenParts.length; i++) {
                m_index.screenParts[m_app.screenParts[i].qualifiedName] = m_app.screenParts[i];
            }
        }

        var screenQueryValue = $location.search().$screen;
        
        if (screenQueryValue) {
            m_currentScreen = m_index.screens[screenQueryValue];
            
            if (!$location.search().$sticky) {
                $location.search({ });
            }
        } else {
            m_currentScreen = m_index.screens[m_app.initialScreenQualifiedName];
        }
        
        m_initialScreenInput = { };
        
        var rawQueryString = getLocationSearchRaw();
        for (var p in rawQueryString){
            if (rawQueryString.hasOwnProperty(p) && p.length > 0 && p.charAt(0) != '$') {
                m_initialScreenInput[p] = rawQueryString[p];
            }
        }
        var queryString = $location.search();
        for (var p in queryString){
            if (queryString.hasOwnProperty(p) && p.length > 0 && p.charAt(0) != '$') {
                //m_initialScreenInput[p.charAt(0).toUpperCase() + p.slice(1)] = queryString[p];
                m_initialScreenInput[p] = queryString[p];
            }
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    function beginGetScreenPartUidl(qualifiedName) {
        var elementName = qualifiedName.replace(':','-').replace('<', '').replace('>', '');
        var uidl = m_index.screenParts[qualifiedName];
        
        if (uidl) {
            return $q.when(uidl);
        }
        
        return $http.get('app/uidl.json/ScreenPart/' + encodeURIComponent(elementName), { cache: true }).then(
            function (response) {
                return response.data;
            },
            function (error) {
                return $q.reject(error);
            }
        );
    }

    //-----------------------------------------------------------------------------------------------------------------

    function translate(stringId, options) {
        if (!stringId) {
            return '';
        }

        var locale = getCurrentLocale();
        
        if (!locale) {
            return stringId;
        }
        
        var localizedString = locale.translations['##' + stringId] || stringId; //TODO: implement UI-location-specific lookup
        
        if (options && options.upperCase === true) {
            localizedString = localizedString.toUpperCase();
        }
        
        return localizedString;
    }

    //-----------------------------------------------------------------------------------------------------------------

    function formatValue(format, value) {
        if (!format || value=='0') {
            return value;
        }
        switch (format) {
            case 'd':
                return moment.utc(value, 'YYYY-MM-DD HH:mm:ss').format('DD MMM YYYY');
            case 'y':
                return moment.utc(value, 'YYYY-MM-DD HH:mm:ss').format('MMMM YYYY');
            case 'd':
                return moment.utc(value, 'YYYY-MM-DD HH:mm:ss').format('L');
            case 'D':
                return moment.utc(value, 'YYYY-MM-DD HH:mm:ss').format('LL');
            case 'c':
                return (value ? '$' + parseFloat(value).formatMoney(2, '.', ',') : '');
            case '#,##0':
                return (value ? parseFloat(value).formatMoney(0, '.', ',') : '');
            case '0.00':
            case '#,##0.00':
                return (value ? parseFloat(value).formatMoney(2, '.', ',') : '');
            case '0.0000':
            case '#,##0.0000':
                return (value ? parseFloat(value).formatMoney(4, '.', ',') : '');
            case '0.00000':
            case '#,##0.00000':
                return (value ? parseFloat(value).formatMoney(5, '.', ',') : '');
        }
        return value;
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

        if (implResult === undefined) {
            implResult = input;
        }
        
        if (impl.returnsPromise) {
            promise = implResult;
        }
        else {
            promise = $q(function (resolve, reject) {
                resolve(implResult);
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
    
    function implementDataBinding(scope, binding) {
        var impl = m_dataBindingImplementations[binding.sourceType];
        impl.execute(scope, binding);
    }

    //-----------------------------------------------------------------------------------------------------------------

    function implementController(scope) {
        scope.translate = translate;
        scope.formatValue = formatValue;
        scope.hasEnumFlag = hasEnumFlag;
        scope.appScope = $rootScope.appScope;
		if (!scope.model) {
			scope.model = {
				Data: {},
				State: {}
			};
		}
        if (scope.appScope.model) {
            scope.model.appState = scope.appScope.model.State;
        }

        if (scope.uidl) {
            console.log('uidlService::implementController', scope.uidl.qualifiedName);

            if (scope.uidl.widgetType && m_controllerImplementations[scope.uidl.widgetType]) {
                m_controllerImplementations[scope.uidl.widgetType].implement(scope);
            }

            for (var i = 0; i < scope.uidl.behaviors.length; i++) {
                var behavior = scope.uidl.behaviors[i];
                if (behavior.subscription) {
                    implementSubscription(scope, behavior);
                }
            }

            for (var i = 0; i < scope.uidl.dataBindings.length; i++) {
                implementDataBinding(scope, scope.uidl.dataBindings[i]);
            }
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    function getDocument() {
        return m_uidl;
    }
    
    //-----------------------------------------------------------------------------------------------------------------

    function getApp() {
        return m_app;
    }

    //-----------------------------------------------------------------------------------------------------------------

    function getCurrentScreen() {
        return m_currentScreen;
    }

    //-----------------------------------------------------------------------------------------------------------------

    function getInitialScreenInput() {
        return m_initialScreenInput;
    }
    
    //-----------------------------------------------------------------------------------------------------------------

    function getCurrentLocale() {
        return m_uidl.locales[m_uidl.currentLocaleIdName];
    }

    //-----------------------------------------------------------------------------------------------------------------

    function getMetaType(name) {
        return m_uidl.metaTypes[toCamelCase(name)];
    }

    //-----------------------------------------------------------------------------------------------------------------

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

    function loadTemplateById(templateId) {
        var fullTemplateId = 'app/uidl-element-template/' + templateId;
        var template = $templateCache.get(fullTemplateId);

        if (template) {
            return $q.when(template);
        } else {
            var deferred = $q.defer();

            $http.get(fullTemplateId, { cache: true }).then(
                function (response) {
                    $templateCache.put(fullTemplateId, response.data);
                    deferred.resolve(response.data);
                },
                function (error) {
                    deferred.reject(error);
                }
            );

            return deferred.promise;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    function selectValue(context, expression) {
        return Enumerable.Return(context).Select('ctx=>ctx.' + expression).Single();
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    function createInputContext(scopeModel, input) {
        var inputContext;
		if (scopeModel) {
			inputContext = {
				model: {
					Data: scopeModel.Data,
					State: scopeModel.State,
					Input: input || scopeModel.Input,
					appState: scopeModel.appState
				}
			};
		} else {
			inputContext = {
				model: {
					Data: { },
					State: { },
					Input: input || { },
					appState: { }
				}
			};
		}
        return inputContext;
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    function writeValueByNavigationPath(obj, navigations, value) {
        var target = obj;
        for (var j = 0; j < navigations.length - 1; j++) {
            var nextTarget = target[navigations[j]];
            if (!nextTarget) {
                nextTarget = target[navigations[j]] = { };
            }
            target = nextTarget;
        }
        var lastNavigation = navigations[navigations.length-1];
        target[lastNavigation] = value;
    }

    //-----------------------------------------------------------------------------------------------------------------

    function readValueByNavigationPath(obj, navigations) {
        var target = obj;
        for (var j = 0; j < navigations.length - 1; j++) {
            var nextTarget = target[navigations[j]];
            if (!nextTarget) {
                return null;
            }
            target = nextTarget;
        }
        var lastNavigation = navigations[navigations.length-1];
        return target[lastNavigation];
    }
    
    //-----------------------------------------------------------------------------------------------------------------

    function performModelAlterations(model, alterations) {
        var context = {
            model: model
        };
        for (var i = 0; i < alterations.length; i++) {
            var alteration = alterations[i];
            switch (alteration.type) {
                case 'Copy':
                    var value = null;
                    if (alteration.sourceValue) {
                        value = alteration.sourceValue;
                    } else if (alteration.sourceExpression && alteration.sourceExpression!=='null')                         {
                        value = Enumerable.Return(context).Select('ctx=>ctx.' + alteration.sourceExpression).Single();
                    }
                    var target = context;
                    for (var j = 0; j < alteration.destinationNavigations.length - 1; j++) {
                        var nextTarget = target[alteration.destinationNavigations[j]];
                        if (!nextTarget) {
                            nextTarget = target[alteration.destinationNavigations[j]] = { };
                        }
                        target = nextTarget;
                    }
                    var lastNavigation = alteration.destinationNavigations[alteration.destinationNavigations.length-1];
                    target[lastNavigation] = value;
                    break;
            }
        }
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
            
            if (input && behavior.inputExpression) {
                var inputContext = createInputContext(scope.model, input);
                input = selectValue(inputContext, behavior.inputExpression);
            }
            
            if (behavior.navigationType == 'Popup' && behavior.targetType == 'Screen') {
                var url = 
                    $location.protocol() + '://' + 
                    $location.host() + ':' + $location.port() + 
                    '/#/?$sticky=1&$screen=' + behavior.targetQualifiedName;
                
                var h = parseInt(screen.height * 0.8);
                var w = parseInt(screen.width * 0.9);
                var x = parseInt((screen.width - w) / 2);
                var y = parseInt((screen.height - h) / 4);
                var windowOptions = 
                    'toolbar=no,titlebar=no,menubar=no,scrollbars=yes,resizable=yes,' +
                    'top=' + y + ',left=' + x + ',width=' + w + ',height=' + h;
                    
                window.open(url, '_blank', windowOptions);
                return;
            }
            
            switch (behavior.targetType) {
                case 'Screen':
                    var screenQualifiedName = null;
                    if (behavior.targetQualifiedNameExpression) {
                        var screenNameContext = createInputContext(scope.model, input);
                        screenQualifiedName = selectValue(screenNameContext, behavior.targetQualifiedNameExpression);
                    } else {
                        screenQualifiedName = behavior.targetQualifiedName;
                    }
                
                    var screenUidl = m_index.screens[screenQualifiedName];
                    $rootScope.currentScreen = null;
                    $timeout(function() {
                        $rootScope.currentScreen = screenUidl;
                        location.hash = screenUidl.qualifiedName;
                        $timeout(function() {
                            //if (oldScreen) {
                            //    $rootScope.$broadcast(oldScreen.qualifiedName + ':NavigatingAway', input);
                            //}
                            $rootScope.$broadcast(screenUidl.qualifiedName + ':NavigatedHere', input || getInitialScreenInput());
                        });
                    });
                    break;
                case 'ScreenPart':
                    beginGetScreenPartUidl(behavior.targetQualifiedName).then(function(screenPart) {
                        $rootScope.$broadcast(behavior.targetContainerQualifiedName + ':NavReq', {
                            screenPart: screenPart,
                            input: input
                        });
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
            var payload = input;
            if (behavior.argumentExpression) {
                var context = { };
                if (input) {
                    context.model = {
                        Data: scope.model.Data,
                        State: scope.model.State,
                        Input: input
                    };
                } else {
                    context.model = scope.model;
                }
                payload = Enumerable.Return(context).Select('ctx=>ctx.' + behavior.argumentExpression).Single();
            }
            if (scope.$parent) {
                scope.$parent.$emit(behavior.commandQualifiedName + ':Executing', payload);
            }
            scope.$broadcast(behavior.commandQualifiedName + ':Executing', payload);
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['Broadcast'] = {
        returnsPromise: false,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > broadcast', behavior.notificationQualifiedName, behavior.direction, input);
            var payload = input;
            if (behavior.payloadExpression) {
                var context = { };
                if (input) {
                    context.model = {
                        Data: scope.model.Data,
                        State: scope.model.State,
                        Input: input
                    };
                } else {
                    context.model = scope.model;
                }
                payload = Enumerable.Return(context).Select('ctx=>ctx.' + behavior.payloadExpression).Single();
            }
            if (behavior.direction.indexOf('BubbleUp') > -1) {
                scope.$emit(behavior.notificationQualifiedName, payload);
            }
            if (behavior.direction.indexOf('TunnelDown') > -1) {
                scope.$broadcast(behavior.notificationQualifiedName, payload);
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
                Data: scope.model.Data,
                State: scope.model.State,
                Input: input
            };
            for (var i = 0; i < behavior.parameterNames.length; i++) {
                var parameterValue = (
                    behavior.parameterExpressions[i] && behavior.parameterExpressions[i].length > 0 ?
                    Enumerable.Return(parameterContext).Select('ctx=>ctx.' + behavior.parameterExpressions[i]).Single() :
                    null);
                requestData[behavior.parameterNames[i]] = parameterValue;
            }
            var entityNameRequired = (behavior.callResultType==='EntityQuery' || behavior.callResultType==='EntityQueryExport');
            var requestPath = 
                'app/api' + 
                '/' + behavior.callType +
                '/' + behavior.callResultType +
                (entityNameRequired ? '/' + behavior.entityName : '') +
                '/' + behavior.callTargetType + 
                '/' + behavior.contractName + 
                '/' + behavior.operationName +
                (behavior.entityName && !entityNameRequired ? '/' + behavior.entityName : '');
                
            if (behavior.callResultType === 'EntityQueryExport') {
                requestPath += '/' + behavior.exportFormatId;
            }
            if (behavior.callTargetType === 'EntityMethod') {
                requestPath += '?$entityId=' + encodeURIComponent(scope.model.State.Entity['$id']);
            }
            if (behavior.querySelectList || behavior.queryIncludeList) {
                var queryBuilder = new EntityQueryBuilder(behavior.entityName, requestPath);
                if (behavior.querySelectList) {
                    for (var i = 0; i < behavior.querySelectList.length; i++) {
                        queryBuilder.select(behavior.querySelectList[i]);
                    }
                }
                if (behavior.queryIncludeList) {
                    for (var i = 0; i < behavior.queryIncludeList.length; i++) {
                        queryBuilder.include(behavior.queryIncludeList[i]);
                    }
                }
                requestPath += queryBuilder.getQueryString();
            }
            if (behavior.prepareOnly===true) {
                return $q.when({
                    url: requestPath,
                    data: requestData
                });
            }
             
            return commandService.sendCommand(behavior.callType, requestPath, requestData);
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['AlertUser'] = {
        returnsPromise: true,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > alertUser');
            var uidlAlert = m_app.userAlerts[toCamelCase(behavior.alertQualifiedName)];
            var deferred = $q.defer(); 
            var alertHandle = {
                uidl: uidlAlert,
                parameters: { },
                faultInfo: null,
                answer: function(choice) {
                    return deferred.resolve(choice);
                }
            };
            var context = {
                model: {
                    Input: input,
                    Data: scope.model.Data,
                    State: scope.model.State
                }
            };
            var contextAsEnumerable = Enumerable.Return(context);
            for (var i = 0; i < behavior.parameterExpressions.length ; i++) {
                alertHandle.parameters[uidlAlert.parameterNames[i]] = contextAsEnumerable.Select('ctx=>ctx.' + behavior.parameterExpressions[i]).Single();
            }
            if (behavior.faultInfoExpression) {
                alertHandle.faultInfo = contextAsEnumerable.Select('ctx=>ctx.' + behavior.faultInfoExpression).Single();
                $rootScope.userAlertTechnicalInfo = JSON.stringify(alertHandle.faultInfo, null, 4);
            } else {
                $rootScope.userAlertTechnicalInfo = null;
            }
            switch (behavior.displayMode) {
                case 'Inline':
                    scope.inlineUserAlert.current = alertHandle;
                    break;
                case 'Popup':
                    $rootScope.showPopupAlert(alertHandle);
                    break;
                case 'Modal':
                    $rootScope.$broadcast(m_app.modalAlert.qualifiedName + ':Show', alertHandle);
                    break;
            }
            if (uidlAlert.resultChoices.length) {
                return deferred.promise;
            } else {
                return $q.when(true);
            }
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['AlterModel'] = {
        returnsPromise: false,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > alertModel', behavior.alterations, input);
            scope.model.Input = input;
            performModelAlterations(scope.model, behavior.alterations);

            /*
            for (var i = 0; i < behavior.alterations.length; i++) {
                var alteration = behavior.alterations[i];
                switch (alteration.type) {
                    case 'Copy':
                        var value = null;
                        if (alteration.sourceValue) {
                            value = alteration.sourceValue;
                        } else if (alteration.sourceExpression && alteration.sourceExpression!=='null')                         {
                            value = Enumerable.Return(context).Select('ctx=>ctx.' + alteration.sourceExpression).Single();
                        }
                        var target = context;
                        for (var j = 0; j < alteration.destinationNavigations.length - 1; j++) {
                            var nextTarget = target[alteration.destinationNavigations[j]];
                            if (!nextTarget) {
                                nextTarget = target[alteration.destinationNavigations[j]] = { };
                            }
                            target = nextTarget;
                        }
                        var lastNavigation = alteration.destinationNavigations[alteration.destinationNavigations.length-1];
                        target[lastNavigation] = value;
                        break;
                }
            }
            */
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['ProjectInput'] = {
        returnsPromise: false,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > projectModel', behavior.alterations, input);
            var projectedInput = {
                Source: input,
                Target: { }
            };
            var projectedModel = {
                State: scope.model.State,
                Data: scope.model.Data,
                Input: projectedInput,
                appState: scope.model.appState
            }
            performModelAlterations(projectedModel, behavior.alterations);
            return projectedModel.Input;
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['BranchByRule'] = {
        returnsPromise: false,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > switch', behavior.branchRules, input);

			var inputContext = createInputContext(scope.model, input);
            var leftSideValue = selectValue(inputContext, behavior.valueExpression);

			console.log('run-behavior > switch > value', leftSideValue);

            for (var i = 0; i < behavior.branchRules.length; i++) {
				var rule = behavior.branchRules[i];
                var rightSideValue = (
                    rule.valueExpression ?
                    selectValue(inputContext, rule.valueExpression) : 
                    rule.valueConstant);
				
				if (leftSideValue === rightSideValue) {
					console.log('run-behavior > switch > matched rule #', i);
					implementBehavior(scope, rule.onMatch, input);
					return;
				}
            }
			
			if (behavior.otherwise) {
				console.log('run-behavior > switch > otherwise')
				implementBehavior(scope, behavior.otherwise, input);
			}
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['QueryModel'] = {
        returnsPromise: true,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > queryModel', behavior.sourceExpression);
            scope.model.Input = input;
            var context = {
                model: scope.model
            };
            
            var value = (
                behavior.sourceExpression==='null' || !behavior.sourceExpression
                ? behavior.constantValue
                : Enumerable.Return(context).Select('ctx=>ctx.' + behavior.sourceExpression).Single());
            return $q.when(value);
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['QueryAppState'] = {
        returnsPromise: true,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > queryAppState', behavior.sourceExpression);
            scope.model.Input = input;
            var context = {
                state: scope.appScope.model.state
            };
            
            var value = (
                behavior.sourceExpression==='null' || !behavior.sourceExpression
                ? behavior.constantValue
                : Enumerable.Return(context).Select('ctx=>ctx.' + behavior.sourceExpression).Single());
            return $q.when(value);
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['ActivateSessionTimeout'] = {
        returnsPromise: false,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > activateSessionTimeout', behavior.idleMinutesExpression);
            var timeoutMinutes = 0;
            if (behavior.idleMinutesExpression) {
                scope.model.Input = input;
                var context = {
                    model: scope.model
                };
                timeoutMinutes = Enumerable.Return(context).Select('ctx=>ctx.' + behavior.idleMinutesExpression).Single();
            } else {
                timeoutMinutes = m_app.sessionIdleTimeoutMinutes;
            }
            var timeoutMs = (timeoutMinutes * 60000) - 10000;
            sessionService.activateExpiry(timeoutMs, scope.appScope.uidl.qualifiedName + ':UserSessionExpired');
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['DeactivateSessionTimeout'] = {
        returnsPromise: false,
        execute: function (scope, behavior, input) {
            console.log('run-behavior > deactivateSessionTimeout');
            sessionService.deactivateExpiry();
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['RestartApp'] = {
        returnsPromise: false,
        execute: function (scope, behavior, input) {
            window.location.reload();
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_behaviorImplementations['DownloadContent'] = {
        returnsPromise: false,
        execute: function (scope, behavior, input) {
            scope.model.Input = input;
            var context = {
                model: scope.model
            };
            var contentId = Enumerable.Return(context).Select('ctx=>ctx.' + behavior.contentIdExpression).Single();
            $rootScope.beginBrowserDownload('app/downloadContent/' + contentId);
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_dataBindingImplementations['AppState'] = {
        execute: function(scope, binding) {
            try {
                var initialValue = readValueFromSource();
                writeValueToDestination(initialValue);
            }
            catch(err) {
            }            
            
            scope.$watch('model.' + binding.sourceExpression, function(newValue, oldValue) {
                writeValueToDestination(newValue);
            });
        
            function readValueFromSource() {
                var context = {
                    appState: scope.model.appState
                };
                var value = Enumerable.Return(context).Select('ctx=>ctx.' + binding.sourceExpression).Single();
                return value;
            }
            
            function writeValueToDestination(value) {
                var target = scope;
                for (var i = 0; i < binding.destinationNavigations.length - 1; i++) {
                    target = target[binding.destinationNavigations[i]];
                }
                var lastNavigation = binding.destinationNavigations[binding.destinationNavigations.length-1];
                target[lastNavigation] = value;
            }
        }
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['ScreenPartContainer'] = {
        implement: function (scope) {
            scope.$on(scope.uidl.qualifiedName + ':NavReq', function (event, data) {
                console.log('screenPartContainer::on-NavReq', scope.uidl.qualifiedName, '->', data.screenPart.qualifiedName);
                scope.currentScreenPart = null;
                $timeout(function() {
                    scope.currentScreenPart = data.screenPart;
                    location.hash = data.screenPart.qualifiedName;
                    $timeout(function() {
                        scope.$broadcast(data.screenPart.qualifiedName + ':NavigatedHere', data.input);
                        if (data.screenPart.contentRoot) {
                            scope.$broadcast(data.screenPart.contentRoot.qualifiedName + ':NavigatedHere', data.input);
                        }
                        $rootScope.$broadcast(scope.uidl.qualifiedName + ':ScreenPartLoaded', scope.currentScreenPart);
                        //if (oldScreenPart) {
                        //    $rootScope.$broadcast(oldScreenPart.qualifiedName + ':NavigatingAway', data.input);
                        //}
                    });
                });
            });
            if (scope.uidl.initalScreenPartQualifiedName) {
                beginGetScreenPartUidl(scope.uidl.initalScreenPartQualifiedName).then(function(screenPart) {
                    scope.currentScreenPart = screenPart;
                    $timeout(function() {
                        scope.$broadcast(scope.uidl.initalScreenPartQualifiedName + ':NavigatedHere');
                        $rootScope.$broadcast(scope.uidl.qualifiedName + ':ScreenPartLoaded', scope.currentScreenPart);
                    });
                });
            }
        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['LanguageSelector'] = {
        implement: function (scope) {
            function getNeutralCulture(name) {
                var hyphenPos = name.indexOf('-');
                if (hyphenPos > 0) {
                    return name.substr(0, hyphenPos);
                }
                return name;
            };

            var uidlDoc = scope.uidlService.getDocument();
            var allLocales = uidlDoc.locales;
            var allNames = [];
            for (var locale in allLocales) {
                if (allLocales.hasOwnProperty(locale) && locale !== uidlDoc.currentLocaleIdName) {
                    allNames.push(getNeutralCulture(locale));
                }
            }
            
            scope.allLanguages = allNames;
            scope.currentLanguage = getNeutralCulture(uidlDoc.currentLocaleIdName);
        }
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['ModalUserAlert'] = {
        implement: function (scope) {
            scope.$on(scope.uidl.qualifiedName + ':Show', function(event, data) {
                scope.alert = data;
                scope.$broadcast(scope.uidl.qualifiedName + ':ShowModal');
            });
            
            scope.answerAlert = function(choice) {
                scope.$broadcast(scope.uidl.qualifiedName + ':HideModal');
                scope.alert.answer(choice);
                scope.alert = null;
            }
        }
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['UserAlertBox'] = {
        implement: function (scope) {
            scope.invokeAction = function() {
                if (scope.model.State.ActionUrl) {
                    window.location.href = scope.model.State.ActionUrl;
                } else {
                    window.location.reload();
                }
            };
            
            scope.$on(scope.uidl.qualifiedName + ':StateSetter', function(event, data) {
                scope.model = { 
                    State: data 
                };
                
                if (data.Details && data.Details.constructor != Array) {
                    data.Details = [ data.Details ];
                }
            });
        }
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['ManagementConsole'] = {
        implement: function (scope) {
            
            scope.copyOfMainMenu = angular.copy(scope.uidl.mainMenu);
            
            function implementMenuItems(items) {
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    if (item.behaviors) {
                        for (var j = 0; j < item.behaviors.length; j++) {
                            var behavior = item.behaviors[j];
                            if (behavior.subscription) {
                                implementSubscription(scope, behavior);
                            }
                        }
                    }
                    implementMenuItems(item.subItems);
                }
            }

            implementMenuItems(scope.copyOfMainMenu.items);

            if (window.appInit) {
			    window.appInit();
            }

			scope.invokeUtilityCommand = function(command) {
				scope.utilityCommandInProgress = true;
				if (scope.$parent) {
					scope.$parent.$emit(command.qualifiedName + ':Executing');
				}
				scope.$broadcast(command.qualifiedName + ':Executing');
			};
			
			scope.isUtilityCommandEnabled = function(command) {
				return true;
			};
			
            scope.$on(scope.uidl.qualifiedName + ':UtilityCommandCompleted', function (event, data) {
                scope.utilityCommandInProgress = false;
            });
			
			
            scope.$on(scope.uidl.qualifiedName + ':MainContent:ScreenPartLoaded', function (event, data) {
                scope.mainContentScreenPart = data;
            });
            
            scope.$on("$destroy", function() {
                console.log('uidlWidget::$destroy() - MANAGEMENT CONSOLE - ', scope.uniqueWidgetId);
            }); 

        },
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['Report'] = {
        implement: function (scope) {
            scope.uidl.criteriaForm.$skin = scope.uidl.$skin;
            scope.uidl.resultTable.$skin = scope.uidl.$skin;
            if (scope.uidl.visualizationChart) {
                scope.uidl.visualizationChart.$skin = scope.uidl.$skin;
            }

            scope.hasVisualization = (hasEnumFlag(scope.uidl.reportComponents, 'Visualization') && scope.uidl.visualizationChart);
            scope.hasResultSet = hasEnumFlag(scope.uidl.reportComponents, 'ResultSet');
            
            scope.model.State.criteria = {};
            scope.model.State.reportInProgress = false;
            
            if (!scope.uidl.criteriaForm.needsInitialModel) {
                $timeout(function() {
                    scope.$broadcast(scope.uidl.qualifiedName + ':CriteriaForm:ModelSetter', scope.model.State.criteria);
                });
            }

            scope.$on(scope.uidl.qualifiedName + ':ShowReport:Executing', function(event, data) {
                scope.model.State.reportInProgress = true;
            });
            scope.$on(scope.uidl.resultTable.qualifiedName + ':QueryCompleted', function(event, data) {
                if (scope.model.State.reportInProgress===true) {
                    scope.model.State.reportInProgress = false;
                    scope.$emit(scope.uidl.qualifiedName + ':ReportReady', data);
                }
            });
            scope.$on(scope.uidl.resultTable.qualifiedName + ':QueryFailed', function(event, data) {
                if (scope.model.State.reportInProgress===true) {
                    scope.$emit(scope.uidl.qualifiedName + ':ReportFailed', data);
                }
                scope.model.State.reportInProgress = false;
            });
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['ChartReport'] = {
        implement: function (scope) {
            scope.model.State.criteria = {};

            $timeout(function () {
                scope.$broadcast(scope.uidl.qualifiedName + ':CriteriaForm:ModelSetter', scope.model.State.criteria);
            });
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

	m_controllerImplementations['Chart'] = {
        implement: function(scope) {
            scope.invokeCommand = function(command, item) {
                for (var i = 0; i < command.items.length; i++) {
                    command.items[i].isChecked = (command.items[i].value === item.value);
                }
                scope.$emit(command.qualifiedName + ':Executing', item.value);
                $rootScope.$broadcast(':global:CommandCheckChanged', command);
            }

            scope.calculateHeight = function(mediumValue) {
                if (!scope.uidl.height) {
                    return mediumValue;
                }
                
                var factor = 1;
                
                switch (scope.uidl.height) {
                    case 'ExtraSmall': factor = 0.25; break;
                    case 'Small': factor = 0.5; break;
                    case 'MediumSmall': factor = 0.75; break;
                    case 'MediumLarge': factor = 1.5; break;
                    case 'Large': factor = 2; break;
                    case 'ExtraLarge': factor = 4; break;
                };
                
                return parseInt(mediumValue * factor);
            };
            
			scope.$on(':global:CommandCheckChanged', function (event, data) {
			    var command = Enumerable.From(scope.uidl.commands).FirstOrDefault('c=>c.qualifiedName=="' + data.qualifiedName + '"');
                if (command) {
                    for (var i = 0; i < command.items.length; i++) {
                        command.items[i].isChecked = data.items[i].isChecked;
                    }
                }
			});

			scope.$on(scope.uidl.modelSetterQualifiedName, function (event, data) {
                if (scope.uidl.dataExpression) {
                    scope.model.State.data = scope.uidlService.selectValue(data, scope.uidl.dataExpression);
                } else {
                    scope.model.State.data = data;
                }
			});
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['MenuItem'] = {
        implement: function (scope) {
            scope.displayItems = [{
                text: scope.uidl.text,
                icon: scope.uidl.icon
            }];
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['Gauge'] = {
        implement: function (scope) {
			scope.$on(scope.uidl.modelSetterQualifiedName, function (event, data) {
                var valueContext = {
                    input: data
                };
                var gaugeValues = [];
                
                for(var i = 0; i < scope.uidl.values.length; i++) {
                    var valueUidl = scope.uidl.values[i];
                    gaugeValues.push({
                        title: valueUidl.title,
                        value: scope.uidlService.selectValue(valueContext, valueUidl.valueProperty),
                        alertType: valueUidl.alertType,
                        alertText: valueUidl.alertText
                    });
                }
                
                scope.model.State.values = gaugeValues;
			});

            scope.$on(scope.uidl.updateSourceQualifiedName, function(event, data) {
                scope.model.State.data = data;
            });
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['Crud'] = {
        implement: function (scope) {
            if (scope.uidl.formTypeSelector && scope.uidl.formTypeSelector.selections.length === 1) {
                scope.uidl.entityName = scope.uidl.formTypeSelector.selections[0].widget.entityName;
                scope.uidl.form = scope.uidl.formTypeSelector.selections[0].widget;
                scope.uidl.formTypeSelector = null;
            }

            var metaType = scope.uidlService.getMetaType(scope.uidl.entityName);
            scope.metaType = metaType;
            scope.commandInProgress = false;
            scope.entityAuth = null;

            scope.refresh = function () {
                scope.resetCrudState();
                scope.requestAuthorization();
                scope.queryEntities();
            };

            scope.softRefresh = function() {
                scope.uiShowCrudForm = false;
                scope.selectedEntity = null;
                scope.entity = null;
                scope.$broadcast(scope.uidl.grid.qualifiedName + ':PageRefreshRequested');
            };
            
            scope.requestAuthorization = function (optionalEntityId) {
                if (scope.uidl.disableAuthorizationChecks) {
                    return $q.when({ CanRetrieve: true, CanCreate: false, CanUpdate: false, CanDelete: false });
                }
                
                return scope.entityService.checkAuthorization(scope.uidl.grid.entityName, optionalEntityId).then(
                    function(response) {
                        scope.entityAuth = response;
						scope.originalEntity = (response.fullEntity ? angular.copy(response.fullEntity) : null);
                        return response;
                    },
                    function(fault) {
                        scope.entityAuth = null;
                        scope.$emit(scope.uidl.qualifiedName + ':QueryEntityFailed', commandService.createFaultInfo(fault));
                        return $q.reject(fault);
                    }
                );
            };
            
            scope.updateSelectedEntityCommands = function () {
                if (scope.uidl.entityCommands && 
                    scope.uidl.entityCommands.length > 0 && 
                    scope.uidl.updateCommandsOnSelection && 
                    scope.selectedEntity && scope.selectedEntity['$id']) 
                {
                    scope.requestAuthorization(scope.selectedEntity['$id']);
                }
            };

            scope.isEntityCommandEnabled = function (command) {
                if (command.authorization.operationName && scope.entityAuth && scope.entityAuth.enabledOperations) {
                    return (scope.entityAuth.enabledOperations[command.authorization.operationName] === true);
                }
                return true;
            };
            
            scope.queryEntities = function () {
                scope.selectedEntity = null;
                scope.commandInProgress = true;
                
                if (scope.uidl.mode !== 'Inline' || scope.uidl.inlineStorageStyle === 'InverseForeignKey') {
                    scope.resultSet = null;
                    var query = scope.entityService.newQueryBuilder(scope.uidl.entityName);
                    
                    if (scope.uidl.entityTypeFilter) {
                        query.ofType(scope.uidl.entityTypeFilter);
                    }

                    if (scope.uidl.inlineStorageStyle === 'InverseForeignKey') {
                        query.where(scope.uidl.inverseNavigationProperty, scope.parentEntityId, 'eq');
                    }

                    var preparedRequest = {
                        query: query,
                        data: { }
                    };
                    
                    scope.$broadcast(scope.uidl.grid.qualifiedName + ':RequestPrepared', preparedRequest);
                } else {
                    $timeout(function() {
                        scope.commandInProgress = false;
                        scope.$broadcast(scope.uidl.qualifiedName + ':Grid:DataReceived', scope.resultSet);
                    });
                }
            };

            scope.resetCrudState = function() {
                scope.uiShowCrudForm = false;
                scope.selectedEntity = null;
                scope.entity = null;
                scope.entityAuth = null;
                scope.commandInProgress = false;
            };

            scope.$on(scope.uidl.grid.qualifiedName + ':QueryCompleted', function (event, data) {
                scope.resultSet = data.ResultSet;
                scope.commandInProgress = false;
            });

            scope.$on(scope.uidl.qualifiedName + ':NavigatedHere', function (event) {
                scope.refresh();
            });

            scope.$on(scope.uidl.qualifiedName + ':RefreshRequested', function (event) {
                scope.refresh();
            });

            scope.$on(scope.uidl.qualifiedName + ':Grid:ObjectSelected', function(event, data) {
                scope.$apply(function() {
                    scope.selectedEntity = data;
                    scope.updateSelectedEntityCommands();
                });
            });

            scope.$on(scope.uidl.qualifiedName + ':Grid:ObjectSelectedById', function (event, id) {
                scope.$apply(function() {
                    scope.selectedEntity = Enumerable.From(scope.resultSet).Where("$.$id == '" + id + "'").First();
                    scope.updateSelectedEntityCommands();
                });
            });

            scope.$on(scope.uidl.qualifiedName + ':Grid:ObjectSelectedByIndex', function (event, index) {
                scope.$apply(function() {
                    scope.selectedEntity = scope.resultSet[index];
                    scope.updateSelectedEntityCommands();
                });
            });

            scope.$on(scope.uidl.qualifiedName + ':Grid:EditRequested', function(event, data) {
                scope.$apply(function() {
                    scope.selectedEntity = data;
                    scope.editEntity(scope.selectedEntity);
                });
            });

            scope.$on(scope.uidl.qualifiedName + ':Grid:EditRequestedById', function (event, id) {
                scope.$apply(function() {
                    scope.selectedEntity = Enumerable.From(scope.resultSet).Where("$.$id == '" + id + "'").First();
                    scope.editEntity(scope.selectedEntity);
                });
            });

            scope.$on(scope.uidl.qualifiedName + ':Grid:EditRequestedByIndex', function (event, index) {
                scope.$apply(function() {
                    scope.selectedEntity = scope.resultSet[index];
                    scope.editEntity(scope.selectedEntity);
                });
            });

            scope.$on(scope.uidl.qualifiedName + ':ContextSetter', function(event, id) {
                if (id) {
                    scope.editEntity({ '$id': id });
                }
            });
            
            scope.editEntity = function (entity) {
                if (!entity || scope.uidl.disableForm) {
                    return;
                }

                scope.model.entity = entity;
                scope.model.isNew = false;
                scope.entityAuth = null;
                scope.uiShowCrudForm = true;

                scope.requestAuthorization(entity['$id']).then(function(response) {
                    if (response.update===true) {
                        if (scope.uidl.formTypeSelector) {
                            for (var i = 0; i < scope.uidl.formTypeSelector.selections.length ; i++) {
                                scope.$broadcast(scope.uidl.formTypeSelector.selections[i].widget.qualifiedName + ':EditAuthorized', response);
                            }
                        } else {
                            scope.$broadcast(scope.uidl.form.qualifiedName + ':EditAuthorized', response);
                        }
                    }
                    
                    if (response.fullEntity) {
                        scope.entityService.trackRetrievedEntities([ response.fullEntity ]);
                        scope.model.entity = response.fullEntity;
                    }
                    
                    $timeout(function() {
                        if (scope.uidl.formTypeSelector) {
                            scope.$broadcast(scope.uidl.formTypeSelector.qualifiedName + ':ModelSetter', scope.model.entity);
                        } else {
                            scope.$broadcast(scope.uidl.form.qualifiedName + ':ModelSetter', scope.model.entity);
                        }
                    });
                });
            };

            scope.newEntity = function () {
                scope.selectedEntity = null;

                if (scope.uidl.formTypeSelector) {
                    scope.newEntityCreated({});
                } else {
                    scope.entityService.newDomainObject(metaType.name).then(
                        function (newObj) {
                            scope.newEntityCreated(newObj);
                        },
                        function (fault) {
                            scope.$emit(scope.uidl.qualifiedName + ':NewDomainObjectFailed', commandService.createFaultInfo(fault));
                        }
                    );
                }
            };

            scope.newEntityCreated = function(newObj) {
                scope.model.entity = newObj;
                scope.model.isNew = true;

                scope.uiShowCrudForm = true;

                $timeout(function () {
                    if (scope.uidl.mode === 'Inline' && scope.uidl.inlineStorageStyle === 'InverseForeignKey') {
                        newObj[scope.uidl.inverseNavigationProperty] = scope.parentEntityId;
                    }
                    
                    if (scope.uidl.form) {
                        scope.$broadcast(scope.uidl.form.qualifiedName + ':ModelSetter', scope.model.entity);
                        scope.$broadcast(scope.uidl.form.qualifiedName + ':EditAuthorized');
                    } else if (scope.uidl.formTypeSelector) {
                        scope.$broadcast(scope.uidl.formTypeSelector.qualifiedName + ':ModelSetter', scope.model.entity);
                        scope.$broadcast(scope.uidl.formTypeSelector.qualifiedName + ':EditAuthorized');
                    }
                });
            };

            scope.deleteEntity = function (entity) {
                if (!entity) {
                    return;
                }

                if (scope.uidl.mode !== 'Inline' || scope.uidl.inlineStorageStyle === 'InverseForeignKey') {
                    scope.entityService.deleteEntity(entity).then(
                        function(result) {
                            scope.queryEntities();
                            scope.uiShowCrudForm = false;
                            scope.$emit(scope.uidl.qualifiedName + ':DeleteEntityCompleted');
                        },
                        function (fault) {
                            scope.$emit(scope.uidl.qualifiedName + ':DeleteEntityFailed', commandService.createFaultInfo(fault));
                        }
                    );
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
                //scope.$broadcast(':global:FormValidating', { isValud: true });

                if (scope.uidl.mode !== 'Inline' || scope.uidl.inlineStorageStyle === 'InverseForeignKey') {
					var diff = (scope.originalEntity ? scope.entityService.getEntityDiff(entity, scope.originalEntity) : entity);
					
					scope.entityService.storeEntity(diff).then(
                        function() {
                            scope.$emit(scope.uidl.qualifiedName + ':StoreEntityCompleted');

                            if (scope.uidl.disableGrid) {
                                scope.commandInProgress = false;
                                scope.editEntity(entity);
                            } else {
                                scope.softRefresh();
                            }
                        },
                        function (fault) {
                            scope.$emit(scope.uidl.qualifiedName + ':StoreEntityFailed', commandService.createFaultInfo(fault));
                            scope.commandInProgress = false;
                            scope.resetFormState();
                        }
                    );
                } else {
                    if (scope.model.isNew) {
                        scope.resultSet.push(entity);
                    }
                    scope.refresh();
                }
            };

            scope.recalcAfterChange = function (entity) {
                if (scope.uidl.mode !== 'Inline' || scope.uidl.inlineStorageStyle === 'InverseForeignKey') {
                    scope.entityService.recalcEntity(entity).then(
                        function(newEntity) {
                            scope.$broadcast(scope.uidl.form.qualifiedName + ':ModelUpdater', newEntity);
                        },
                        function(fault) {
                            scope.$emit(scope.uidl.qualifiedName + ':RecalculateEntityFailed', commandService.createFaultInfo(fault));
                        }
                    );
                }
            };
            
            scope.rejectChanges = function (entity) {
                if (scope.uidl.mode !== 'Inline' || scope.uidl.inlineStorageStyle === 'InverseForeignKey') {
                    scope.commandInProgress = false;
                    scope.softRefresh();
                } else {
                    scope.refresh();
                }
            };

            scope.isCommandAuthorized = function (command) {
                if (command.kind !== 'Submit') {
                    return true;
                }
                if (!scope.entityAuth) {
                    return false;
                }
                if (scope.model.isNew) {
                    return (scope.entityAuth.create===true);
                } else {
                    return (scope.entityAuth.update===true);
                }
            };
            
            scope.invokeEntityCommand = function (command) {
                scope.$emit(command.qualifiedName + ':Executing');
            };

            scope.invokeStaticCommand = function (command) {
                scope.$emit(command.qualifiedName + ':Executing');
            };

            scope.invokeCommand = function (command) {
                scope.commandInProgress = true;
                if (command.kind==='Submit') {
                    var validationResult = { isValid: false };
                    scope.$broadcast(':global:FormValidating', validationResult);
                    $timeout(function() {
                        if (validationResult.isValid===true) {
                            scope.resetFormState({ commandInProgress: true });
                            scope.$emit(command.qualifiedName + ':Executing');
                        } else {
                            scope.commandInProgress = false;
                        }
                    });
                } else {
                    scope.$emit(command.qualifiedName + ':Executing');
                }
            };
            
            scope.resetFormState = function(state) {
                if (scope.uidl.form) {
                    scope.$broadcast(scope.uidl.form.qualifiedName + ':StateResetter', state);
                } else if (scope.uidl.formTypeSelector) {
                    scope.$broadcast(scope.uidl.formTypeSelector.qualifiedName + ':StateResetter', state);
                }
            };

            scope.$on(scope.uidl.qualifiedName + ':EditAuthorized', function(event, data) {
                scope.requestAuthorization();
            });

            scope.$on(scope.uidl.qualifiedName + ':ModelSetter', function (event, data) {
                scope.commandInProgress = false;

                if (scope.uidl.inlineStorageStyle === 'InverseForeignKey') {
                    scope.parentEntityId = data['$id'];
                    scope.refresh();
                } else {
                    var auth = scope.entityAuth;
                    scope.resetCrudState();
                    scope.entityAuth = auth;

                    if (!auth || !auth.update) {
                        scope.uidl.disableAuthorizationChecks = true;
                    }
                    
                    scope.resultSet = data;
                    scope.selectedEntity = null;
                    scope.$broadcast(scope.uidl.qualifiedName + ':Grid:DataReceived', scope.resultSet);
                }
            });

            if (scope.uidl.form && scope.uidl.form.autoRecalculateOnChange) {
                scope.$on(scope.uidl.form.qualifiedName + ':Changed', function (event, changedEntity) {
                    scope.recalcAfterChange(changedEntity);
                });
            }
            scope.$on(scope.uidl.qualifiedName + ':Save:Executing', function (event) {
                scope.saveChanges(scope.model.entity);
            });
            scope.$on(scope.uidl.grid.qualifiedName + ':InlineSave:Executing', function (event, entity) {
				if (scope.uidl.grid.inlineEditor) {
					scope.$emit(scope.uidl.grid.inlineEditor.qualifiedName + ':Submitting', entity);
					$timeout(function() {
						scope.saveChanges(entity);
					});
				} else {
					scope.saveChanges(entity);
				}
            });
            scope.$on(scope.uidl.qualifiedName + ':Cancel:Executing', function (event) {
                scope.rejectChanges(scope.model.entity);
            });
            scope.$on(scope.uidl.qualifiedName + ':Delete:Executing', function (event) {
                scope.deleteEntity(scope.model.entity);
            });
            
            scope.$watch('selectedEntity', function(newValue, oldValue) {
                scope.$emit(scope.uidl.qualifiedName + ':SelectedEntityChanged', newValue);
            });
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['DataGrid'] = {
        implement: function (scope) {
            scope.metaType = scope.uidlService.getMetaType(scope.uidl.entityName);

            //var dataQuery = scope.uidl.dataQuery;

            if (scope.uidl.displayColumns && scope.uidl.displayColumns.length) {
                scope.gridColumns = scope.uidl.displayColumns;
            } else {
                scope.gridColumns = scope.uidl.defaultDisplayColumns;
            }
            
            scope.defaultSortColumn = Enumerable.From(scope.gridColumns)
                .Where(function(c) { return (c.defaultSort===true); })
                .FirstOrDefault();
            
            if (scope.uidl.enableAutonomousQuery) {
                $timeout(function() {
                    var query = scope.entityService.newQueryBuilder(scope.uidl.entityName);
                    var preparedRequest = {
                        query: query,
                        data: { }
                    };
                    //scope.onRequestPrepared(null, preparedRequest);
                    scope.$broadcast(scope.uidl.qualifiedName + ':RequestPrepared', preparedRequest);
                });
            }

            scope.beginQueryDetailPaneData = function(rowData) {
                if (!scope.uidl.detailPaneQueryServer) {
                    var value = (
                        scope.uidl.detailPaneExpression ? 
                        selectValue(rowData, scope.uidl.detailPaneExpression) : 
                        rowData);
                    return $q.when(value);
                } 
                
                var queryBuilder = null;
                var queryData = null;
                
                if (scope.preparedRequest) {
                    queryBuilder = (
                        scope.preparedRequest.query ? 
                        scope.preparedRequest.query.clone() : 
                        new EntityQueryBuilder(scope.uidl.entityName, scope.preparedRequest.url));
                    queryData = scope.preparedRequest.data;
                } else {
                    queryBuilder = new EntityQueryBuilder(scope.uidl.entityName);
                }

                if (scope.uidl.detailPaneExpression) {
                    queryBuilder.select(scope.uidl.detailPaneExpression);
                }
                queryBuilder.where('$id', rowData.$id);
                
                var requestUrl = queryBuilder.getQueryUrl();
                return $http.post(requestUrl, queryData).then(
                    function (response) {
                        if (response.data.ResultSet && response.data.ResultSet.length === 1) {
                            var record = response.data.ResultSet[0];
                            var value = (
                                scope.uidl.detailPaneExpression ? 
                                selectValue(record, scope.uidl.detailPaneExpression) : 
                                record);
                            return $q.when(value);
                        } else {
                            return $q.reject(response.data);
                        }
                    },
                    function (fault) {
                        return $q.reject(fault);
                    }
                );
            };
            
            //for (var i = 0; i < scope.gridColumns.length; i++) {
            //    var column = scope.gridColumns[i];
            //    column.metaType = scope.uidlService.getMetaType(column.declaringTypeName);
            //    column.metaProperty = column.metaType.properties[toCamelCase(column.navigations[column.navigations.length - 1])];
            //}

            //scope.displayProperties = Enumerable.From(scope.uidl.displayColumns).Select(function (name) {
            //    return metaType.properties[toCamelCase(name)];
            //}).ToArray();

            //scope.refresh = function () {
            //    scope.queryEntities();
            //};

            //scope.queryEntities = function () {
            //    scope.resultSet = null;
            //    if (scope.uidl.mode !== 'Inline') {
            //        scope.entityService.queryEntity(scope.uidl.entityName + dataQuery).then(function (data) {
            //            scope.resultSet = data.ResultSet;
            //        });
            //    } else {
            //        scope.resultSet = scope.parentModel.entity[scope.parentUidl.propertyName];
            //    }
            //};

            //scope.$on(scope.uidl.qualifiedName + ':Search:Executing', function (event) {
            //    dataQuery = "";
            //    scope.queryEntities();
            //});

            //scope.queryEntities();
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['Form'] = {
        implement: function (scope) {
            function fieldHasModifier(field, modifier) {
                return (field.modifiers && field.modifiers.indexOf(modifier) > -1);
            };
            
            if (scope.uidl.$skin) {
                for (var i = 0; i < scope.uidl.fields.length ; i++) {
                    scope.uidl.fields[i].$skin = scope.uidl.$skin;
                }
            }
            
            scope.fieldHasModifier = fieldHasModifier;
            scope.metaType = scope.uidlService.getMetaType(scope.uidl.entityName);
            scope.tabSetIndex = 0;
            scope.plainFields = Enumerable.From(scope.uidl.fields)
                .Where(function(f) { return !fieldHasModifier(f, 'Tab') && !fieldHasModifier(f, 'Section'); })
                .Where(function(f) { return !fieldHasModifier(f, 'RangeEnd'); })
                .Where(function(f) { return scope.isUidlAuthorized(f); })
                .ToArray();
            scope.sectionFields = Enumerable.From(scope.uidl.fields)
                .Where(function(f) { return fieldHasModifier(f, 'Section') })
                .Where(function(f) { return scope.isUidlAuthorized(f); })
                .ToArray();
            scope.tabSetFields = Enumerable.From(scope.uidl.fields)
                .Where(function(f) { return fieldHasModifier(f, 'Tab') })
                .Where(function(f) { return scope.isUidlAuthorized(f); })
                .ToArray();
            scope.calculatedFields = Enumerable.From(scope.uidl.fields)
                .Where(function(f) { return f.isCalculated; })
                .ToArray();

            scope.waitingForInitialModel = (scope.uidl.needsInitialModel ? true : false);
            scope.commandInProgress = scope.waitingForInitialModel;
            scope.editAuthorized = (scope.uidl.needsAuthorize ? false : true);

			if (scope.uidl.mode === 'StandaloneCreate') {
				scope.parentModel = {
				    entity: scope.entityService.newDomainObject(scope.metaType.name)
				};
			}

            scope.selectTab = function(index) {
                scope.tabSetIndex = index;
            };

            scope.invokeCommand = function (command) {
                scope.commandInProgress = true;
                if (command.kind==='Submit') {
                    var validationResult = { isValid: false };
                    scope.$broadcast(':global:FormValidating', validationResult);
                    $timeout(function() {
                        if (validationResult.isValid===true) {
                            scope.$emit(command.qualifiedName + ':Executing');
                            scope.$emit(scope.uidl.qualifiedName + ':Submitted', scope.model.Data.entity);
                        } else {
                            scope.commandInProgress = false;
                        }
                    });
                } else {
                    scope.$emit(command.qualifiedName + ':Executing');
                }
            };

            scope.autoSubmitForm = function() {
                for (var i = 0; i < scope.uidl.commands.length ; i++) {
                    var command = scope.uidl.commands[i];
                    if (command.kind === 'Submit') {
                        scope.invokeCommand(command);
                        break;
                    }
                }
            };
            
            scope.validate = function(deferred) {
                var result = true;

                var validateFuncName = 'validateWidget_Form';
                var validateFunc = window[validateFuncName];
                if (typeof validateFunc === 'function') {
                    result = validateFunc(scope);
                }

                for (var i = 0; i < scope.uidl.fields.length; i++) {
                    if (scope.uidl.fields[i].nestedWidget) {
                        
                    }
                }
            };

            scope.$on(scope.uidl.qualifiedName + ':ModelSetter', function(event, data) {
                scope.model.Data.entity = data;
                scope.suppressAutoSubmitOnce = true;
                scope.commandInProgress = false;
                scope.waitingForInitialModel = false;
                scope.tabSetIndex = 0;
                
                if (!data) {
                    return;
                }

                $timeout(function() {
                    Enumerable.From(scope.uidl.fields)
                        .Where("$.fieldType=='InlineGrid' || $.fieldType=='InlineForm' || $.fieldType=='LookupMany'")
                        .ForEach(function (field) {
                            if (field.nestedWidget.widgetType === 'Crud' && field.nestedWidget.inlineStorageStyle === 'InverseForeignKey') {
                                scope.$broadcast(field.nestedWidget.qualifiedName + ':ModelSetter', data);
                            } else {
                                scope.$broadcast(field.nestedWidget.qualifiedName + ':ModelSetter', data[field.propertyName]);
                            }
                        });
                    
                    if (scope.uidl.autoSubmitOnChange || scope.uidl.autoRecalculateOnChange) {
                        scope.$watch('model.Data.entity', function(newValue, oldValue) {
                            if (scope.suppressAutoSubmitOnce) {
                                scope.suppressAutoSubmitOnce = false;
                            } else if (scope.uidl.autoSubmitOnChange) {
                                scope.autoSubmitForm();
                            } else if (scope.uidl.autoRecalculateOnChange) {
                                if (!scope.autoRecalculateTimer) {
                                    scope.autoRecalculateTimer = $timeout(function() {
                                        scope.autoRecalculateTimer = null;
                                        scope.suppressAutoSubmitOnce = true; // ignore watcher invocation after setting new values on calculated properties
                                        scope.$emit(scope.uidl.qualifiedName + ':Changed', scope.model.Data.entity);
                                    }, 500);
                                }
                            }
                        }, true);
                    }
                });
            });
            
            scope.$on(scope.uidl.qualifiedName + ':ModelUpdater', function(event, updatedEntity) {
                for (var i = 0 ; i < scope.calculatedFields.length ; i++) {
                    var field = scope.calculatedFields[i];
                    scope.model.Data.entity[field.propertyName] = updatedEntity[field.propertyName];
                }
            });

            scope.$on(scope.uidl.qualifiedName + ':EditAuthorized', function(event, data) {
                scope.editAuthorized = true;
                Enumerable.From(scope.uidl.fields)
                    .Where("$.fieldType=='InlineGrid' || $.fieldType=='InlineForm' || $.fieldType=='LookupMany'")
                    .Where(function(f) { return (!data || !data.restrictedEntryProperties || data.restrictedEntryProperties[f.propertyName]); })
                    .ForEach(function (field) {
                        scope.$broadcast(field.nestedWidget.qualifiedName + ':EditAuthorized');
                    });
            });
            
            scope.$on(scope.uidl.qualifiedName + ':StateResetter', function (event, data) {
                if (data && data.commandInProgress) {
                    scope.commandInProgress = data.commandInProgress;
                } else {
                    scope.commandInProgress = false;
                    scope.tabSetIndex = 0;
                }
            });
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['Text'] = {
        implement: function (scope) {
            scope.model.State.Contents = scope.uidl.text;
            
            scope.$on(scope.uidl.qualifiedName + ':FormatSetter', function(event, data) {
                scope.model.State.Contents = data;
            });
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['JsonText'] = {
        implement: function (scope) {
            function valueTransform(key, value) {
                if (key && key.length > 0 && key.charAt(0) == '$') {
                    return undefined;
                }
                //if (typeof value === "string") {
                //    return value;
                //}
                return value;
            }
            
            function syntaxHighlight(json) {
                if (typeof json != 'string') {
                     json = JSON.stringify(json, valueTransform, 4);
                }
                json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
                var result = json.replace(
                    /("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, 
                    function (match) {
                        var cls = 'number';
                        if (/^"/.test(match)) {
                            if (/:$/.test(match)) {
                                cls = 'key';
                            } else {
                                cls = 'string';
                            }
                        } else if (/true|false/.test(match)) {
                            cls = 'boolean';
                        } else if (/null/.test(match)) {
                            cls = 'null';
                        }
                        return '<span class="' + cls + '">' + match + '</span>';
                    }
                );
                return result;
            }

            //scope.formattedHtml = syntaxHighlight(scope.parentModel);
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['TransactionForm'] = {
        implement: function (scope) {
            scope.model.State.Input = { };

            scope.$on(scope.uidl.qualifiedName + ':Loaded', function (event, data){ 
                var editAuthorizedData = {
                    create: true,
                    'delete': false,
                    enabledOperations: null,
                    fullEntity: null,
                    isRestrictedEntry: false,
                    restrictedEntryProperties: null,
                    retrieve: true,
                    update: true
                };
                scope.$broadcast(scope.uidl.inputForm.qualifiedName + ':EditAuthorized', editAuthorizedData);
            });

            scope.$on(scope.uidl.qualifiedName + ':ShowModal', function(event, data) {
                scope.commandInProgress = false;
            });
            
            scope.invokeCommand = function (command) {
                if (command.kind==='Submit') {
                    scope.commandInProgress = true;
                    var validationResult = { isValid: false };
                    scope.$broadcast(':global:FormValidating', validationResult);
                    $timeout(function() {
                        if (validationResult.isValid===true) {  
                            scope.$emit(command.qualifiedName + ':Executing');
                            if (!scope.uidl.outputForm) {
                                scope.$broadcast(scope.uidl.qualifiedName + ':HideModal');
                            }
                        } else {
                            scope.commandInProgress = false;
                        }
                    });
                } else {
                    scope.$broadcast(scope.uidl.qualifiedName + ':HideModal');
                }
            };

            scope.hideModal = function(command) {
                scope.$broadcast(scope.uidl.qualifiedName + ':HideModal');
            };
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['EntityMethodForm'] = {
        implement: function (scope) {
            scope.$on(scope.uidl.qualifiedName + ':ShowModal', function(event, data) {
                scope.commandInProgress = false;
                if (scope.model.State.Entity) {
                    scope.model.State.Input = { 
                        '$entityId' : scope.model.State.Entity['$id']
                    };
                    //scope.model.State.Input = scope.model.State.Input; 
                    scope.$broadcast(scope.uidl.inputForm.qualifiedName + ':ModelSetter', scope.model.State.Input);
                } else {
                    scope.$emit(scope.uidl.qualifiedName + ':NoEntityWasSelected');
                }
            });
            
            scope.invokeCommand = function (command) {
                if (command.kind==='Submit') {
                    scope.commandInProgress = true;
                    var validationResult = { isValid: false };
                    scope.$broadcast(':global:FormValidating', validationResult);
                    $timeout(function() {
                        if (validationResult.isValid===true) {  
                            scope.$emit(command.qualifiedName + ':Executing');
                            scope.$broadcast(scope.uidl.qualifiedName + ':HideModal');
                        } else {
                            scope.commandInProgress = false;
                        }
                    });
                } else {
                    scope.$broadcast(scope.uidl.qualifiedName + ':HideModal');
                }
            };
        }
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['LookupGrid'] = {
        implement: function (scope) {
            var metaType = scope.uidlService.getMetaType(scope.uidl.entityName);
            scope.metaType = metaType;

            if (scope.uidl.displayColumns && scope.uidl.displayColumns.length) {
                scope.gridColumns = scope.uidl.displayColumns;
            } else {
                scope.gridColumns = scope.uidl.defaultDisplayColumns;
            }

            scope.queryLookupRecords = function () {
                scope.lookupRecords = null;
                scope.entityService.queryEntity(scope.uidl.entityName, function(query) {
                    if (scope.uidl.queryFilter) {
                        var filterValueContext = createInputContext(scope.model, scope.parentModel);
                        query.applyUidlFilters(scope.uidlService, scope.uidl.queryFilter, filterValueContext);
                    }
                }).then(function (data) {
                    scope.lookupRecords = data.ResultSet;
                    var modelAsEnumerable = Enumerable.From(scope.model); 
                    for (var i = 0; i < scope.lookupRecords.length; i++) {
                        var record = scope.lookupRecords[i];
                        record.isChecked = modelAsEnumerable.Any("$ == '" + record['$id'] + "'");
                    }
                    $timeout(function () {
                        scope.$broadcast(scope.uidl.qualifiedName + ':DataReceived', scope.lookupRecords);
                    });
                });
            };

            scope.refresh = function () {
                scope.queryLookupRecords();
            };

            scope.updateCheckboxModel = function (entityId, isChecked) {
                //var entityId = scope.lookupRecords[rowIndex]['$id'];
                var model = scope.model;

                for (var i = model.length - 1; i >= 0; i--) {
                    if (model[i] === entityId) {
                        model.splice(i, 1);
                    }
                }

                if (isChecked) {
                    model.push(entityId);
                }
            };

            scope.$on(scope.uidl.qualifiedName + ':ModelSetter', function (event, data) {
                scope.model = data;
                scope.queryLookupRecords();
            });
            
            scope.$on(scope.uidl.qualifiedName + ':EditAuthorized', function(event, data) {
                scope.editAuthorized = true;
            });
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['TypeSelector'] = {
        implement: function (scope) {
            scope.selectedType = { name: null };
            scope.parentModelProperty = scope.uidl.parentModelProperty; //toCamelCase(scope.uidl.parentModelProperty);
            
            scope.selectedTypeChanged = function (type) {
                scope.entityService.newDomainObject(type).then(function (newObj) {
                    scope.model = {
                        entity: newObj
                    };

                    scope.selectedType.name = newObj['$type'];
                    scope.selectTabByType(scope.selectedType.name);

                    if (scope.parentModel) {
                        if (scope.parentUidl) {
                            // parent is FORM FIELD
                            scope.parentModel[scope.parentUidl.propertyName] = newObj;
                        } else {
                            // parent is CRUD
                            if (scope.uidl.parentInverseNavigationProperty) { // preserve foreign key value
                                var foreignKeyValue = scope.parentModel[scope.parentModelProperty][scope.uidl.parentInverseNavigationProperty];
                                newObj[scope.uidl.parentInverseNavigationProperty] = foreignKeyValue;
                            }
                            scope.parentModel[scope.parentModelProperty] = newObj;
                        }
                        scope.sendModelToSelectedWidget();
                    }
                });
            };

            scope.sendModelToSelectedWidget = function () {
                if (!scope.selectedType || !scope.selectedType.name) {
                    return;
                }
                var selection = Enumerable.From(scope.uidl.selections).Where("$.typeName=='" + scope.selectedType.name + "'").First();
                var selectedWidgetQualifiedName = selection.widget.qualifiedName;
                $timeout(function() {
                    scope.$broadcast(selectedWidgetQualifiedName + ':ModelSetter', scope.model.entity);
                    scope.$broadcast(selectedWidgetQualifiedName + ':EditAuthorized');
                });
            };

            scope.parentModelReceived = function() {
                if (scope.parentUidl) {
                    // parent is FORM FIELD
                    scope.model.entity = scope.parentModel[scope.parentUidl.propertyName];
                } else if (scope.parentModel) {
                    // parent is CRUD
                    scope.model.entity = scope.parentModel[scope.parentModelProperty];
                }

                if (scope.model.entity) {
                    scope.selectedType.name = scope.model.entity['$type'];
                    scope.selectTabByType(scope.selectedType.name);
                }
            };
            
            scope.selectTabByIndex = function(index) {
                if (index !== scope.selectedTabIndex) {
                    scope.selectedTabIndex = index;
                    scope.$emit(scope.uidl.qualifiedName + ':SelectionChanged', scope.uidl.selections[index]);
                    $timeout(function() {
                        scope.$broadcast(scope.uidl.selections[index].widget.qualifiedName + ':NavigatedHere');
                    });
                }
            }

            scope.selectTabByType = function(typeName) {
                var newTabIndex = Enumerable.From(scope.uidl.selections).Select('sel=>sel.typeName').IndexOf(typeName);
                scope.selectTabByIndex(newTabIndex >= 0 ? newTabIndex : 0);
            }
            
            scope.model = {
                entity: null
            };

            if (scope.parentModel) {
                scope.parentModelReceived();
            }

            scope.$on(scope.uidl.qualifiedName + ':ModelSetter', function (event, data) {
                if (data) {
                    if (scope.parentUidl) {
                        // parent is FORM FIELD
                        scope.parentModel[scope.parentUidl.propertyName] = data;
                    } else if (scope.parentModel) {
                        // parent is CRUD
                        scope.parentModel[scope.parentModelProperty] = data;
                    }
                    scope.parentModelReceived();
                    scope.sendModelToSelectedWidget();
                }
            });

            if (scope.model.entity && scope.model.entity['$type']) {
                scope.selectedType.name = scope.model.entity['$type'];
                scope.selectTabByType(scope.selectedType.name);
            } else if (scope.uidl.defaultTypeName) {
                scope.selectTabByType(scope.uidl.defaultTypeName);
            }
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['TabbedScreenPartSet'] = {
        implement: function (scope) {
            scope.selectedTabIndex = -1;
            scope.initializedTabIndexes = { };
            
            scope.selectTabByIndex = function(index) {
                if (index !== scope.selectedTabIndex) {
                    scope.selectedTabIndex = index;
                    
                    if (!scope.initializedTabIndexes[index]) {
                        scope.initializedTabIndexes[index] = true;
                        $timeout(function() {
                            scope.$broadcast(scope.uidl.tabs[index].qualifiedName + ':NavigatedHere');
                        });
                    }
                }
            }
            
            if (scope.uidl.tabs.length > 0) {
                scope.$timeout(function() {
                    scope.selectTabByIndex(0);
                });
            }
        }
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    m_controllerImplementations['TaskPadItem'] = {
        implement: function (scope) {
            scope.performTask = function(command) {
                scope.$emit(command.qualifiedName + ':Executing');
            };
        }
    };

    //-----------------------------------------------------------------------------------------------------------------

    return {
        getDocument: getDocument,
        setDocument: setDocument,
        getApp: getApp,
        getLocationSearchRaw: getLocationSearchRaw,
        getInitialScreenInput: getInitialScreenInput,
        getCurrentScreen: getCurrentScreen,
        getCurrentLocale: getCurrentLocale,
        getMetaType: getMetaType,
        getRelatedMetaType: getRelatedMetaType,
        //takeMessagesFromServer: takeMessagesFromServer,
        implementController: implementController,
        translate: translate,
        formatValue: formatValue,
        loadTemplateById: loadTemplateById,
		createInputContext: createInputContext,
        selectValue: selectValue,
        readValueByNavigationPath: readValueByNavigationPath,
        writeValueByNavigationPath: writeValueByNavigationPath
    };

}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.controller('cloneUidlWithUniqueId',
['$scope', 'uidlService',
function ($scope, uidlService) {
    $scope.cloneUidl = function(uidl, idSuffix) {
        $scope.thisUidl = jQuery.extend({}, uidl);
        $scope.thisUidl.idName = $scope.thisUidl.idName + '_' + idSuffix;
        $scope.thisUidl.qualifiedName = $scope.thisUidl.qualifiedName + '_' + idSuffix;
        $scope.thisUidl.elementName = $scope.thisUidl.elementName + '_' + idSuffix;
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.controller('appStart',
['$http', '$scope', '$rootScope', '$timeout', 'uidlService', 'entityService', 'commandService',
function ($http, $scope, $rootScope, $timeout, uidlService, entityService, commandService) {

	function completeInittialization() {
		uidlService.implementController($scope);

		$rootScope.currentScreen = uidlService.getCurrentScreen();
		$rootScope.currentLocale = uidlService.getCurrentLocale();
		$scope.pageTitle = $scope.translate($scope.app.text) + ' - ' + $scope.translate($scope.currentScreen.text);

		$timeout(function() {
			$rootScope.$broadcast($rootScope.currentScreen.qualifiedName + ':NavigatedHere', uidlService.getInitialScreenInput());
		});

        //commandService.startPollingMessages();
	}

    $scope.pageTitle = 'LOADING . . .';

    $http.get('app/uidl.json').then(function (httpResult) {
        uidlService.setDocument(httpResult.data);

		$rootScope.app = uidlService.getApp();
		$rootScope.uidl = uidlService.getApp();
		$rootScope.entityService = entityService;
		$rootScope.uidlService = uidlService;
		$rootScope.commandService = commandService;
		$rootScope.appScope = $scope;
        $scope.uidl = $rootScope.app;

        if ($scope.uidl.isUserAuthenticated===true) {
            $http.get('app/stateRestore').then(
                function(response) {
					$scope.model = { 
						Data: { },
						State: response.data
					}
                    $scope.$emit($scope.uidl.qualifiedName + ':UserAlreadyAuthenticated');
					completeInittialization();
                }
            );
        } else {
			completeInittialization();
		}
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
                
                var initFuncName = 'initWidget_Screen';
                var initFunc = window[initFuncName];
                if (typeof initFunc === 'function') {
                    initFunc($scope);
                }
            });
            //$scope.$on($scope.uidl.qualifiedName + ':NavigatingAway', function () {
            //    $scope.$destroy();
            //});
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
                
                if ($scope.uidl.$skin) {
                    $scope.uidl.contentRoot.$skin  = $scope.uidl.$skin;
                }
                
                var initFuncName = 'initWidget_ScreenPart';
                var initFunc = window[initFuncName];
                if (typeof initFunc === 'function') {
                    initFunc($scope);
                }
            });
            //$scope.$on($scope.uidl.qualifiedName + ':NavigatingAway', function () {
            //    $scope.$destroy();
            //});
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlWidget', 
['uidlService', 'entityService', 'commandService', '$timeout', '$http', '$compile', '$rootScope',
function (uidlService, entityService, commandService, $timeout, $http, $compile, $rootScope) {
    var uniqueWidgetId = 1;

    return {
        scope: {
            uidl: '=',
            parentUidl: '=',
            parentModel: '=?'
        },
        restrict: 'E',
        replace: true,
        link: function ($scope, elem, attrs) {
            $scope.uniqueWidgetId = 'uidlWidget' + uniqueWidgetId++;
        },
        template: '<ng-include src="\'app/uidl-element-template/\' + uidl.templateName"></ng-include>',
        controller: function ($scope) {
            $scope.$timeout = $timeout;
            $scope.$http = $http;
            $scope.$compile = $compile;
            $scope.$rootScope = $rootScope;
            $scope.uidlService = uidlService;
            $scope.entityService = entityService;
            $scope.commandService = commandService;
            $scope.inlineUserAlert = { current: null };
            
            $scope.parentFormFieldHasModifier = function(modifier) {
                if ($scope.parentUidl && $scope.parentUidl.modifiers) {
                    return hasEnumFlag($scope.parentUidl.modifiers, modifier);
                }
                return false;
            };
            
            $scope.isUidlAuthorized = function(uidlElement) {
                if (!uidlElement.authorization || uidlElement.authorization.requiredClaims.length == 0) {
                    return true;
                }
                try {
                    var userClaims = ($scope.appScope.model.State.LoggedInUser.AllClaims || $scope.appScope.model.State.LoggedInUser.LoginResult.AllClaims);
                    for (var i = 0 ; i < uidlElement.authorization.requiredClaims.length; i++) {
                        var claim = uidlElement.authorization.requiredClaims[i];
                        if (userClaims.indexOf(claim) >= 0) {
                            return true;
                        }
                    }
                } catch(ex) { }
                
                return false;
            };
            
            $scope.implementUidl = function() {
                if ($scope.uidl && !$scope.uidlWasImplemented) {
                    if (!$scope.uidl.$skin) {
                        if ($scope.parentUidl && $scope.parentUidl.$skin) {
                            $scope.uidl.$skin = $scope.parentUidl.$skin;
                        } else {
                            $scope.uidl.$skin = { };
                        }
                    }
                    
                    uidlService.implementController($scope);

                    var initFuncName = 'initWidget_' + $scope.uidl.widgetType;
                    var initFunc = window[initFuncName];
                    if (typeof initFunc === 'function') {
                        initFunc($scope);
                    }
            
                    $timeout(function() {
                        $scope.$emit($scope.uidl.qualifiedName + ':Loaded');
                    });
                    
                    $scope.uidlWasImplemented = true;
                }
            };
            
            //console.log('uidlWidget::controller', $scope.uidl.qualifiedName);
            //uidlService.implementController($scope);
            
            if ($scope.uidl) {
                $scope.implementUidl();
            } else {
                $scope.$watch('uidl', function (newValue, oldValue) {
                    console.log('uidlWidget::watch(uidl)', oldValue ? oldValue.qualifiedName : '0', '->', $scope.uidl ? $scope.uidl.qualifiedName : '0');
                    $scope.implementUidl();
                });
            }

            if ($scope.controllerInitCount) {
                $scope.controllerInitCount = $scope.controllerInitCount+1;
            } else {
                $scope.controllerInitCount = 1;
            }
            
            $scope.$on("$destroy", function() {
                if ($scope.uidl) {
                    console.log('uidlWidget::$destroy() - ', $scope.uidl.qualifiedName);
                } else {
                    console.log('uidlWidget::$destroy() - ??? ', uniqueWidgetId);
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
        templateUrl: 'app/uidl-element-template/GridField',
        controller: function ($scope) {
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlFormField',
['$timeout', '$rootScope', 'uidlService', 'entityService', '$http',
function ($timeout, $rootScope, uidlService, entityService, $http) {
    return {
        scope: {
            parentUidl: '=',
            uidl: '=',
            entity: '='
        },
        restrict: 'E',
        replace: true,
        link: function (scope, elem, attrs) {
        },
        templateUrl: function(elem, attrs) {
            return 'app/uidl-element-template/' + (attrs.templateName || 'FormField');
        },
        //templateUrl: 'uidl-element-template/' + parentUidl.fieldsTemplateName, 
        //template: '<ng-include src="\'uidl-element-template/\' + parentUidl.fieldsTemplateName"></ng-include>',
        controller: function ($scope) {
            $scope.uidlService = uidlService;
            $scope.entityService = entityService;
            $scope.$http = $http;
            $scope.uniqueFieldId = $scope.parentUidl.elementName + '_' + $scope.uidl.propertyName;
            $scope.translate = $scope.uidlService.translate;
            $scope.hasEnumFlag = hasEnumFlag;

            $scope.hasUidlModifier = function (modifier) {
                return ($scope.uidl.modifiers && $scope.uidl.modifiers.indexOf(modifier) > -1);
            };
            
            if ($scope.parentUidl.usePascalCase === false) {
                $scope.uidl.propertyName = toCamelCase($scope.uidl.propertyName);
            }

            if ($scope.uidl.groupIndex===0) {
                $scope.groupFields = Enumerable.From($scope.parentUidl.fields).Where(function(f) {
                    return (f !== $scope.uidl && f.groupId === $scope.uidl.groupId)
                }).OrderBy('$.groupIndex').ToArray();
            }
            
            if ($scope.uidl.fieldType==='Lookup') {
                if ($scope.uidl.lookupEntityName) {
                    var metaType = uidlService.getMetaType($scope.uidl.lookupEntityName);

                    $scope.lookupMetaType = metaType;
                    $scope.lookupValueProperty = ($scope.uidl.lookupValueProperty ? $scope.uidl.lookupValueProperty : '$id');
                    $scope.lookupTextProperty = ($scope.uidl.lookupDisplayProperty ? $scope.uidl.lookupDisplayProperty : metaType.defaultDisplayPropertyNames[0]);
                    $scope.lookupForeignKeyProperty = $scope.uidl.propertyName; // + '_FK';

                    $scope.isLoadingTypeAhead = false;
                    $scope.isTypeAheadResultSetEmpty = true;

                    $scope.loadTypeAhead = function(prefix) {
                        $scope.isLoadingTypeAhead = true;
                        $scope.entityService.queryEntity($scope.uidl.lookupEntityName, function(query) {
                            query.where($scope.lookupTextProperty, prefix, ':cn');
                        }).then(
                            function(data) {
                                $scope.lookupResultSet = data.ResultSet;
                                if ($scope.uidl.applyDistinctToLookup) {
                                    $scope.lookupResultSet = Enumerable.From($scope.lookupResultSet).Distinct('$.' + $scope.lookupTextProperty).ToArray();
                                }
                                $scope.isLoadingTypeAhead = false;
                                $scope.isTypeAheadResultSetEmpty = ($scope.lookupResultSet.length===0);
                                return $scope.lookupResultSet;
                            },
                            function(faultResponse) {
                                $scope.isLoadingTypeAhead = false;
                                $scope.isTypeAheadResultSetEmpty = true;
                            }
                        );
                    }
                    
                    $scope.formatTypeAheadItem = function(model) {
                        return model[$scope.lookupTextProperty];
                    }
                    
                    $scope.ellipsisClicked = function() {
                        var lookupContext = {
                            targetEntity: $scope.entity,
                            targetProperty: $scope.lookupForeignKeyProperty,
                            lookupMetaType: $scope.lookupMetaType,
                            lookupTextProperty: $scope.lookupTextProperty,
                            lookupValueProperty: $scope.lookupValueProperty,
                            dataGridUidl: $scope.uidl.nestedWidget
                        };
                        $rootScope.$broadcast(':global:AdvancedLookupRequest', lookupContext);
                    };
 
                    if ($scope.hasUidlModifier('DropDown') && !$scope.hasUidlModifier('TypeAhead')) {
                        $scope.entityService.queryEntity($scope.uidl.lookupEntityName, function(query) {
                            if ($scope.uidl.lookupQueryFilter) {
                                var filterValueContext = $scope.uidlService.createInputContext($scope.model, $scope.entity);
                                query.applyUidlFilters($scope.uidlService, $scope.uidl.lookupQueryFilter, filterValueContext);
                            }
                        }).then(function(data) {
                            $scope.lookupResultSet = data.ResultSet;

                            if ($scope.uidl.applyDistinctToLookup) {
                                $scope.lookupResultSet = Enumerable.From($scope.lookupResultSet).Distinct('$.' + $scope.lookupTextProperty).ToArray();
                            }
                        });
                    }

                    if ($scope.hasUidlModifier('Ellipsis')) {
                        $scope.getLookupDisplayText = function() {
                            if ($scope.displayName) {
                                var idText = $scope.entity[$scope.uidl.propertyName] + ' | ';
                                if (idText.length < 15) {
                                    return idText + $scope.displayName;
                                }
                            }
                            if ($scope.entity) {
                                return $scope.entity[$scope.uidl.propertyName];
                            }
                            return null;
                        };
                        
                        $scope.$watch('entity.' + $scope.uidl.propertyName, function(newValue, oldValue) {
                            $scope.displayName = null;
                            if (newValue) {
                                var query = new EntityQueryBuilder($scope.uidl.lookupEntityName);
                                query.select($scope.lookupTextProperty);
                                query.where($scope.lookupValueProperty, newValue);
                                $http.get(query.getQueryUrl()).then(function(response) {
                                    if (response.data.ResultSet && response.data.ResultSet.length == 1) {
                                        $scope.displayName = response.data.ResultSet[0][$scope.lookupTextProperty];
                                    }
                                });
                            }
                        });
                    }
                } else if ($scope.uidl.standardValues || $scope.uidl.lookupSourceProperty) {
                    var onLookupSourceAvailable = function(source) {
                        $scope.lookupValueProperty = 'id';
                        $scope.lookupTextProperty = 'text';
                        $scope.lookupForeignKeyProperty = $scope.uidl.propertyName;
                        $scope.lookupResultSet = [];

                        for (var i = 0; i < source.length; i++) {
                            var value = source[i];
                            $scope.lookupResultSet.push({
                                id: value,
                                text: uidlService.translate(value)
                            });
                        }
                    }; 

                    if ($scope.uidl.standardValues) {
                        onLookupSourceAvailable($scope.uidl.standardValues);
                    } else if($scope.entity) {
                        onLookupSourceAvailable($scope.entity[$scope.uidl.lookupSourceProperty]);
                    } else {
                        $scope.$watch('entity', function(newValue, oldValue) {
                            if (newValue) {
                                onLookupSourceAvailable(newValue[$scope.uidl.lookupSourceProperty]);
                            }
                        });
                    }
                }
            }

            $scope.hiddenValues = { };
            $scope.$on($scope.parentUidl.qualifiedName + ':ModelSetter', function(event, data) {
                $scope.hiddenValues = { };
                if ($scope.uidl.initialValue && data && !data[$scope.uidl.propertyName]) {
                    data[$scope.uidl.propertyName] = $scope.uidl.initialValue;
                }
            });
            
            $scope.editAuthorized = ($scope.parentUidl.needsAuthorize ? false : true);
            if (!$scope.editAuthorized) {
                $scope.$on($scope.parentUidl.qualifiedName + ':EditAuthorized', function(event, data) {
                    $scope.editAuthorized = (!data || !data.restrictedEntryProperties || data.restrictedEntryProperties[$scope.uidl.propertyName]);
                });
            }
            
            $timeout(function () {
                var initFuncName = 'initWidget_FormField';
                var initFunc = window[initFuncName];
                if (typeof initFunc === 'function') {
                    try {
                        initFunc($scope);
                    } catch(e) {
                        console.log('ERROR!', e);
                    }
                }
            });
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlEllipsisLookupSearch',
['$timeout', '$rootScope', 'uidlService', 'entityService',
function ($timeout, $rootScope, uidlService, entityService) {
    return {
        scope: {
        },
        restrict: 'E',
        replace: true,
        link: function (scope, elem, attrs) {
        },
        templateUrl: function(elem, attrs) {
            return 'app/uidl-element-template/' + (attrs.templateName || 'EllipsisLookupSearchModal');
        },
        controller: function ($scope) {
            $scope.$timeout = $timeout;
            
            $scope.invokeInitFunc = function() {
                var initFuncName = 'initWidget_EllipsisLookupSearchModal';
                var initFunc = window[initFuncName];
                if (typeof initFunc === 'function') {
                    initFunc($scope);
                } else {
                    $timeout($scope.invokeInitFunc);
                }
            };
            
            $scope.$on(':global:AdvancedLookupRequest', function(event, data) {
                $scope.lookupContext = data;
                $scope.showLookupSearchModal();
            });

            $scope.lookupObjectSelected = function(selectedObject) {
                $scope.hideLookupSearchModal();
                $scope.lookupContext.targetEntity[$scope.lookupContext.targetProperty] = selectedObject[$scope.lookupContext.lookupValueProperty] || selectedObject['$id'];
                $scope.lookupContext = null;
            };

            $scope.clearSelection = function() {
                $scope.hideLookupSearchModal();
                $scope.lookupContext.targetEntity[$scope.lookupContext.targetProperty] = null;
                $scope.lookupContext = null;
            };

            $scope.invokeInitFunc();
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.controller('formNestedObject',
['$scope', 'entityService', 'commandService', 'uidlService',
function ($scope, entityService, commandService, uidlService) {
    
    $scope.isNullable = (
        $scope.fieldHasModifier($scope.field, 'Nullable') && 
        $scope.field.fieldType === 'InlineForm' && 
        $scope.field.nestedWidget &&
        $scope.field.nestedWidget.widgetType === 'Form' && 
        $scope.field.nestedWidget.entityName 
        ? true
        : false);
    
    $scope.nestedObjectExists = function(toggle) {
        if (!$scope.isNullable) {
            return true;
        }
        
        if (!$scope.model.Data.entity) {
            return false;
        }

        if (arguments.length) {
            if (toggle === true) {
                entityService.newDomainObject($scope.field.nestedWidget.entityName).then(
                    function (newObj) {
                        $scope.model.Data.entity[$scope.field.propertyName] = newObj;
                        $scope.$broadcast($scope.field.nestedWidget.qualifiedName + ':ModelSetter', newObj);
                        $scope.$broadcast($scope.field.nestedWidget.qualifiedName + ':EditAuthorized');
                    },
                    function (fault) {
                        scope.$emit($scope.uidl.qualifiedName + ':NewDomainObjectFailed', commandService.createFaultInfo(fault));
                    }
                );
            } else if (toggle === false) {
                $scope.model.Data.entity[$scope.field.propertyName] = null;
            }
        }
        
        return ($scope.model.Data.entity[$scope.field.propertyName] ? true : false);
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

            $scope.getDisplayText = function() {
                if ($scope.alert.current.faultInfo && $scope.alert.current.faultInfo.faultCode && $scope.alert.current.faultInfo.faultCode.length > 0) {
                    return $scope.translate($scope.alert.current.faultInfo.faultCode);
                } else {
                    return $scope.translate($scope.alert.current.text);
                }
            }

            $scope.answerAlert = function(choice) {
                $scope.alert.current.answer(choice);
                $scope.alert.current = null;
            };
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
		templateUrl: 'app/uidl-element-template-report-lookup',
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

theApp.directive('applyAutoFillFix', ['$timeout', function ($timeout) {
    return {
        require: 'ngModel',
        link: function (scope, elem, attrs, ctrl) {
            scope.check = function(){
                var val = elem[0].value;
                if(ctrl.$viewValue !== val){
                    ctrl.$setViewValue(val)
                }
                $timeout(scope.check, 300);
            };
            scope.check();
        }
    }
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('errSrc', function() {
    return {
        link: function(scope, element, attrs) {
            element.bind('error', function() {
                if (attrs.src != attrs.errSrc) {
                    attrs.$set('src', attrs.errSrc);
                }
            });
        }
    }
});

//---------------------------------------------------------------------------------------------------------------------

theApp.directive('uidlDisplayFormat', ['uidlService', function (uidlService) {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            ngModel.$formatters.push(formatter);
            ngModel.$parsers.push(parser);

            //element.on('change', function (e) {
            //    var target = e.target;
            //    target.value = formatter(ngModel.$modelValue);
            //});

            function parser(value) {
                return value;
            }

            function formatter(value) {
                return uidlService.formatValue(scope.uidl.format, value);
            }
        }
    };
}]);

//---------------------------------------------------------------------------------------------------------------------

theApp.controller('menuSubItemsExpander', 
['$scope', '$q', 'uidlService', 'entityService', 
function ($scope, $q, uidlService, entityService) {
    var newSubItems = [ ];
    
    for (var i = 0 ; i < $scope.item.subItems.length ; i++) {
        var subItem = $scope.item.subItems[i];
        if (subItem.repeater && !subItem.$previewed) {
            newSubItems.push({
                $previewed: true,
                $expanded: false,
                $source: subItem,
                icon: 'refresh',
                text: 'Loading...',
                subItems: [ ],
            });
        } else {
            subItem.$previewed = true;
            subItem.$expanded = true;
            newSubItems.push(subItem);
        }
    }
    
    $scope.item.subItems = newSubItems;

    $scope.toggleSubItems = function() {
        var subItemsArray = $scope.item.subItems;
        
        for (var i = 0 ; i < subItemsArray.length ; i++) {
            var subItem = subItemsArray[i];
            
            if (subItem.$previewed && !subItem.$expanded) {
                $scope.beginExpandSubItem(subItem).then(function(realSubItems) {
                    subItemsArray.splice(i, 1);
                    for (var j = 0; j < realSubItems.length ; j++) {
                        subItemsArray.splice(i + j, 0, realSubItems[j]);
                    }
                });
                break;
            }
        }
    };
    
    $scope.beginExpandSubItem = function(subItem) {
        var templateItem = subItem.$source;
        switch (templateItem.repeater) {
            case 'Query': 
                return entityService.queryEntity(templateItem.repeaterQueryEntityName, function(query) {
                    if (templateItem.repeaterQueryDisplayProperty) {
                        query.select(templateItem.repeaterQueryDisplayProperty);
                    }
                    if (templateItem.repeaterQueryValueProperty) {
                        query.select(templateItem.repeaterQueryValueProperty);
                    }
                }).then(
                    function(data) {
                        var realSubItems = [ ];
                        var records = data.ResultSet;
                        for (var i = 0; i < records.length; i++) {
                            var realItem = $.extend(true, { }, templateItem);
                            realItem.text = '' + (templateItem.repeaterQueryDisplayProperty ? 
                                records[i][templateItem.repeaterQueryDisplayProperty] : 
                                templateItem.text /*records[i]['$type']*/);
                            realItem.value = '' + (templateItem.repeaterQueryValueProperty ? 
                                records[i][templateItem.repeaterQueryValueProperty] : 
                                records[i]['$id']);
                            realSubItems.push(realItem);
                        }
                        return realSubItems;
                    },
                    function (fault) {
                        return $q.reject(fault);
                    }
                );
            case 'Static': 
                var realSubItems = [ ];
                for (var p in templateItem.repeaterStaticValues) {
                    if (templateItem.repeaterStaticValues.hasOwnProperty(p) && p.length > 0 && p.charAt(0) != '$') {
                        var realItem = $.extend(true, { }, templateItem);
                        realItem.text = p;
                        realItem.value = templateItem.repeaterStaticValues[p];
                        realSubItems.push(realItem);
                    }
                }
                return $q.when(realSubItems);
        }
        
        return [ ];
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
        if (!items) {
            return items;
        }
        return items.slice().reverse();
    };
});

//---------------------------------------------------------------------------------------------------------------------

theApp.filter('twoColumnRows', function () {
    return function (items) {
        var rows = [];
        var rowCount = Math.floor(items.length / 2) + (items.length % 2);
        for (var i = 0; i < rowCount; i++) {
            rows.push(items[i]);
            if (rowCount + i < items.length) {
                items[i]['$nextCol'] = items[rowCount + i];
            }
        }
        return rows;
    };
});

//---------------------------------------------------------------------------------------------------------------------

theApp.filter('translated', ['uidlService', function(uidlService) {
    return function(s) {
        return uidlService.translate(s);
    };
}]);
