import { ninvoke } from "q";

function GraphQL(url) {

    function sendRequest(queryText, variables = null) {
        const promise = fetch(url, {
            method: 'POST',
            mode: 'cors',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
            },
            body: JSON.stringify({
                query: queryText,
                variables
            })
        })
        .then(result => {
            return result.json();
        });

        return promise;
    }

    const defaultGqlStringifyBrackets = {
        openObj: '{',
        closeObj: '}'
    };

    function gqlStringify(value, brackets = defaultGqlStringifyBrackets) {
        const type = (Array.isArray(value) ? 'array' : typeof value);
        let items;

        switch (type) {
            case 'undefined':
                return 'undefined';
            case 'object':
                items = [];
                for (let key in value) {
                    items.push(`${key}: ${gqlStringify(value[key])}`)
                }
                return `${brackets.openObj} ${items.join(', ')} ${brackets.closeObj}`;
            case 'array':
                items = [];
                for (let element of value) {
                    items.push(gqlStringify(element));
                }
                return `[ ${items.join(', ')} ]`;
            default:
                return JSON.stringify(value);
        }
    }

    return {

        create(dataInitProps) {
            const mutationText = `mutation { create(data: ${gqlStringify(dataInitProps)}) { id } }`;

            const promise = sendRequest(mutationText).then(json => {
                return json.data.create;
            });

            return promise;
        },

        retrieve(select, where) {
            const filterText = (
                where 
                ? gqlStringify(where, {openObj: '(', closeObj: ')'}) 
                : ''
            );
            const selectText = select.join(',');
            const queryText = `{ fetch ${filterText} { ${selectText} } }`;

            const promise = sendRequest(queryText).then(json => {
                return json.data.fetch;
            });

            return promise;
        },

        update(key, dataChangeProps) {
            const changePropsWithKey = {...dataChangeProps, id: key};
            const mutationText = `mutation { update(data: ${gqlStringify(changePropsWithKey)}) { id } }`;

            const promise = sendRequest(mutationText).then(json => {
                return json.data.update;
            });

            return promise;
        },

        delete(key) {
            const mutationText = `mutation { delete(id: ${key}) { id } }`;
            const promise = sendRequest(mutationText).then(json => {
                return json.data.update;
            });
            return promise;
        }
    };
}

export const DAL = {
    GraphQL
};
