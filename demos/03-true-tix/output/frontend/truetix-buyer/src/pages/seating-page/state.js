import { connect } from 'react-redux';

const initialState = {
    seatingPlan : null,
    selectedSeatId : null,
    selectedSeatInfo : null
};

const Thunks = {
    pageReady({ seatingApi, performanceId }) {
        return async (dispatch, getState) => {
            const seatingPlan = await seatingApi.getSeatingMap(performanceId);
            dispatch({
                type: 'SEATING_PAGE.SET_SEATING_PLAN',
                seatingPlan   
            });
        };
    },
    seatMapSeatSelected({ seatingApi, performanceId }, seat) {
        return async (dispatch, getState) => {
            dispatch({
                type: 'SEATING_PAGE.SET_SELECTED_SEAT_ID',
                selectedSeatId: seat.id   
            });
            dispatch({
                type: 'SEATING_PAGE.SET_SELECTED_SEAT_INFO',
                selectedSeatInfo: null   
            });
            const selectedSeatInfo = await seatingApi.getSeat(performanceId, seat.id);
            dispatch({
                type: 'SEATING_PAGE.SET_SELECTED_SEAT_INFO',
                selectedSeatInfo   
            });
        };
    }
};

export const PageReducer = (state = initialState, action) => {
    switch (action.type) {
        case 'SEATING_PAGE.SET_SEATING_PLAN':
            return { 
                ...state, 
                seatingPlan: action.seatingPlan
            };
        case 'SEATING_PAGE.SET_SELECTED_SEAT_ID':
            return { 
                ...state, 
                selectedSeatId: action.selectedSeatId
            };
        case 'SEATING_PAGE.SET_SELECTED_SEAT_INFO':
            return { 
                ...state, 
                selectedSeatInfo: action.selectedSeatInfo
            };
        default:
            return state;
    }
};

export const PageConnector = connect(
    (state, ownProps) => { 
        const ownState = state.$page;
        return { 
            performanceId: ownProps.performanceId,
            seatingApi: ownProps.seatingApi,
            seatingPlan : ownState.seatingPlan,
            selectedSeatId : ownState.selectedSeatId,
            selectedSeatInfo : ownState.selectedSeatInfo
        };
    },
    (dispatch) => Thunks
);
