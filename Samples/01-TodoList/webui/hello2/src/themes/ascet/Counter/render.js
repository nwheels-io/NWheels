import React from 'react';
import * as Counter from './../../../components/Counter'

export const UIRender = (props, actions, context) => (
    <React.Fragment>
        <span>Initial:</span>
        <span><strong>{props.initialValue}</strong></span>

        <span>Current:</span>
        <span><strong>{props.currentValue}</strong></span>

        <span>Delta:</span>
        <span><strong>{props.delta}</strong></span>

        <button onClick={() => actions.incrementOne()}>Increment ({props.increments})</button>
        <button onClick={() => actions.decrementOne()}>Decrement ({props.decrements})</button>
    </React.Fragment>
)
