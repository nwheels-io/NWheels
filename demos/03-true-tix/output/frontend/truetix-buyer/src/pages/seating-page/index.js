import React from "react";
import { createStore, combineReducers, applyMiddleware } from 'redux';
import { Provider } from 'react-redux';
import thunk from 'redux-thunk';
import { prepareComponent, PubSub, DAL, GridLayout, Text, SeatMap } from '../../library/components';
import { DemoMinimal as skin } from '../../library/skins';
import { PageConnector, PageReducer } from './state'

const PageLayoutGrid = prepareComponent(GridLayout, 'pageLayout', skin.GridLayout);
const PerformanceInfoText = prepareComponent(Text, 'performanceInfo', skin.Text);
const SeatMapSeatMap = prepareComponent(SeatMap, 'seatMap', skin.SeatMap);
const SeatInfoText = prepareComponent(Text, 'seatInfo', skin.Text);

function buildPageStore() {
    const rootReducer = combineReducers({
        ...PerformanceInfoText.Reducers,
        ...SeatInfoText.Reducers,
        ...SeatMapSeatMap.Reducers
    });
    return createStore(
        rootReducer,
        applyMiddleware(thunk)
    );
}

const store = buildPageStore();

export const SeatingPage = (props) => {
    const onSeatMapSeatSelected = PubSub.Event('seatingPage/seatMap/seatSelected');
    const Body = PageConnector(PureBody);

    return (
        <Provider store={store}>
            <Body />
        </Provider>
    );
};

const PureBody = (props) => {
    const PageLayoutGridCell = PageLayoutGrid.getCellComponent();
    return (
        <PageLayoutGrid rows={3} cols={3}>
            <PageLayoutGridCell row={0} col={1}>
                <PerformanceInfoText text={props.performance.title} />
            </PageLayoutGridCell>
            <PageLayoutGridCell row={1} col={1}>
                <SeatMapSeatMap rows={props.seatingPlan.rows} colors={{'sale':'green', 'resale':'orange', 'sold':'red'}} />
            </PageLayoutGridCell>
            <PageLayoutGridCell row={2} col={1}>
                <SeatInfoText text={props.selectedSeatInfo.title}/>
            </PageLayoutGridCell>
        </PageLayoutGrid>
    );
}

