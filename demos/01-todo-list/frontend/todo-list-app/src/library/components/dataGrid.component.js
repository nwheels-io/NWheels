import { connect } from 'react-redux';

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
    beginLoadItems: (dal, ID, fields) => {
        return (dispatch) => {
            dispatch(ActionCreators.loadStarted());
            // setTimeout(() => {
            //     
            //         [
            //             { key: 111, state: 'UNCHANGED', data: { id: 111, title: 'AAA', done: true } },
            //             { key: 112, state: 'UNCHANGED', data: { id: 112, title: 'BBB', done: false } },
            //             { key: 113, state: 'UNCHANGED', data: { id: 113, title: 'CCC', done: false } }
            //         ],
            //         true    
            //     ));
            // }, 5000);
            
            dal.retrieve(fields)
                .then(dataArray => {
                    const items = dataArray.map(data => ({
                        key: data.id, //TODO: parameterize this
                        state: 'UNCHANGED',
                        data
                    }));
                    dispatch(ActionCreators.loadFinished(items, true));
                })
                .catch(error => {
                    console.error(error);
                    dispatch(ActionCreators.loadFinished(null, false));
                });
        };
    },
    beginCommitItem: (dal, ID, itemKey, itemPropChanges, isDeleting) => {
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

            let promise;
            if (item.state === 'NEW') {
                promise = dal.create(itemPropChanges)
            } else if (!isDeleting) {
                promise = dal.update(itemKey, itemPropChanges);
            } else {
                promise = dal.delete(itemKey);
            }

            if (isDeleting) {
                promise.then(() => dispatch(ActionCreators.removeItem(itemKey)));
            } else {
                promise.then(serverPropChanges => dispatch(ActionCreators.itemCommitFinished(itemKey, serverPropChanges.id, serverPropChanges, isDeleting, true)));
            }

            promise.catch(err => dispatch(ActionCreators.itemCommitFinished(itemKey, null, null, isDeleting, false)));

            // console.log('SENDING TO SERVER', {
            //     action: (item.state === 'NEW' ? 'CREATE' : (isDeleting ? 'DELETE' : 'UPDATE')),
            //     key: (item.state === 'NEW' ? undefined : item.key), 
            //     changes: (isDeleting ? undefined : itemPropChanges)
            // });

            // setTimeout(() => {
            //     if (isDeleting) {
            //         dispatch(ActionCreators.removeItem(itemKey));
            //     } else {
            //         const serverId = item.data.id || MOCK_SERVER_ID++;
            //         dispatch(ActionCreators.itemCommitFinished(itemKey, serverId, {id: serverId}, isDeleting, true));
            //     }
            // }, 3000);
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
            addItem: (newItemProps) => {
                dispatch(ActionCreators.addItem(newItemProps));
            },
            selectCell: (row, col) => {
                dispatch(ActionCreators.selectCell(row, col));
            },
            beginLoadItems: (dal, fields) => {
                dispatch(ThunkCreators.beginLoadItems(dal, ID, fields));
            },
            beginCommitItem: (dal, key, itemPropChanges, isDeleting) => {
                dispatch(ThunkCreators.beginCommitItem(dal, ID, key, itemPropChanges, isDeleting));
            }
        };
    }
)(skin);

export const DataGrid = {
    Connector,
    Reducer
};
