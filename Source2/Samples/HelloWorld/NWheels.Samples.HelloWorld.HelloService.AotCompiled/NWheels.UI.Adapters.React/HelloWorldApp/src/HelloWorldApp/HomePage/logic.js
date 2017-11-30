import { combineReducers } from 'redux'
import Transaction from './Transaction/logic' 

const initialState = {
}

const thisReducer = (state = initialState, action) => {
    switch (action.type) {
        default:
            return state
    }
}

export const thisStoreFromAppStore = (appStore) => {
    return appStore.HomePage
}

export default combineReducers({
    thisReducer,
    Transaction
})
