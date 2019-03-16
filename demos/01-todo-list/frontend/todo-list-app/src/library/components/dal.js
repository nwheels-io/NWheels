import { ninvoke } from "q";

function GraphQL(entity, url) {

    return {

        retrieve(select, where) {
            const filterText = (
                where 
                ? JSON.stringify(where).replace(/^{/,'(').replace(/}$/,')') 
                : ''
            );
            const selectText = select.join(',');
            const queryText = `{ ${entity}${filterText} { ${selectText} } }`;

            const promise = fetch(url, {
                method: 'POST',
                mode: 'cors',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json',
                },
                body: JSON.stringify({
                    query: queryText,
                    variables: null
                })
            })
            .then(result => {
                return result.json();
            })
            .then(json => {
                return json.data[entity];
            });

            return promise;
        }

    };
}

export const DAL = {
    GraphQL
};
