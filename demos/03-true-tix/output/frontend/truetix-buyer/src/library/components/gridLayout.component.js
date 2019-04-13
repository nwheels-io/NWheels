import { connect } from 'react-redux';

const Reducer = (ID) => (state = { }, action) => {
    switch (action.type) {
        default:
            return state;
    }
};

const Connector = (ID) => (skin) => connect(
    (state, ownProps) => { 
        return { };
    }
)(skin);

export const GridLayout = {
    Connector,
    Reducer
}
