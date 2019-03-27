export function prepareComponent(component, ID, skin) {
    return {
        Component: component.Connector(ID)(skin),
        Reducers: {
            [ID]: component.Reducer(ID)
        }
    };
};
