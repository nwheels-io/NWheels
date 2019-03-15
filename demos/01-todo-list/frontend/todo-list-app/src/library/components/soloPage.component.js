import { connect } from 'react-redux';

const Reducer = (state = {}, action) => {
    return state;
}

const Connector = (ID) => (skin) => connect(
    (state) => ({}),
    (dispatch) => ({})
)(skin);

export const SoloPage = {
    Connector,
    Reducer
};
