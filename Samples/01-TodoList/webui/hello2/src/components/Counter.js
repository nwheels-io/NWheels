import React, { Component } from 'react';
import PropTypes from 'prop-types'
import { combineReducers } from 'redux'
import { connect } from 'react-redux'

const ACTION_INCREMENT = 'COUNTER__INCREMENT'
const ACTION_DECREMENT = 'COUNTER__DECREMENT'

const actionIncrement = (count) => {
    return {
        type: ACTION_INCREMENT,
        count
    }
}

const actionDecrement = (count) => {
    return {
        type: ACTION_DECREMENT,
        count
    }
}

const mapStateToProps = (state, ownProps) => {
    let delta = ownProps.step * (state.increments - state.decrements)
    return {
        ...state,
        delta,
        currentValue: ownProps.initialValue + delta
    }
}

const mapDispatchToProps = (dispatch, ownProps) => {
    return {
        incrementOne: () => {
            dispatch(actionIncrement(1))
        },
        decrementOne: () => {
            dispatch(actionDecrement(1))
        },
        incrementMany: (count) => {
            dispatch(actionIncrement(count))
        },
        decrementMany: (count) => {
            dispatch(actionDecrement(count))
        }
    }
}

export const UIReducer = (state = { increments: 0, decrements: 0 }, action) => {
    switch (action.type) {
        case ACTION_INCREMENT:
            return {
                ...state,
                increments: state.increments + action.count
            }
        case ACTION_DECREMENT:
            return {
                ...state,
                decrements: state.decrements + action.count
            }
        default:
            return state
    }
}

const UIView = ({ initialValue, currentValue, delta, increments, decrements, incrementOne, decrementOne }) => (
    <React.Fragment>
        <span>Initial:</span>
        <span><strong>{initialValue}</strong></span>

        <span>Current:</span>
        <span><strong>{currentValue}</strong></span>

        <span>Delta:</span>
        <span><strong>{delta}</strong></span>

        <button onClick={() => incrementOne()}>Increment ({increments})</button>
        <button onClick={() => decrementOne()}>Decrement ({decrements})</button>
    </React.Fragment>
)

UIView.propTypes = {
    initialValue: PropTypes.number.isRequired,
    currentValue: PropTypes.number.isRequired,
    delta: PropTypes.number.isRequired,
    increments: PropTypes.number.isRequired,
    decrements: PropTypes.number.isRequired,
    incrementOne: PropTypes.func.isRequired,
    decrementOne: PropTypes.func.isRequired
}

export const UIComponent = connect(
    mapStateToProps,
    mapDispatchToProps
)(UIView)
