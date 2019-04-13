import { connect } from 'react-redux';

const initialState = {
    seatingPlan : null,
    selectedSeatId : null,
    selectedSeatInfo : null
};

const Thunks = {
    pageReady({ seatingApi, performanceId }) {
        return (dispatch, getState) => {
            const seatingPlanPromise = seatingApi.getSeatingMap(performanceId);
            seatingPlanPromise.then(data => {
                dispatch({
                    type: 'SEATING_PAGE.SET_SEATING_PLAN',
                    seatingPlan: data   
                });
            });
        };
    },
    seatMapSeatSelected({ seatingApi, performanceId }, seat) {
        return (dispatch, getState) => {
            dispatch({
                type: 'SEATING_PAGE.SET_SELECTED_SEAT_ID',
                selectedSeatId: seat.id   
            });
            dispatch({
                type: 'SEATING_PAGE.SET_SELECTED_SEAT_INFO',
                selectedSeatInfo: null   
            });
            const seatInfoPromise = seatingApi.getSeat(performanceId, seat.id);
            seatInfoPromise.then(data => {
                dispatch({
                    type: 'SEATING_PAGE.SET_SELECTED_SEAT_INFO',
                    selectedSeatInfo: data 
                });
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
        const props = { 
            performanceId: ownProps.performanceId,
            seatingApi: ownProps.seatingApi,
            seatingPlan : ownState.seatingPlan,
            selectedSeatId : ownState.selectedSeatId,
            selectedSeatInfo : ownState.selectedSeatInfo
        };
        return props;
    },
    (dispatch) => {
        return {
            pageReady(props) {
                dispatch(Thunks.pageReady(props));
            },
            seatMapSeatSelected(props, seat) {
                dispatch(Thunks.seatMapSeatSelected(props, seat));
            }
        }
    } 
);
