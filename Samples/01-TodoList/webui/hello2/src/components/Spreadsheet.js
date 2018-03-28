import React, { Component } from 'react';
import PropTypes from 'prop-types'
import { combineReducers } from 'redux'
import { connect } from 'react-redux'

export const SPREADSHEET_COLUMN_TYPE_KEY = 'KEY'
export const SPREADSHEET_COLUMN_TYPE_INPUT = 'INPUT'
export const SPREADSHEET_COLUMN_TYPE_ORDER = 'ORDER'
export const SPREADSHEET_COLUMN_TYPE_COMMAND = 'COMMAND'
export const SPREADSHEET_COLUMN_TYPE_STATUS = 'STATUS'

const ACTION_ROW_APPEND = 'SPREADSHEET__ROW_APPEND'

export const UIReducer = (state = { items: [], nextKey: 1 }, action) => {
    switch (action.type) {
        case ACTION_ROW_APPEND:
            let nextItems = state.items.slice()
            nextItems.push({ key: state.nextKey, value: 'Value#' + state.nextKey })
            const nextState = {
                ...state,
                nextKey: state.nextKey + 1,
                items: nextItems
            }
            return nextState
        default:
            return state
    }
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
    theme: PropTypes.object.isRequired
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

