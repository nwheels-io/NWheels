import React from 'react';
import { render } from 'react-dom'
import './index.css';
import { createStore } from 'redux'
import { Provider } from 'react-redux'
import { IndexPage, IndexPageReducer } from './IndexPage';
import registerServiceWorker from './registerServiceWorker';

const store = createStore(
    IndexPageReducer,
    window.__REDUX_DEVTOOLS_EXTENSION__ && window.__REDUX_DEVTOOLS_EXTENSION__()
)

render (
    <Provider store={store}>
        <IndexPage />
    </Provider>,
    document.getElementById('root')
)

registerServiceWorker();
