import { throws } from "assert";

function Event(name) {
    
    let listeners = [];

    return {
        addListener(listener) {
            listeners.push(listener);
        },
        removeListener(listener) {
            listeners = listeners.filter(l => l !== listener);
        },
        fire(...eventArgs) {
            listeners.map(listener => listener(...eventArgs));
        }
    };
}

export const PubSub = {
    Event
};
