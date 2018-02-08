import React from 'react'
import { connect } from 'react-redux'
import Transaction from './Transaction/render'

const HomePage = props => (
    <Transaction />
)

const mapStateToProps = state => {
    //let thisStore = thisStoreFromAppStore(state)
    return {
    }
}

const mapDispatchToProps = dispatch => {
    return {
    }
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(HomePage)
