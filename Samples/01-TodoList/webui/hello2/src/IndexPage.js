import React, { Component } from 'react';
import {
    BrowserRouter as Router,
    Route,
    Link
} from 'react-router-dom'
import { combineReducers } from 'redux'
import { Provider } from 'react-redux'
import { namespaced } from 'redux-subspace'
import { SubspaceProvider, subspaced } from 'react-redux-subspace'
import * as Counter from './components/Counter';
import * as Spreadsheet from './components/Spreadsheet';

const HomeComponent = () => (
    <div>
        <h2>Home</h2>
    </div>
)

const CountersOneComponent = () => (
    <div>
        <h2>Counters One</h2>
        <SubspaceProvider mapState={(state) => state.c1} namespace="c1">
            <Counter.UIComponent initialValue={123} step={1} />
        </SubspaceProvider>
        <br />
        <SubspaceProvider mapState={(state) => state.c2} namespace="c2">
            <Counter.UIComponent initialValue={456} step={2} />
        </SubspaceProvider>
    </div>
)
const CountersOneReducer = combineReducers({
    c1: namespaced('c1')(Counter.UIReducer),
    c2: namespaced('c2')(Counter.UIReducer)
})

const CountersTwoComponent = () => (
    <div>
        <h2>Counters Two</h2>
        <SubspaceProvider mapState={(state) => state.c3} namespace="c3">
            <Counter.UIComponent initialValue={-11} step={1} />
        </SubspaceProvider>
        <br />
        <SubspaceProvider mapState={(state) => state.c4} namespace="c4">
            <Counter.UIComponent initialValue={-22} step={2} />
        </SubspaceProvider>
    </div>
)
const CountersTwoReducer = combineReducers({
    c3: namespaced('c3')(Counter.UIReducer),
    c4: namespaced('c4')(Counter.UIReducer)
})

const TodoListComponent = () => (
    <div>
        <h2>TODO List</h2>
        <SubspaceProvider mapState={(state) => state.s1} namespace="s1">
            <Spreadsheet.UIComponent />
        </SubspaceProvider>
    </div>
)
const TodoListReducer = combineReducers({
    s1: namespaced('s1')(Spreadsheet.UIReducer)
})

const ToolBarComponent = () => (
    <nav>
        <Link to="/">Home</Link>
        {' | '}
        <Link to="/counters1">Counters 1</Link>
        {' | '}
        <Link to="/counters2">Counters 2</Link>
        {' | '}
        <Link to="/todo">TODO List</Link>
    </nav>
)

export const IndexPageComponent = () => (
    <Router>
        <div>
            <ToolBarComponent />
            <hr />
            <Route exact path="/" component={HomeComponent} />
            <Route path="/counters1" component={subspaced((state) => state.one, 'one')(CountersOneComponent)} />
            <Route path="/counters2" component={subspaced((state) => state.two, 'two')(CountersTwoComponent)} />
            <Route path="/todo" component={subspaced((state) => state.todo, 'todo')(TodoListComponent)} />
        </div>
    </Router>
);
export const IndexPageReducer = combineReducers({
    one: namespaced('one')(CountersOneReducer),
    two: namespaced('two')(CountersTwoReducer),
    todo: namespaced('todo')(TodoListReducer)
})
