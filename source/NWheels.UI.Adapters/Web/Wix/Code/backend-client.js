function fetchGraphQL(query) {
    fetch(endpointUrl, {
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
