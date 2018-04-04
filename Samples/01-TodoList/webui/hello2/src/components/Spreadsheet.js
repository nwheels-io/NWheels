import React, { Component } from 'react';
import PropTypes from 'prop-types'
import { combineReducers } from 'redux'
import { connect } from 'react-redux'
import { AbstractDataSource } from './DataSource'

export const SPREADSHEET_COLUMN_TYPE_KEY = 'KEY'
export const SPREADSHEET_COLUMN_TYPE_INPUT = 'INPUT'
export const SPREADSHEET_COLUMN_TYPE_ORDER = 'ORDER'
export const SPREADSHEET_COLUMN_TYPE_COMMAND = 'COMMAND'
export const SPREADSHEET_COLUMN_TYPE_STATUS = 'STATUS'

const ACTION_GET_ALL           = 'SPREADSHEET_GET_ALL'
const ACTION_GET_ALL_BEGAN     = 'SPREADSHEET_GET_ALL_BEGAN'
const ACTION_GET_ALL_COMPLETED = 'SPREADSHEET_GET_ALL_COMPLETED'
const ACTION_GET_ALL_FAILED    = 'SPREADSHEET_GET_ALL_FAILED'

const ACTION_VALUE_SET         = 'SPREADSHEET_VALUE_SET'

const ACTION_SAVE_BEGAN        = 'SPREADSHEET_SAVE_BEGAN'
const ACTION_SAVE_COMPLETED    = 'SPREADSHEET_SAVE_COMPLETED'
const ACTION_SAVE_FAILED       = 'SPREADSHEET_SAVE_FAILED'

const ACTION_DELETE_BEGAN      = 'SPREADSHEET_DELETE_BEGAN'
const ACTION_DELETE_COMPLETED  = 'SPREADSHEET_DELETE_COMPLETED'
const ACTION_DELETE_FAILED     = 'SPREADSHEET_DELETE_FAILED'

class UIStateMachine {
    static SPREADSHEET_GET_ALL(state, { dataSource }) {
        return (dispatch) => {
            dispatch({type: ACTION_GET_ALL_BEGAN})
            dataSource.getAll()
                .then(result => {
                    dispatch({
                        type: ACTION_GET_ALL_COMPLETED, 
                        data: result.records
                    })
                })
                .catch(error => {
                    dispatch({
                        type: ACTION_GET_ALL_FAILED, 
                        error
                    })
                })
        }
    }
    static SPREADSHEET_GET_ALL_BEGAN(state) {
        return {
            ...state,
            isReady: false
        }
    }
    static SPREADSHEET_GET_ALL_FAILED(state, { error }) {
        return {
            ...state,
            isReady: false
        }
    }
}

export const UIReducer = (state = { rows: [], isReady: false }, action) => {
    if (UIStateMachine[action.type]) {
        return UIStateMachine[action.type](state, action)
    }
    return state
}

export class Column extends React.Component {
    constructor(props) {
        super(props)
        this._title = props.title
        this._type = props.type
        this._width = props.width
        this._field = props.field
    }
    render() {
        return null
    }
    get title() { return this._title }
    get type() { return this._type }
    get width() { return this._width }
    get field() { return this._field }
}

export class UIProps {
    constructor(children, items) {
        this._items = items
        this._columns = React.Children
            .toArray(children)
            .map(child => new Column(child.props))
    }
    get items() {
        return this._items
    }
    get columns() {
        return this._columns
    }
}

export class UIActions {
    constructor(dispatch) {
        this._dispatch = dispatch
    }
    appendRow() {
        this._dispatch( {
            type: ACTION_ROW_APPEND
        })
    }
}

const UIView = ({ props, actions }, context) => {
    return context.theme.render.Spreadsheet(props, actions, context)
}

UIView.propTypes = {
    props: PropTypes.instanceOf(UIProps).isRequired,
    actions: PropTypes.instanceOf(UIActions).isRequired
}

UIView.contextTypes = {
    theme: PropTypes.object.isRequired,
    dataSource: PropTypes.instanceOf(AbstractDataSource)
}

const mapStateToProps = (state, ownProps) => {
    return {
        props: new UIProps(ownProps.children, state.items)
    }
}

const mapDispatchToProps = (dispatch, ownProps) => {
    return {
        actions: new UIActions(dispatch)
    }
}

export const UIComponent = connect(
    mapStateToProps,
    mapDispatchToProps
)(UIView)

