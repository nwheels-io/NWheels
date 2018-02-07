export const INPUT_RECEIVED = 'HomePage/Transaction/INPUT_RECEIVED'
export const EXECUTE_STARTING = 'HomePage/Transaction/EXECUTE_STARTING'
export const EXECUTE_COMPLETED = 'HomePage/Transaction/EXECUTE_COMPLETED'
export const EXECUTE_FAILED = 'HomePage/Transaction/EXECUTE_FAILED'

const initialState = {
    input: {
        name: null
    },
    output: {
        success: false,
        result: null,
        error: null
    },
    canExecute: false,
    isExecuting: false
}

const thisReducer = (state = initialState, action) => {
    switch (action.type) {
        case INPUT_RECEIVED:
            return {
                ...state,
                input: {
                    name: action.inputName
                },
                canExecute: action.inputName !== ''
            }

        case EXECUTE_STARTING:
            return {
                ...state,
                isExecuting: true,
                output: {
                    success: false,
                    result: null,
                    error: null
                }
            }

        case EXECUTE_COMPLETED:
            return {
                ...state,
                isExecuting: false,
                output: {
                    success: true,
                    result: action.result,
                    error: null
                }
            }

        case EXECUTE_FAILED:
            return {
                ...state,
                isExecuting: false,
                output: {
                    success: false,
                    result: null,
                    error: action.error
                }
            }

        default:
            return state
    }
}

export const thisStoreFromAppStore = (appStore) => {
    return appStore.HomePage.Transaction
}

export const receiveInput = (inputName) => {
    return {
        type: INPUT_RECEIVED,
        inputName
    }
}

export const beginExecute = () => {
    return (dispatch, getState) => {
        let thisStore = thisStoreFromAppStore(getState())
        let requestUrl = '/api/tx/Hello/Hello'
        let requestPayload = {
            name: thisStore.input.name
        } 

        dispatch({
            type: EXECUTE_STARTING
        })

        fetch(requestUrl, {
            method: "POST",
            body: JSON.stringify(requestPayload),
            headers: {
                "Content-Type": "application/json"
            },
        }).then((response) => {
                if (!response.ok) {
                    throw Error(response.statusText);
                }
                return response;
            })
            .then((response) => response.json())
            .then((responseJson) => dispatch({
                type: EXECUTE_COMPLETED,
                result: responseJson.result
            }))
            .catch((error) => {
                dispatch({
                    type: EXECUTE_FAILED,
                    error: error.message
                })
            });
    }
}

export default thisReducer
