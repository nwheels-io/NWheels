import React, { Component } from 'react';
import PropTypes from 'prop-types'
import { combineReducers } from 'redux'
import { connect } from 'react-redux'

export const DATA_SOURCE_TYPE_SERVER = 'SERVER'
export const DATA_SOURCE_TYPE_CLIENT = 'CLIENT'

export class AbstractDataSource {
    constructor() {
        if (this.constructor === AbstractDataSource) {
            throw new Error("Abstraction not implemented");
        }
    }
    getAll() {
        throw new Error("Abstraction not implemented");
    }
    save(entity) {
        throw new Error("Abstraction not implemented");
    }
    delete(entity) {
        throw new Error("Abstraction not implemented");
    }
}

class ServerDataSource extends AbstractDataSource {
    constructor(entityName) {
        this._entityName = entityName
        this._serverCallCount = 0
        this._nextKey = 789
    }
    getAll() {
        this._serverCallCount++
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                if ((this._serverCallCount % 4) === 0) {
                    reject(new Error('Internal server error'))
                } else {
                    resolve({
                        metadata: {
                            sourceTyoe: DATA_SOURCE_TYPE_SERVER,
                            serviceName: 'TodoService',
                            entityName: this._entityName,
                            more: false
                        },
                        records: [
                            { key: 123, text: "The first item", done: false, order: 222 },
                            { key: 456, text: "The second item", done: true, order: 111 }
                        ]
                    })
                }
            }, 3000)
        })
    }
    save(entity) {
        this._serverCallCount++
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                if ((this._serverCallCount % 4) === 0) {
                    reject(new Error('Internal server error'))
                } else {
                    if (!entity.key) {
                        entity.key = this._nextKey++
                    }
                    resolve({
                        records: [
                            entity
                        ]
                    })
                }
            }, 3000)
        })
    }
    delete(entity) {
        if (!entity.key) {
            return Promise.resolve()
        }
        this._serverCallCount++
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                if ((this._serverCallCount % 4) === 0) {
                    reject(new Error('Internal server error'))
                } else {
                    resolve()
                }
            }, 3000)
        })
    }
}

export class DataSource extends React.Component {
    constructor(props) {
        super(props)
    }
    getChildContext() {
        return {
            dataConnection: this.createConcreteDataSource()
        };
    }
    render() {
        return this.props.children
    }
    createConcreteDataSource() {
        return new ServerDataSource(this.props.entity)
    }
}

DataSource.propTypes = {
    type: PropTypes.oneOf([DATA_SOURCE_TYPE_CLIENT, DATA_SOURCE_TYPE_SERVER]).isRequired,
    entity: PropTypes.string
}

DataSource.childContextTypes = {
    dataSource: PropTypes.instanceOf(AbstractDataSource).isRequired
}
