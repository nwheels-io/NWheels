import React from 'react';
import { render } from 'react-dom'
import PropTypes from 'prop-types'
import './index.css';
import { createStore } from 'redux'
import { Provider } from 'react-redux'
import { IndexPageComponent, IndexPageReducer } from './IndexPage';
import { AscetTheme } from './themes/ascet'
import registerServiceWorker from './registerServiceWorker';

const store = createStore(
    IndexPageReducer,
    window.__REDUX_DEVTOOLS_EXTENSION__ && window.__REDUX_DEVTOOLS_EXTENSION__()
)

render(
    <Provider store={store}>
        <AscetTheme>
            <IndexPageComponent />
        </AscetTheme>
    </Provider>,
    document.getElementById('root')
)

registerServiceWorker();
