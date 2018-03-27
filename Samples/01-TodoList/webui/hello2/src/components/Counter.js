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
    return {
        props: new UIProps(ownProps.initialValue, ownProps.step, state.increments, state.decrements)
    }
}

const mapDispatchToProps = (dispatch, ownProps) => {
    return {
        actions: new UIActions(dispatch)
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

export class UIProps {
    constructor(initialValue, step, increments, decrements) {
        this._initialValue = initialValue
        this._step = step
        this._increments = increments
        this._decrements = decrements
        this._delta = step * (increments - decrements)
    }
    get initialValue() {
        return this._initialValue
    }
    get step() {
        return this._step
    }
    get delta() {
        return this._delta
    }
    get currentValue() {
        return this._initialValue + this._delta
    }
    get increments() {
        return this._increments
    }
    get decrements() {
        return this._decrements
    }
}

export class UIActions {
    constructor(dispatch) {
        this._dispatch = dispatch
    }
    incrementOne() {
        this._dispatch(actionIncrement(1))
    }
    decrementOne() {
        this._dispatch(actionDecrement(1))
    }
    incrementMany(count) {
        this._dispatch(actionIncrement(count))
    }
    decrementMany(count) {
        this._dispatch(actionDecrement(count))
    }
}

const UIView = ({ props, actions }, context) => (
    <React.Fragment>
        {context.theme.render.Counter(props, actions, context)}
    </React.Fragment>
)

UIView.propTypes = {
    props: PropTypes.instanceOf(UIProps).isRequired,
    actions: PropTypes.instanceOf(UIActions).isRequired
}

UIView.contextTypes = {
    theme: PropTypes.object.isRequired
}

export const UIComponent = connect(
    mapStateToProps,
    mapDispatchToProps
)(UIView)
