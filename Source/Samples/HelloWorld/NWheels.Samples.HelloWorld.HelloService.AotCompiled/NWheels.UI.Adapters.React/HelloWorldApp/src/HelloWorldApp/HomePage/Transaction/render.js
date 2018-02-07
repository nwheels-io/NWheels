import React from 'react'
import { connect } from 'react-redux'
import { thisStoreFromAppStore, receiveInput, beginExecute } from './logic'

const Transaction = props => (
    <div>
        <form onSubmit={e => {
            e.preventDefault()
            props.submitTx(e)
        }}>
            <h1>Hello World</h1>
            <p>
                Name:
                <input type="text" className="input_field_name" />
            </p>
            {props.result &&
                <p>
                    We say: {props.result}
                </p>
            }
            {props.error &&
                <p>
                    An error occurred. {props.error}
                </p>
            }
            <p>
                <input type="submit" value="Submit" disabled={props.isExecuting} />
            </p>
        </form>
    </div>
)

const mapStateToProps = state => {
    let thisStore = thisStoreFromAppStore(state)
    return {
        name: thisStore.input.name,
        result: thisStore.output.result,
        error: thisStore.output.error,
        isExecuting: thisStore.isExecuting,
        canExecute: thisStore.canExecute
    }
}

const mapDispatchToProps = dispatch => {
    return {
        receiveInput,
        beginExecute,
        submitTx: (evt) => {
            var inputEl = evt.target.querySelector('.input_field_name')
            var inputName = inputEl.value
            dispatch(receiveInput(inputName))
            dispatch(beginExecute())
        }
    }
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(Transaction)
