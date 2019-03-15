import { connect } from 'react-redux';

const Reducer = (ID) => (state = { nextKey: 1, items: [] }, action) => {
    switch (action.type) {
        case 'ADD':
            const newKey = state.nextKey;
            const newItem = { ['$key']: newKey, ...action.newItemProps };
            return { 
                ...state, 
                nextKey: state.nextKey + 1,
                items: [...state.items, newItem] 
            };
        case 'CHANGE':
            return { 
                ...state, 
                items: state.items.map(item => item['$key'] !== action.key ? item : {...item, ...action.itemPropChanges})
            };
        case 'DELETE':
            return { 
                ...state, 
                items: state.items.filter(item => item['$key'] !== action.key)
            };
    }

    return state;
};

const Connector = (ID) => (skin) => connect(
    (state) => { 
        const ownState = state[ID];
        return { 
            items: ownState.items
        };
    },
    (dispatch) => {
        return {
            add: (newItemProps) => {
                dispatch({
                    type: 'ADD',
                    newItemProps
                });
            },
            change: (key, itemPropChanges) => {
                dispatch({
                    type: 'CHANGE',
                    key,
                    itemPropChanges
                });
            },
            delete: (key) => {
                dispatch({
                    type: 'DELETE',
                    key
                });
            }
        };
    }
)(skin);

export const DataGrid = {
    Connector,
    Reducer
};
