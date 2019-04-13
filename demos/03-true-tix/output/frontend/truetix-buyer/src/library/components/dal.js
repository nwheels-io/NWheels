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

    const defaultGqlStringifyOptions = {
        formatParens: (path, paren) => paren,
        formatValue: () => undefined
    };

    function gqlStringify(value, options = defaultGqlStringifyOptions, path = '') {
        const type = (Array.isArray(value) ? 'array' : typeof value);
        const formattedValue = options.formatValue(path, type, value);
        if (formattedValue) {
            return formattedValue;
        }

        let items, openParen, closeParen;

        switch (type) {
            case 'undefined':
                return 'undefined';
            case 'object':
                items = [];
                for (let key in value) {
                    if (typeof value[key] !== 'undefined') {
                        items.push(`${key}: ${gqlStringify(value[key], options, `${path}/${key}`)}`)
                    }
                }
                openParen = options.formatParens(path, '{');
                closeParen = options.formatParens(path, '}');
                return `${openParen} ${items.join(', ')} ${closeParen}`;
            case 'array':
                items = [];
                for (let element of value) {
                    items.push(gqlStringify(element, options, `${path}[]`));
                }
                openParen = options.formatParens(path, '[');
                closeParen = options.formatParens(path, ']');
                return `${openParen} ${items.join(', ')} ${closeParen}`;
            default:
                return JSON.stringify(value);
        }
    }

    function objectMap(obj, mapFn) {
        return Object.keys(obj).reduce(function(result, key) {
          result[key] = mapFn(obj[key]);
          return result;
        }, {});
    }

    return {

        create(dataInitProps) {
            const mutationText = `mutation { create(data: ${gqlStringify(dataInitProps)}) { id } }`;

            const promise = sendRequest(mutationText).then(json => {
                return json.data.create;
            });

            return promise;
        },

        retrieve(select, where, orderBy) {
            const whereFields = Object.keys(where || {}).filter(f => typeof where[f] !== 'undefined');
            const orderFields = Object.keys(orderBy || {}).filter(f => typeof orderBy[f] !== 'undefined');
            const queryArgsStringifyOptions = {
                formatParens: (path, paren) => {
                    if (path === '') {
                        return (paren === '{' ? '(' : (paren === '}' ? ')' : paren));
                    }
                    return paren;
                },
                formatValue: (path, type, value) => {
                    if (path.indexOf('/orderBy/') === 0 && type === 'string') {
                        return value;
                    }
                    return undefined;
                }
            };

            let queryArgs = {
                ...(whereFields.length > 0 ? where : {}), 
                orderBy: (orderFields.length > 0 
                    ? objectMap(orderBy, ascending => ascending ? 'ASC' : 'DESC')
                    : undefined
                )
            };
            const argsText = (
                whereFields.length > 0 || orderFields.length > 0
                ? gqlStringify(queryArgs, queryArgsStringifyOptions) 
                : ''
            );
            const selectText = select.join(',');
            const queryText = `{ fetch ${argsText} { ${selectText} } }`;

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
        },

        sendRequest
    };
}

export const DAL = {
    GraphQL
};
