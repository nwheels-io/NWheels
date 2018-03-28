import React from 'react';
import { render } from 'react-dom'
import PropTypes from 'prop-types'
import './index.css';
import { createStore } from 'redux'
import { Provider } from 'react-redux'
import * as IndexPage from './IndexPage';
import { AscetTheme } from './themes/ascet'
import registerServiceWorker from './registerServiceWorker';

const store = createStore(
    IndexPage.UIReducer,
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
