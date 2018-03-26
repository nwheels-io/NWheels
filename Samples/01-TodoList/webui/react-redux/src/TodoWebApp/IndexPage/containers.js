import { connect } from 'react-redux'
import { Toolbar_Show_Select } from 'actions'
import IndexPage from 'presentation'

const mapStateToProps = (state, ownProps) => {
    return {
        isSelected: ownProps.Toolbar.selectedIndex === state.visibilityFilter
    }
}

const mapDispatchToProps = (dispatch, ownProps) => {
    return {
        onClick: () => {
            dispatch(setVisibilityFilter(ownProps.filter))
        }
    }
}

const FilterLink = connect(
    mapStateToProps,
    mapDispatchToProps
)(Link)

export default FilterLink