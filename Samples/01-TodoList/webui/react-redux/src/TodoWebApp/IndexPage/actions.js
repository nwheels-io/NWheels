let Crud__nextRecordId = 0

export const Toolbar_Show_Select = index => {
    return {
        type: 'TOOLBAR_SHOW_SELECT',
        index
    }
}

export const Crud_Add = description => {
    return {
        type: 'CRUD_ADD',
        id: Crud__nextRecordId++,
        description,
        done: false
    }
}

export const Crud_Edit = id => {
    return {
        type: 'CRUD_EDIT',
        id
    }
}

export const Crud_Delete = id => {
    return {
        type: 'CRUD_DELETE',
        id
    }
}

export const Crud_Commit = id => {
    return {
        type: 'CRUD_COMMIT',
        id
    }
}

export const Crud_Cancel = id => {
    return {
        type: 'CRUD_CANCEL',
        id
    }
}

export const Crud_Refresh = () => {
    return {
        type: 'CRUD_REFRESH'
    }
}

export const Crud_Data_Ready = arrayOfRecords => {
    return {
        type: 'CRUD_DATA_READY',
        arrayOfRecords
    }
}
