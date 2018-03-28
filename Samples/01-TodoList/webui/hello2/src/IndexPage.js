import React, { Component } from 'react';
import PropTypes from 'prop-types'
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

const HomeComponent = ({}, context) => (
    <div>
        <h2>Home</h2>
        {context.theme.render.Demo()}
    </div>
)
HomeComponent.contextTypes = {
    theme: PropTypes.object.isRequired
}

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
            <Spreadsheet.UIComponent>
                <Spreadsheet.Column title="No." type={Spreadsheet.SPREADSHEET_COLUMN_TYPE_KEY} field="key" width={75} />
                <Spreadsheet.Column title="Order" type={Spreadsheet.SPREADSHEET_COLUMN_TYPE_ORDER} field="order" width={75} />
                <Spreadsheet.Column title="Value" type={Spreadsheet.SPREADSHEET_COLUMN_TYPE_INPUT} field="value" width={200} />
                <Spreadsheet.Column title="Action" type={Spreadsheet.SPREADSHEET_COLUMN_TYPE_COMMAND} />
                <Spreadsheet.Column title="Status" type={Spreadsheet.SPREADSHEET_COLUMN_TYPE_STATUS} />
            </Spreadsheet.UIComponent>
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

export const UIComponent = () => (
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
export const UIReducer = combineReducers({
    one: namespaced('one')(CountersOneReducer),
    two: namespaced('two')(CountersTwoReducer),
    todo: namespaced('todo')(TodoListReducer)
})
