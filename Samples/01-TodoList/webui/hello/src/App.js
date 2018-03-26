import React, { Component } from 'react';
import PropTypes from 'prop-types'
import { combineReducers } from 'redux'
import { connect } from 'react-redux'
import { CounterComponent, CounterComponentReducer } from './components/CounterComponent'
import logo from './logo.svg';
import './App.css';

export const appReducer = combineReducers({
    first: CounterComponentReducer('first'),
    second: CounterComponentReducer('second')
})

export const App = () => (
    <div className="App">
        <header className="App-header">
            <img src={logo} className="App-logo" alt="logo" />
            <h1 className="App-title">Welcome to React</h1>
        </header>
        <p className="App-intro">
            To get started, edit <code>src/App.js</code> and save to reload.
        </p>
        <table>
            <tr>
                <td>First counter</td>
                <td><CounterComponent id='first' initialValue={123} step={1} /></td>
            </tr>
            <tr>
                <td>Second counter</td>
                <td><CounterComponent id='second' initialValue={456} step={2} /></td>
            </tr>
        </table>
    </div>
)
