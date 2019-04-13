import { DAL } from '../library/components/dal';

export function createSeatingApi(backendUrl) {

    const client = DAL.GraphQL(backendUrl);

    return {
        getSeatingMap(performanceId) {
            const queryText = 
                `query {seatingMap(id:${performanceId}) {
                    performance { artist, date, venue {  title, location }},
                    rows {seats { id, status, price }}
                }}`;
            
            return client.sendRequest(queryText).then(json => {
                return json.data.seatingMap;
            });
        },

        getSeat(performanceId, seatId) {
            const queryText = 
                `query {seat(performanceId:${performanceId}, seatId:${seatId}) {
                    status, price, seller {name, avatar, remarks}
                }}`;
            
            return client.sendRequest(queryText).then(json => {
                return json.data.seat;
            });
        }
    };
}