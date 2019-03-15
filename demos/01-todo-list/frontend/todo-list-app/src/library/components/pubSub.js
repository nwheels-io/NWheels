import { throws } from "assert";

class Event {
    name;
    listeners;

    constructor(name) {
        this.name = name;
        this.listeners = [];
    }

    addListener(listener) {
        this.listeners.push(listener);
    }

    removeListener(listener) {
        this.listeners = this.listeners.filter(l => l !== listener);
    }

    fire(...eventArgs) {
        this.listeners.map(listener => listener(...eventArgs));
    }
}

export const PubSub = {
    Event
};
