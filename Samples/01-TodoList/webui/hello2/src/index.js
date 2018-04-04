import React from 'react';
import { render } from 'react-dom'
import PropTypes from 'prop-types'

import { createStore, applyMiddleware } from 'redux'
import { Provider } from 'react-redux'
import thunk from 'redux-thunk';

import './index.css';
import registerServiceWorker from './registerServiceWorker';
import * as IndexPage from './IndexPage';
import { AscetTheme } from './themes/ascet'

const store = createStore(
    IndexPage.UIReducer,
    applyMiddleware(thunk),
    window.__REDUX_DEVTOOLS_EXTENSION__ && window.__REDUX_DEVTOOLS_EXTENSION__()
)

render(
    <Provider store={store}>
        <AscetTheme>
            <IndexPage.UIComponent />
        </AscetTheme>
    </Provider>,
    document.getElementById('root')
)

registerServiceWorker();
