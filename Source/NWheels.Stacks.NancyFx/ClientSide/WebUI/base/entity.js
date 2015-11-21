'use strict';

//-----------------------------------------------------------------------------------------------------------------

var theApp = angular.module('theApp');

//-----------------------------------------------------------------------------------------------------------------

theApp.service('entityService',
['$http', '$q', '$timeout',
function ($http, $q, $timeout) {

    var service = {
        newDomainObject: newDomainObject,
        queryEntity: queryEntity,
        storeEntity: storeEntity,
        deleteEntity: deleteEntity
    };

    return service;

    //-----------------------------------------------------------------------------------------------------------------

    function newDomainObject(typeName) {
        return $http.get('entity/new/' + typeName).then(
            function (response) {
                response.data['$state'] = 'NewModified';
                return response.data;
            },
            function(response) {
                return response;
            }
        );
    }

    //-----------------------------------------------------------------------------------------------------------------

    function queryEntity(typeName, queryBuilderCallback) {
        var queryBuilder = new EntityQueryBuilder(typeName);

        if (queryBuilderCallback) {
            queryBuilderCallback(queryBuilder);
        }

        var url = queryBuilder.getQueryUrl();

        return $http.get(url).then(
            function (response) {
                if (response.data.ResultSet) {
                    for (var i = 0; i < response.data.ResultSet.length; i++) {
                        var entity = response.data.ResultSet[i];
                        entity['$state'] = 'RetrievedModified';
                    }
                }
                return response.data;
            },
            function (response) {
                return response;
            }
        );
    }

    //-----------------------------------------------------------------------------------------------------------------

    function storeEntity(entity) {
        var entityName = entity['$type'];
        var entityId = entity['$id'];
        var entityState = entity['$state'];

        var url = 
            'entity/store/' + entityName + 
            '?EntityId=' + encodeURIComponent(entityId) + 
            '&EntityState=' + encodeURIComponent(entityState);

        return $http.post(url, entity).then(
            function (response) {
                return response.data;
            },
            function (response) {
                
                return response;
            }
        );
    }

    //-----------------------------------------------------------------------------------------------------------------

    function deleteEntity(entity) {
        var entityName = entity['$type'];
        var entityId = entity['$id'];

        var url =
            'entity/delete/' + entityName +
            '?EntityId=' + encodeURIComponent(entityId);

        return $http.post(url).then(
            function (response) {
                return response.data;
            },
            function (response) {
                return response;
            }
        );
    }
}]);

//---------------------------------------------------------------------------------------------------------------------

function EntityQueryBuilder(entityName) {

    var me = this;

    //-----------------------------------------------------------------------------------------------------------------

    this._entityName = entityName;
    this._entityTypeFilter = null;
    this._equalityFilter = {};
    this._orderBy = [];
    this._maxCount = null;
    this._isCountOnly = false;

    //-----------------------------------------------------------------------------------------------------------------

    this.ofType = function (typeName) {
        me._entityTypeFilter = typeName;
        return me;
    };

    //-----------------------------------------------------------------------------------------------------------------

    this.whereEquals = function (property, value) {
        me._equalityFilter[property] = value;
        return me;
    };

    //-----------------------------------------------------------------------------------------------------------------

    this.orderBy = function(property, ascending) {
        var item = {
            propertyName: property,
            ascending: (ascending === undefined ? true : ascending === true)
        };
        me._orderBy.push(item);
        return me;
    };

    //-----------------------------------------------------------------------------------------------------------------

    this.top = function(value) {
        me._maxCount = value;
        return me;
    };

    //-----------------------------------------------------------------------------------------------------------------

    this.count = function () {
        me._isCountOnly = true;
        return me;
    };

    //-----------------------------------------------------------------------------------------------------------------

    this.getQueryUrl = function () {
        var url = 'entity/query/' + me._entityName;
        var delimiter = '?';

        if (me._entityTypeFilter) {
            url = url + delimiter + '$type=' + me._entityTypeFilter;
            delimiter = '&';
        }

        for (var property in me._equalityFilter) {
            if (me._equalityFilter.hasOwnProperty(property)) {
                var value = me._equalityFilter[property];
                url = url + delimiter + encodeURIComponent(property) + '=' + encodeURIComponent(value);
                delimiter = '&';
            }
        }

        for (var i = 0; i < me._orderBy.length; i++) {
            var item = me._orderBy[i];
            var direction = (item.ascending ? ':asc' : ':desc');
            url = url + delimiter + '$orderby=' + encodeURIComponent(item.propertyName + direction);
            delimiter = '&';
        }

        if (me._maxCount) {
            url = url + delimiter + '$top=' + me._maxCount;
            delimiter = '&';
        }

        if (me._isCountOnly) {
            url = url + delimiter + '$count';
            delimiter = '&';
        }

        return url;
    };
};
