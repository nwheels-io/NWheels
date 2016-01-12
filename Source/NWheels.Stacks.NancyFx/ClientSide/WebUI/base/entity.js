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
        checkAuthorization: checkAuthorization,
        queryEntity: queryEntity,
        storeEntity: storeEntity,
        deleteEntity: deleteEntity,
        trackRetrievedEntities: trackRetrievedEntities,
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

    function checkAuthorization(typeName) {
        return $http.get('entity/checkAuth/' + typeName).then(
            function (response) {
                return {
                    create: response.data.CanCreate,
                    retrieve: response.data.CanRetrieve,
                    update: response.data.CanUpdate,
                    'delete': response.data.CanDelete,
                };
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
                    trackRetrievedEntities(response.data.ResultSet);
                    // for (var i = 0; i < response.data.ResultSet.length; i++) {
                        // var entity = response.data.ResultSet[i];
                        // entity['$state'] = 'RetrievedModified';
                    // }
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

    //-----------------------------------------------------------------------------------------------------------------
    
    function trackRetrievedEntities(resultSet) {
        for (var i = 0; i < resultSet.length; i++) {
            var entity = resultSet[i];
            entity['$state'] = 'RetrievedModified';
        }
    }
}]);

//---------------------------------------------------------------------------------------------------------------------

function EntityQueryBuilder(entityName, commandUrl) {

    var me = this;

    //-----------------------------------------------------------------------------------------------------------------

    this._entityName = entityName;
    this._commandUrl = commandUrl || null;
    this._entityTypeFilter = null;
    this._filter = [];
    this._orderBy = [];
    this._select = [];
    this._include = [];
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

    this.where = function (property, value, operator) {
        me._filter.push({ 
            property: property,
            value: value,
            operator: operator || ':eq'
        });
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

    this.select = function(property, aggregation) {
        me._select.push(property + (aggregation ? '!' + aggregation : ''));
        return me;
    };

    //-----------------------------------------------------------------------------------------------------------------

    this.include = function(property, aggregation) {
        me._include.push(property + (aggregation ? '!' + aggregation : ''));
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
        var delimiter = ((me._commandUrl && me._commandUrl.indexOf('?') > -1) ? '&' : '?');   

        if (me._entityTypeFilter) {
            queryString = queryString + delimiter + '$type=' + me._entityTypeFilter;
            delimiter = '&';
        }

        for (var i = 0; i < me._filter.length; i++) {
            var filterItem = me._filter[i];
            queryString = 
                queryString + 
                delimiter + 
                encodeURIComponent(filterItem.property) + ':' + filterItem.operator + '=' + encodeURIComponent(filterItem.value);
            delimiter = '&';
        }

        for (var i = 0; i < me._orderBy.length; i++) {
            var item = me._orderBy[i];
            var direction = (item.ascending ? ':asc' : ':desc');
            queryString = queryString + delimiter + '$orderby=' + encodeURIComponent(item.propertyName) + direction;
            delimiter = '&';
        }

        if (me._select.length > 0) {
            var selectSpec = '';
            for (var i = 0; i < me._select.length; i++) {
                selectSpec = selectSpec + (i > 0 ? ',' : '') + me._select[i];
            }
            queryString = queryString + delimiter + '$select=' + selectSpec;
            delimiter = '&';
        }
        
        if (me._include.length > 0) {
            var includeSpec = '';
            for (var i = 0; i < me._include.length; i++) {
                includeSpec = includeSpec + (i > 0 ? ',' : '') + me._include[i];
            }
            queryString = queryString + delimiter + '$include=' + includeSpec;
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
        var url = '';
        if (me._commandUrl) {
            url = me._commandUrl + me.getQueryString();
        } else {
            url = 'entity/query/' + me._entityName + me.getQueryString();
        }
        return url;
    }

    //-----------------------------------------------------------------------------------------------------------------

    this.clone = function () {
        var cloned = new EntityQueryBuilder(me._entityName, me._commandUrl);

        cloned._entityTypeFilter = me._entityTypeFilter;
        cloned._take = me._take;
        cloned._skip = me._skip;
        cloned._page = me._page;
        cloned._isCountOnly = me._isCountOnly;

        cloned._filter = [];
        for (var i = 0; i < me._filter.length; i++) {
            cloned._filter.push({
                property: me._filter[i].property,
                value: me._filter[i].value,
                operator: me._filter[i].operator
            });
        }

        cloned._orderBy = [];
        for (var i = 0; i < me._orderBy.length; i++) {
            cloned._orderBy.push({
                propertyName: me._orderBy[i].propertyName,
                ascending: me._orderBy[i].ascending
            });
        }

        return cloned;
    }
};
