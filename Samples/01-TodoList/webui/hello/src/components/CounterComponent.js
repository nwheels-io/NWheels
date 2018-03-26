import React, { Component } from 'react';
import PropTypes from 'prop-types'
import { combineReducers } from 'redux'
import { connect } from 'react-redux'

const COUNTER_ACTION_INCREMENT = 'COUNTER_ACTION_INCREMENT'
const COUNTER_ACTION_DECREMENT = 'COUNTER_ACTION_DECREMENT'

const idQualified = (id, value) => `${id}/${value}`

const counterActionIncrement = (id, count) => {
    return {
        type: idQualified(id, COUNTER_ACTION_INCREMENT),
        count
    }
}

const counterActionDecrement = (id, count) => {
    return {
        type: idQualified(id, COUNTER_ACTION_DECREMENT),
        count
    }
}

export const CounterComponentReducer = (id) => (state = { increments: 0, decrements: 0 }, action) => {
    switch (action.type) {
        case idQualified(id, COUNTER_ACTION_INCREMENT):
            return {
                ...state,
                increments: state.increments + action.count
            }
        case idQualified(id, COUNTER_ACTION_DECREMENT):
            return {
                ...state,
                decrements: state.decrements + action.count
            }
        default:
            return state
    }
}

const counterMapStateToProps = (state, ownProps) => {
    const instanceState = state[ownProps.id]

    let delta = ownProps.step * (instanceState.increments - instanceState.decrements)
    return {
        ...instanceState,
        delta,
        currentValue: ownProps.initialValue + delta
    }
}

const counterMapDispatchToProps = (dispatch, ownProps) => {
    return {
        incrementOne: () => {
            dispatch(counterActionIncrement(ownProps.id, 1))
        },
        decrementOne: () => {
            dispatch(counterActionDecrement(ownProps.id, 1))
        },
        incrementMany: (count) => {
            dispatch(counterActionIncrement(ownProps.id, count))
        },
        decrementMany: (count) => {
            dispatch(counterActionDecrement(ownProps.id, count))
        }
    }
}

const CounterView = ({ initialValue, currentValue, delta, increments, decrements, incrementOne, decrementOne }) => (
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

CounterView.propTypes = {
    initialValue: PropTypes.number.isRequired,
    currentValue: PropTypes.number.isRequired,
    delta: PropTypes.number.isRequired,
    increments: PropTypes.number.isRequired,
    decrements: PropTypes.number.isRequired,
    incrementOne: PropTypes.func.isRequired,
    decrementOne: PropTypes.func.isRequired
}

export const CounterComponent = connect(
    counterMapStateToProps,
    counterMapDispatchToProps
)(CounterView)

