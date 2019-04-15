import { fetch } from 'wix-fetch';
const endpointUrl = "https://api.tixlab.app/api/graphql";
function fetchGraphQL(query) {
    return fetch(endpointUrl, {
        method: "POST",
        mode: "cors",
        body: JSON.stringify({
            query: query,
            variables: null
        }),
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
        }
    })
    .then((httpResponse) => {
        if (httpResponse.ok) {
            return httpResponse.json();
        }
        return Promise.reject("Fetch did not succeed");
    });
}

;
let pageState = { };
const pushStateToComps = () => {
    $w("#SeatPlan").postMessage(pageState.seatingPlan.rows);
    $w("#PerformanceInfo").text = pageState.seatingPlan && pageState.seatingPlan.performance.artist;
    $w("#SeatInfo").text = pageState.selectedSeatInfo && `Selected seat status: ${pageState.selectedSeatInfo.status}, price: \$${pageState.selectedSeatInfo.price || 'N/A'}`;
};
$w.onReady(async () => {
    const res = await fetchGraphQL(
        `query {seatingMap(id:${12345}) {
            performance { artist, date, venue {  title, location }},
            rows {seats { id, status, price }}
        }}`
    );
    console.log('QUERY RESULT > ', res);
    pageState.seatingPlan = res.data.seatingMap;  
    pushStateToComps();
    $w("#SeatPlan").onMessage(async (event) => {
        if (event.data.type === 'click') {
            pageState.selectedSeatInfo = (await fetchGraphQL(
                `query { seat(performanceId:${12345}, seatId:${event.data.value.id}) { price, status } }`
            )).data.seat;
        }
        pushStateToComps();
    });
});

