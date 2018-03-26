import React, { Component } from 'react';
import {
    BrowserRouter as Router,
    Route,
    Link
} from 'react-router-dom'
import { combineReducers } from 'redux'
import { Provider } from 'react-redux'
import { namespaced } from 'redux-subspace'
import { SubspaceProvider } from 'react-redux-subspace'
import * as Counter from './components/Counter';

const HomeComponent = () => (
    <div>
        <h2>Home</h2>
    </div>
)

const CountersOneComponent = () => (
    <div>
        <h2>Counters One</h2>
        <Counter.UIComponent initialValue="123" />
        <br />
        <Counter.UIComponent initialValue="456" />
    </div>
)
const CountersOneReducer = combineReducers({
    c1: namespaced('c1')(Counter.UIReducer),
    c2: namespaced('c2')(Counter.UIReducer)
})

const CountersTwoComponent = () => (
    <div>
        <h2>Counters Two</h2>
        <Counter.UIComponent initialValue="-11" />
        <br />
        <Counter.UIComponent initialValue="-22" />
    </div>
)
const CountersTwoReducer = combineReducers({
    c3: namespaced('c3')(Counter.UIReducer),
    c4: namespaced('c4')(Counter.UIReducer)
})

const ToolBar = () => (
    <nav>
        <Link to="/">Home</Link>
        {' | '}
        <Link to="/counters1">Counters 1</Link>
        {' | '}
        <Link to="/counters2">Counters 2</Link>
    </nav>
)

export const IndexPage = () => (
    <Router>
        <div>
            <ToolBar />
            <hr />
            <Route exact path="/" component={HomeComponent} />
            <SubspaceProvider mapState={(state) => state.one}>
                <Route path="/counters1" component={CountersOneComponent} />
            </SubspaceProvider>
            <SubspaceProvider mapState={(state) => state.two}>
                <Route path="/counters2" component={CountersTwoComponent} />
            </SubspaceProvider>
        </div>
    </Router>
);

export const IndexPageReducer = combineReducers({
    one: namespaced('one')(CountersOneReducer),
    two: namespaced('two')(CountersTwoReducer)
});
