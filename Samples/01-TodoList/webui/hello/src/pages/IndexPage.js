import React from 'react';
import PropTypes from 'prop-types'
import { combineReducers } from 'redux'
import { connect } from 'react-redux'
import { CounterComponent, CounterComponentReducer } from '../components/CounterComponent'

export const Reducer = (id) => combineReducers({
    "index/first": CounterComponentReducer(`${id}/first`),
    "index/second": CounterComponentReducer(`${id}/second`)
})

const View = ({id}) => (
    <section>
        <h2>Index Page</h2>
        <table>
            <tr>
                <td>First counter</td>
                <td><CounterComponent id="index/first" initialValue={123} step={1} /></td>
            </tr>
            <tr>
                <td>Second counter</td>
                <td><CounterComponent id="index/second" initialValue={456} step={2} /></td>
            </tr>
        </table>
    </section>
)

View.propTypes = {
    id: PropTypes.string.isRequired
}

const mapStateToProps = (state, ownProps) => {
    const instanceState = state[ownProps.id]
    return instanceState
}

const mapDispatchToProps = (dispatch, ownProps) => {
    return { }
}

export const Component = connect(
    mapStateToProps,
    mapDispatchToProps
)(View)
