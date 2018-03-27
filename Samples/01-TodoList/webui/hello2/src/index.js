import React from 'react';
import { render } from 'react-dom'
import PropTypes from 'prop-types'
import './index.css';
import { createStore } from 'redux'
import { Provider } from 'react-redux'
import { IndexPageComponent, IndexPageReducer } from './IndexPage';
import registerServiceWorker from './registerServiceWorker';

const store = createStore(
    IndexPageReducer,
    window.__REDUX_DEVTOOLS_EXTENSION__ && window.__REDUX_DEVTOOLS_EXTENSION__()
)

class AscetTheme extends React.Component {
    getChildContext() {
        return {
            theme: {
                renderDemo: () => (
                    <div style={{border: '1px solid black', padding:'3px', margin: '3px'}}>
                        This is the ASCET theme!
                    </div>
                )
            }
        };
    }
    render() {
        return this.props.children
    }
}
AscetTheme.childContextTypes = {
    theme: PropTypes.object.isRequired
}

render(
    <Provider store={store}>
        <AscetTheme>
            <IndexPageComponent />
        </AscetTheme>
    </Provider>,
    document.getElementById('root')
)

registerServiceWorker();
