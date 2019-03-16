import React from "react";
import { createStore, combineReducers, applyMiddleware } from 'redux';
import { Provider } from 'react-redux';
import thunk from 'redux-thunk';
import { prepareComponent, PubSub, DataGrid, Form, SoloPage } from '../../library/components';
import { DemoMinimal as skin } from '../../library/skins';

const IndexSoloPage = prepareComponent(SoloPage, 'index', skin.SoloPage);
const NewItemForm = prepareComponent(Form, 'newItem', skin.Form);
const TodosDataGrid = prepareComponent(DataGrid, 'todos', skin.DataGrid);

function buildIndexPageStore() {
    const rootReducer = combineReducers({
        ...IndexSoloPage.Reducers,
        ...NewItemForm.Reducers,
        ...TodosDataGrid.Reducers
    });
    return createStore(
        rootReducer,
        applyMiddleware(thunk)
    );
}

const store = buildIndexPageStore();

export const IndexPage = (props) => {
    const onNewItemAdd = new PubSub.Event('indexSoloPage/newItemForm/add');

    return (
        <Provider store={store}>
            <IndexSoloPage.Component>
                <NewItemForm.Component 
                    fields={[
                        { name: 'title', label: 'New Item' }
                    ]} 
                    actions={[
                        { name: 'add', label: 'Add', onExecute: (data) => onNewItemAdd.fire(data) }
                    ]}
                    pushResetForm={onNewItemAdd} 
                />
                <TodosDataGrid.Component 
                    columns = {[
                        { title: 'ID', field: 'id' },
                        { title: 'Title', field: 'title' },
                        { title: 'Done', field: 'done' }
                    ]}
                    pushAddItem={onNewItemAdd} 
                />
            </IndexSoloPage.Component>
        </Provider>
    );
};
