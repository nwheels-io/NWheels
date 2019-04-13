import React from "react";
import { createStore, combineReducers, applyMiddleware } from 'redux';
import { Provider } from 'react-redux';
import thunk from 'redux-thunk';
import { prepareComponent, PubSub, DAL, Text, SeatMap } from '../../library/components';
import { DemoMinimal as skin } from '../../library/skins';
import { PageConnector, PageReducer } from './state'
import { createSeatingApi } from '../../backends/seatingApi';

const PerformanceInfoText = prepareComponent(Text, 'performanceInfo', skin.Text);
const SeatMapSeatMap = prepareComponent(SeatMap, 'seatMap', skin.SeatMap);
const SeatInfoText = prepareComponent(Text, 'seatInfo', skin.Text);

class PureBody extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        const GridLayout = skin.GridLayout;
        const GridCell = skin.GridCell;
        const props = this.props;

        return (
            <GridLayout rows={3} cols={3}>
                <GridCell row={0} col={1}>
                    {props.seatingPlan && 
                        <PerformanceInfoText.Component text={props.seatingPlan.performance.artist} />
                    }
                </GridCell>
                <GridCell row={1} col={1}>
                    {props.seatingPlan && 
                        <SeatMapSeatMap.Component 
                            rows={props.seatingPlan.rows} 
                            colors={{'sale':'green', 'resale':'orange', 'sold':'red'}} 
                            onSeatSelected={(seat) => props.seatMapSeatSelected(this.props, seat)} />
                    }
                </GridCell>
                <GridCell row={2} col={1}>
                    {props.selectedSeatInfo && 
                        <SeatInfoText.Component text={`Price: ${props.selectedSeatInfo.price}`}/>
                    }
                </GridCell>
            </GridLayout>
        );
    }
    componentDidMount() {
        this.props.pageReady(this.props);
    }
}

const Body = PageConnector(PureBody);

function buildPageStore() {
    const rootReducer = combineReducers({
        $page: PageReducer,
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
const seatingApi = createSeatingApi('https://api.tixlab.app/api/graphql');

export const SeatingPage = (props) => {
    return (
        <Provider store={store}>
            <Body performanceId={12345} seatingApi={seatingApi} />
        </Provider>
    );
};
