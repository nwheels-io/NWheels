import { connect } from 'react-redux';

const Reducer = (ID) => (state = { values: { } }, action) => {
    switch (action.type) {
        case 'FORM_SET_VALUE':
            return { 
                ...state, 
                values: {
                    ...state.values,
                    [action.name]: action.value
                }
            };
        case 'FORM_RESET':
            return { 
                ...state, 
                values: { }
            };
        default:
            return state;
    }
};

const Connector = (ID) => (skin) => connect(
    (state, ownProps) => { 
        const ownState = state[ID];
        return { 
            fields: ownProps.fields,
            values: ownState.values,
            submit: ownProps.onSubmit
        };
    },
    (dispatch) => {
        return {
            setValue: (name, value) => {
                dispatch({
                    type: 'FORM_SET_VALUE',
                    name,
                    value
                });
            },
            resetForm: () => {
                dispatch({
                    type: 'FORM_RESET'
                });
            }
        };
    }
)(skin);

export const Form = {
    Connector,
    Reducer
}
