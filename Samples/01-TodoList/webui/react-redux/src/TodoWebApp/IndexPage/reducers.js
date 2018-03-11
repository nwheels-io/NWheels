import { combineReducers } from 'redux'

const Toolbar = (state, action) => {
    if (!state) {
        state = {
            options: ['AAA', 'BBB', 'CCC'],
            selectedIndex: 0
        }
    }

    switch (action.type) {
        case 'TOOLBAR_SHOW_SELECT':
            return {
                ...state,
                selectedIndex: action.index
            }
            break
        default:
            return state
    }
}

const Crud = (state = [], action) => {
    switch (action.type) {
        case 'ADD_TODO':
            return [
                ...state,
                {
                    id: action.id,
                    text: action.text,
                    completed: false
                }
            ]
        case 'TOGGLE_TODO':
            return state.map(todo =>
                (todo.id === action.id)
                    ? { ...todo, completed: !todo.completed }
                    : todo
            )
        default:
            return state
    }
}

const IndexPage = combineReducers({
    Toolbar,
    Crud
})

export default IndexPage
