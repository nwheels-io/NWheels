import React, { Component } from 'react';
import PropTypes from 'prop-types'
import { combineReducers } from 'redux'
import { connect } from 'react-redux'
import * as IndexPage from './pages/IndexPage'
import logo from './logo.svg';
import './App.css';

export const appReducer = combineReducers({
    index: IndexPage.Reducer('index')
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
        <IndexPage.Component id="index" />
    </div>
)
