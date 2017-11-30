import React from 'react'
import { render } from 'react-dom'
import { Provider } from 'react-redux'
import { ConnectedRouter } from 'react-router-redux'
import store, { history } from './store'
import HelloWorldApp from './HelloWorldApp/render'

import './index.css'

const target = document.querySelector('#root')

render(
  <Provider store={store}>
    <ConnectedRouter history={history}>
      <div>
        <HelloWorldApp />
      </div>
    </ConnectedRouter>
  </Provider>,
  target
)
