import { combineReducers } from 'redux'
import { routerReducer } from 'react-router-redux'
import HomePage from './HomePage/logic'

export default combineReducers({
    router: routerReducer,
    HomePage,
})
