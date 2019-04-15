    //------------------------------------------------------------------------------------
    // UPLOAD THROUGH DOCUMENT SERVICES (temporary solution)
    //------------------------------------------------------------------------------------
    
    const ds = document.querySelector('#preview').contentWindow.documentServices;
    const steps = [];
    
    // create page
    steps.push(() => {
        const defer = Promise.defer();
        const pageRef = ds.pages.add(pageName);
        ds.pages.navigateTo(pageRef.id, () => {
            defer.resolve()
        });
        return defer.promise
    });
    
    // create comps
    comps.forEach(comp => {
        steps.push(() => {
            if (comp.html) {
                return saveHtml(comp.html)
                    .then((tempUrl) => {
                        console.log('saved html temp url', tempUrl);
                        _.set(comp.compDef, 'data.url', tempUrl);
                        addComponent(comp.compDef)
                    })
            } else {
                addComponent(comp.compDef)
            }
        })
    });
    
    // create wixCode
    steps.push(() => {
        return saveWixCode(wixCode);
    });
    
    steps.reduce((promise, action) => promise.then(action), Promise.resolve())
        .then(() => window.alert('Wix as a Service is Cool! (like we are)'));
    
    // SDK
    function addComponent(compDef) {
        console.log('addComponent', compDef);
        ds.components.add(ds.pages.getCurrentPage(), compDef);
    }
    
    function saveHtml(htmlText) {
        const SAVE_URL_TEMPLATE = '<%= editorServerRoot %>/api/html/save_temp?metaSiteId=<%= metaSiteId %>&editorSessionId=<%= editorSessionId %>';
        const saveHtmlUrl = _.template(SAVE_URL_TEMPLATE)({
            editorServerRoot: window.serviceTopology.editorServerRoot,
            metaSiteId: ds.generalInfo.getMetaSiteId(),
            editorSessionId: getQueryParam('editorSessionId')
        });
    
        console.log('saveHtml request', saveHtmlUrl)
        return fetch(
            saveHtmlUrl,
            {
                "credentials": "include",
                "headers": {
                    "accept": "*/*",
                    "accept-language": "en-US,en;q=0.9,ru;q=0.8,he;q=0.7,es;q=0.6",
                    "content-type": "application/json; charset=utf-8",
                    "x-xsrf-token": "1554886602|Bz-PyXsapNGf"
                },
                "referrer": "https://editor.wix.com/html/editor/web/renderer/edit/b8674fbf-81de-419a-afe8-39e123201dda?metaSiteId=69721ac4-1ce3-40ff-806c-5ce7f6fb9761",
                "referrerPolicy": "no-referrer-when-downgrade",
                "body": JSON.stringify({html: htmlText}),
                "method": "POST",
                "mode": "cors"
            })
            .then(response => {
                if (!response.ok) {
                    return Promise.reject("unable to save html");
                }
                return response.json();
            })
            .then(json => {
                console.log('saveHtml response', json);
                return json.payload;
            });
    }
    
    function saveWixCode(wixCodeText) {
        const file = fileDesc(ds.wixCode.pages.getFileId(ds.pages.getCurrentPageId()));
        const fs = ds.wixCode.fileSystem;
        console.log('saveWixCode file', file);
    
        return fs.writeFile(file, wixCodeText)
            .then(() => fs.flush())
            .then(() => console.log('saveWixCode compete'))
    }
    
    function fileDesc(fn) {
        return {
            virtual: true,
            localTimeStamp: 0,
            eTag: "\"virtual\"",
            name: fn.split('/').pop(),
            length: 0,
            directory: false,
            location: fn,
            attributes: {readOnly: false}
        }
    }
    
    function getQueryParam(name) {
        return window.location.search.substring(1).split('&').map(q => q.split('=')).find(pp => pp[0] === name)[1]
    }
