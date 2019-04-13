import { connect } from 'react-redux';

const NoopReducer = (ID) => (state = { }, action) => {
    switch (action.type) {
        default:
            return state;
    }
};

const GridLayoutConnector = (ID) => (skin) => connect(
    (state, ownProps) => { 
        return { 
            cols: ownProps.cols,
            rows: ownProps.rows
        };
    },
    (dispatch) => {
        return {}
    }
)(skin);

const GridCellConnector = (ID) => (skin) => connect(
    (state, ownProps) => {
        return {
            row: ownProps.row,
            col: ownProps.col
        }
    },
    (dispatch) => {
        return {}
    }
);

export const GridLayout = {
    Connector: GridLayoutConnector,
    Reducer: NoopReducer
}

export const GridCell = {
    Connector: GridCellConnector,
    Reducer: NoopReducer
}
