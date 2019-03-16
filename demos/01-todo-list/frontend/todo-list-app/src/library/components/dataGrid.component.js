import { connect } from 'react-redux';
import { Action } from 'rxjs/internal/scheduler/Action';

const initialState = {
    nextKey: -1,
    isLoaded: false,
    isLoadFailed: false,
    items: [],
    selectedCell: null
};

const mergeItemChanges = (item, newKey, newState, newDataProps) => {
    return {
        ...item, 
        key: newKey || item.key,
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
                (item) => mergeItemChanges(item, null, itemState, action.itemPropChanges || {})
            );
            return {
                ...state,
                items,
                selectedCell: null
            };
        case 'DATAGRID_ITEM_COMMIT_FINISHED':
            itemState = (action.success ? 'UNCHANGED' : 'FAILED');
            items = replaceItem(
                state.items, 
                action.key, 
                (item) => mergeItemChanges(item, action.newKey, itemState, action.itemPropChanges || {})
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
                    item => mergeItemChanges(item, null, item.state, action.itemPropChanges || {})
                )
            };
        case 'DATAGRID_REMOVE_ITEM':
            return { 
                ...state, 
                items: state.items.filter(item => item.key !== action.key),
                selectedCell: null
            };
        case 'DATAGRID_SELECT_CELL':
            return {
                ...state,
                selectedCell: { row: action.row, col: action.col }
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
    itemCommitFinished: (key, newKey, itemPropChanges, isDeleting, success) => {
        return {
            type: 'DATAGRID_ITEM_COMMIT_FINISHED',
            key,
            newKey,
            itemPropChanges,
            isDeleting,
            success
        };
    },
    selectCell: (row, col) => {
        return {
            type: 'DATAGRID_SELECT_CELL',
            row,
            col
        }
    }
};

let MOCK_SERVER_ID = 114;

const ThunkCreators = {
    beginLoadItems: (ID) => {
        return (dispatch) => {
            dispatch(ActionCreators.loadStarted());
            setTimeout(() => {
                dispatch(ActionCreators.loadFinished(
                    [
                        { key: 111, state: 'UNCHANGED', data: { id: 111, title: 'AAA', dont: true } },
                        { key: 112, state: 'UNCHANGED', data: { id: 112, title: 'BBB', dont: false } },
                        { key: 113, state: 'UNCHANGED', data: { id: 113, title: 'CCC', dont: false } }
                    ],
                    true    
                ));
            }, 5000);
        };
    },
    beginCommitItem: (ID, itemKey, itemPropChanges, isDeleting) => {
        return (dispatch, getState) => {
            const ownState = getState()[ID];
            const item = ownState.items.find(item => item.key === itemKey);
            if (!item) {
                return;
            }

            if (item.state === 'NEW' && isDeleting) {
                dispatch(ActionCreators.removeItem(itemKey));
                return;
            }

            if (!isDeleting && itemPropChanges) {
                dispatch(ActionCreators.changeItem(itemKey, itemPropChanges));
            }

            dispatch(ActionCreators.itemCommitStarted(itemKey, itemPropChanges, isDeleting));
            
            console.log('SENDING TO SERVER', {
                action: (item.state === 'NEW' ? 'CREATE' : (isDeleting ? 'DELETE' : 'UPDATE')),
                key: (item.state === 'NEW' ? undefined : item.key), 
                changes: (isDeleting ? undefined : itemPropChanges)
            });

            setTimeout(() => {
                if (isDeleting) {
                    dispatch(ActionCreators.removeItem(itemKey));
                } else {
                    const serverId = item.id || MOCK_SERVER_ID++;
                    dispatch(ActionCreators.itemCommitFinished(itemKey, serverId, {id: serverId}, isDeleting, true));
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
            items: ownState.items,
            nextKey: ownState.nextKey,
            selectedCell: ownState.selectedCell
        };
    },
    (dispatch) => {
        return {    
            beginLoadItems: () => {
                dispatch(ThunkCreators.beginLoadItems(ID));
            },
            addItem: (newItemProps) => {
                dispatch(ActionCreators.addItem(newItemProps));
            },
            beginCommitItem: (key, itemPropChanges, isDeleting) => {
                dispatch(ThunkCreators.beginCommitItem(ID, key, itemPropChanges, isDeleting));
            },
            selectCell: (row, col) => {
                dispatch(ActionCreators.selectCell(row, col));
            }
        };
    }
)(skin);

export const DataGrid = {
    Connector,
    Reducer
};
