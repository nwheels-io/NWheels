import React from 'react';
import * as Counter from './../../../components/Counter'
import './style.css';
import classnames from 'classnames'

export const UIRender = (props, actions, context) => (
    <div className={classnames('Component_Counter', {
        'positive': props.delta > 0,
        'negative': props.delta < 0
    })}>
        <span>Initial:</span>
        <span><strong>{props.initialValue}</strong></span>

        <span>Current:</span>
        <span><strong>{props.currentValue}</strong></span>

        <span>Delta:</span>
        <span><strong>{props.delta}</strong></span>

        <button className="increment" onClick={() => actions.incrementOne()}>Increment ({props.increments})</button>
        <button className="decrement" onClick={() => actions.decrementOne()}>Decrement ({props.decrements})</button>
    </div>
)
