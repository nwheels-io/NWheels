'use strict';

//-----------------------------------------------------------------------------------------------------------------

var theApp = angular.module('theApp');

//-----------------------------------------------------------------------------------------------------------------

theApp.service('entityService',
['$http', '$q', '$timeout',
function ($http, $q, $timeout) {

    var service = {
        newDomainObject: newDomainObject,
        newQueryBuilder: newQueryBuilder,
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
            function(fault) {
                return $q.reject(fault);
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
            function (fault) {
                return $q.reject(fault);
            }
        );
    }

    //-----------------------------------------------------------------------------------------------------------------

    function newQueryBuilder(typeName) {
        var queryBuilder = new EntityQueryBuilder(typeName);
        return queryBuilder;
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
            function (fault) {
                return $q.reject(fault);
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
            function (fault) {
                return $q.reject(fault);
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
    this._take = null;
    this._skip = null;
    this._page = null;
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

    this.take = function(value) {
        me._take = value;
        return me;
    };

    //-----------------------------------------------------------------------------------------------------------------

    this.skip = function(value) {
        me._skip = value;
        return me;
    };

    //-----------------------------------------------------------------------------------------------------------------

    this.page = function(value) {
        me._page = value;
        return me;
    };

    //-----------------------------------------------------------------------------------------------------------------

    this.count = function () {
        me._isCountOnly = true;
        return me;
    };
    
    //-----------------------------------------------------------------------------------------------------------------

    this.getQueryString = function () {
        var queryString = '';
        var delimiter = '?';

        if (me._entityTypeFilter) {
            queryString = queryString + delimiter + '$type=' + me._entityTypeFilter;
            delimiter = '&';
        }

        for (var property in me._equalityFilter) {
            if (me._equalityFilter.hasOwnProperty(property)) {
                var value = me._equalityFilter[property];
                queryString = queryString + delimiter + encodeURIComponent(property) + '=' + encodeURIComponent(value);
                delimiter = '&';
            }
        }

        for (var i = 0; i < me._orderBy.length; i++) {
            var item = me._orderBy[i];
            var direction = (item.ascending ? ':asc' : ':desc');
            queryString = queryString + delimiter + '$orderby=' + encodeURIComponent(item.propertyName + direction);
            delimiter = '&';
        }

        if (me._skip) {
            queryString = queryString + delimiter + '$skip=' + me._skip;
            delimiter = '&';
        }

        if (me._take) {
            queryString = queryString + delimiter + '$take=' + me._take;
            delimiter = '&';
        }

        if (me._page) {
            queryString = queryString + delimiter + '$page=' + me._page;
            delimiter = '&';
        }

        if (me._isCountOnly) {
            queryString = queryString + delimiter + '$count';
            delimiter = '&';
        }

        return queryString;
    };

    //-----------------------------------------------------------------------------------------------------------------

    this.getQueryUrl = function () {
        var url = 'entity/query/' + me._entityName + me.getQueryString();
        return url;
    }
};
