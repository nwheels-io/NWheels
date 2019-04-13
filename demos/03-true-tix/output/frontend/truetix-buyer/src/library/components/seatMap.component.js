import { connect } from 'react-redux';

const Reducer = (ID) => (state = { selectedSeatId: null }, action) => {
    switch (action.type) {
        case 'SEATMAP_SELECT_SEAT':
            return { 
                ...state, 
                selectedSeatId: action.seatId
            };
        default:
            return state;
    }
};

const Connector = (ID) => (skin) => connect(
    (state, ownProps) => { 
        const ownState = state[ID];
        return { 
            rows: ownProps.rows,
            colors: ownProps.colors,
            selectedSeatId: ownState.selectedSeatId
        };
    },
    (dispatch, ownProps) => {
        return {
            selectSeat: (seat) => {
                dispatch({
                    type: 'SEATMAP_SELECT_SEAT',
                    seatId: seat.id
                });
                ownProps.onSeatSelected && ownProps.onSeatSelected(seat);
            }
        };
    }
)(skin);

export const SeatMap = {
    Connector,
    Reducer
}
