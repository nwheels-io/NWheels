import React from 'react';
import * as Counter from './../../../components/Counter'
import './style.css';
import classnames from 'classnames'

export default (props, actions, context) => (
    <div className={classnames('Component_Counter', {
        'positive': props.delta > 0,
        'negative': props.delta < 0
    })}>
        <span className="label">Initial</span>
        <span className="value">{props.initialValue}</span>

        <span className="label">Current</span>
        <span className="value">{props.currentValue}</span>

        <span className="label">Delta</span>
        <span className="value">{props.delta}</span>

        <button className="increment" onClick={() => actions.incrementOne()}>Increment ({props.increments})</button>
        <button className="decrement" onClick={() => actions.decrementOne()}>Decrement ({props.decrements})</button>
    </div>
)
