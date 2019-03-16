import { connect } from 'react-redux';
import { Action } from 'rxjs/internal/scheduler/Action';

const initialState = {
    nextKey: -1,
    isLoaded: false,
    isLoadFailed: false,
    items: []
};

const mergeItemChanges = (item, newState, newDataProps) => {
    return {
        ...item, 
        state: newState || item.state, 
        data: { 
            ...item.data, 
            ...newDataProps
        }
    };
};

const replaceItem = (items, key, newItemFactory) => {
    return items.map(item => 
        item.key !== key 
        ? item 
        : newItemFactory(item)
    );
}

const Reducer = (ID) => (state = initialState, action) => {
    let itemState;
    let items;

    switch (action.type) {
        case 'DATAGRID_LOAD_STARTED':
            return initialState;
        case 'DATAGRID_LOAD_FINISHED':
            return {
                nextKey: initialState.nextKey,
                isLoaded: action.success,
                isLoadFailed: !action.success,
                items: action.items || []
            };
        case 'DATAGRID_ITEM_COMMIT_STARTED':
            itemState = (action.isDeleting ? 'DELETING' : 'SAVING');
            items = replaceItem(
                state.items, 
                action.key, 
                (item) => mergeItemChanges(item, itemState, {})
            );
            return {
                ...state,
                items
            };
        case 'DATAGRID_ITEM_COMMIT_FINISHED':
            itemState = (action.success ? 'UNCHANGED' : 'FAILED');
            items = replaceItem(
                state.items, 
                action.key, 
                (item) => mergeItemChanges(item, itemState, {})
            );
            return {
                ...state,
                items
            };
        case 'DATAGRID_ADD_ITEM':
            const newItem = {
                key: state.nextKey,
                state: 'NEW',
                data: action.newItemProps
            };
            return { 
                ...state, 
                nextKey: state.nextKey - 1,
                items: [...state.items, newItem] 
            };
        case 'DATAGRID_CHANGE_ITEM':
            return { 
                ...state, 
                items: replaceItem(
                    state.items, 
                    action.key, 
                    item => mergeItemChanges(item, item.state, action.itemPropChanges)
                )
            };
        case 'DATAGRID_REMOVE_ITEM':
            return { 
                ...state, 
                items: state.items.filter(item => item.key !== action.key)
            };
    }

    return state;
};

const ActionCreators = {
    addItem: (newItemProps) => {
        return {
            type: 'DATAGRID_ADD_ITEM',
            newItemProps
        };
    },
    changeItem: (key, itemPropChanges) => {
        return {
            type: 'DATAGRID_CHANGE_ITEM',
            key,
            itemPropChanges
        };
    },
    removeItem: (key) => {
        return {
            type: 'DATAGRID_REMOVE_ITEM',
            key
        };
    },
    loadStarted: () => {
        return {
            type: 'DATAGRID_LOAD_STARTED'
        };
    },
    loadFinished: (items, success) => {
        return {
            type: 'DATAGRID_LOAD_FINISHED',
            items,
            success
        };
    },
    itemCommitStarted: (key, itemPropChanges, isDeleting) => {
        return {
            type: 'DATAGRID_ITEM_COMMIT_STARTED',
            key, 
            itemPropChanges,
            isDeleting
        };
    },
    itemCommitFinished: (key, itemPropChanges, isDeleting, success) => {
        return {
            type: 'DATAGRID_ITEM_COMMIT_FINISHED',
            key,
            itemPropChanges,
            isDeleting,
            success
        };
    }
};

const ThunkCreators = {
    beginLoadItems: () => {
        return (dispatch) => {
            dispatch(ActionCreators.loadStarted());
            setTimeout(() => {
                dispatch(ActionCreators.loadFinished(
                    [
                        { key: 111, state: 'UNCHANGED', data: { id: 111, title: 'AAA', dont: true } },
                        { key: 222, state: 'UNCHANGED', data: { id: 222, title: 'BBB', dont: false } },
                        { key: 333, state: 'UNCHANGED', data: { id: 333, title: 'CCC', dont: false } }
                    ],
                    true    
                ));
            }, 5000);
        };
    },
    beginCommitItem: (itemKey, itemPropChanges, isDeleting) => {
        return (dispatch) => {
            dispatch(ActionCreators.itemCommitStarted(itemKey, itemPropChanges, isDeleting));
            setTimeout(() => {
                if (isDeleting) {
                    dispatch(ActionCreators.removeItem(itemKey));
                } else {
                    dispatch(ActionCreators.itemCommitFinished(itemKey, itemPropChanges, isDeleting, true));
                }
            }, 3000);
        };
    },
}

const Connector = (ID) => (skin) => connect(
    (state) => { 
        const ownState = state[ID];
        return { 
            isLoaded: ownState.isLoaded,
            isLoadFailed: ownState.isLoadFailed,
            items: ownState.items
        };
    },
    (dispatch) => {
        return {
            beginLoadItems: () => {
                dispatch(ThunkCreators.beginLoadItems());
            },
            addItem: (newItemProps) => {
                dispatch(ActionCreators.addItem(newItemProps));
            },
            beginCommitItem: (key, itemPropChanges, isDeleting) => {
                dispatch(ThunkCreators.beginCommitItem(key, itemPropChanges, isDeleting));
            }
        };
    }
)(skin);

export const DataGrid = {
    Connector,
    Reducer
};
