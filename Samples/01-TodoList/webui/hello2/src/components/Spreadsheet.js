import React, { Component } from 'react';
import PropTypes from 'prop-types'
import { combineReducers } from 'redux'
import { connect } from 'react-redux'

const ACTION_ROW_APPEND = 'SPREADSHEET__ROW_APPEND'

const actionRowAppend = () => {
    return { type: ACTION_ROW_APPEND }
}

const mapStateToProps = (state, ownProps) => {
    return {
        items: state.items
    }
}

const mapDispatchToProps = (dispatch, ownProps) => {
    return {
        appendRow: () => {
            dispatch(actionRowAppend())
        }
    }
}

export const UIReducer = (state = { items: [], nextKey: 1 }, action) => {
    switch (action.type) {
        case ACTION_ROW_APPEND:
            let newItems = state.items.slice()
            newItems.push({ key: state.nextKey, value: 'Value#' + state.nextKey })
            const newState = {
                ...state,
                nextKey: state.nextKey + 1,
                items: newItems
            }
            return newState
        default:
            return state
    }
}

const UIView = ({ appendRow, items }) => (
    <div>
        <table>
            <thead>
                <tr>
                    <td>Key</td>
                    <td>Value</td>
                </tr>
            </thead>
            <tbody>
                {items.map(item => (
                    <tr>
                        <td>{item.key}</td>
                        <td>{item.value}</td>
                    </tr>
                ))}
            </tbody>
        </table>
        <div>
            <a onClick={() => appendRow()}>Add</a>
        </div>
    </div>
)
UIView.propTypes = {
    items: PropTypes.array.isRequired,
    appendRow: PropTypes.func.isRequired
}

export const UIComponent = connect(
    mapStateToProps,
    mapDispatchToProps
)(UIView)
