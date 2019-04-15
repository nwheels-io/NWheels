(function() {
    const pageName = "Seating";
    const comps = [
        {
            html: "<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n    <meta charset=\"UTF-8\">\n    <title>Hall</title>\n    <script\n            src=\"https://code.jquery.com/jquery-3.4.0.min.js\"\n            integrity=\"sha256-BJeo0qm959uMBGb65z40ejJYGSgR7REI4+CW1fNKwOg=\"\n            crossorigin=\"anonymous\"></script>\n    <style>\n        .hall-row {\n            display: flex;\n            flex-direction: row;\n            justify-content: center;\n        }\n\n        .hall-seat {\n            flex: 0 0 40px;\n            height: 40px;\n            text-align: center;\n            display: flex;\n            flex-direction: column;\n            justify-content: center;\n            margin: 3px;\n            border: 1px solid black;\n        }\n\n        .hall-seat.sale {\n            background-color: darkseagreen;\n        }\n        .hall-seat.resale {\n            background-color: goldenrod;\n        }\n        .hall-seat.sold {\n            background-color: indianred;\n        }\n    </style>\n</head>\n<body onLoad=\"ready()\">\n<script>\n    window.onmessage = function(event){\n\n        if (event.data && Array.isArray(event.data)) {\n            var $hall = $('#hall').empty()\n            $.each(event.data, function(rowIndex, row) {\n                var $row = $('<div class=\"hall-row\" />').appendTo($hall);\n                $.each(row.seats, function(seatIndex, seat) {\n                    var $seat = $('<div class=\"hall-seat\" />')\n                        .text('$' + (seat.price || 'N/A'))\n                        .appendTo($row)\n                        .addClass(seat.status)\n                        .on('click', function(e) {\n                            handleSeatClick(rowIndex +1, seatIndex +1, seat);\n                        })\n                });\n            });\n        }\n        else {\n            console.log(\"HTML Code Element received a generic message:\");\n            console.log(event.data);\n        }\n    };\n\n    function handleSeatClick(rowNum, seatNum, seat){\n\n        window.parent.postMessage({\n            \"type\":\"click\",\n            // \"label\":label,\n            \"value\": $.extend({rowNum: rowNum, seatNum: seatNum}, seat)\n        } , \"*\");\n    }\n\n    function ready(){\n        window.parent.postMessage({\"type\":\"ready\"}, \"*\");\n    }\n\n</script>\n\n<div id=\"hall\" class=\"hall\">\n\n</div>\n\n</body>\n</html>",
            compDef: {
                type: "Component",
                componentType: "wysiwyg.viewer.components.HtmlComponent",
                skin: "wysiwyg.viewer.skins.HtmlComponentSkin",
                style: "htco1",
                layout: {
                    width: 980,
                    height: 359,
                    x: 0,
                    y: 0,
                    scale: 1,
                    rotationInDegrees: 0,
                    fixedPosition: false
                },
                connections: {
                    type: "ConnectionList",
                    items: [
                        {
                            type: "WixCodeConnectionItem",
                            role: "html1"
                        }
                    ]
                },
                data: {
                    type: "HtmlComponent",
                    sourceType: "tempUrl",
                    metadata: {
                        schemaVersion: "1.0",
                        isPreset: false,
                        isHidden: false
                    },
                    freezeFrame: false
                },
                props: null,
                activeModes: { }
            }
        },
        {
            html: null,
            compDef: {
                type: "Component",
                componentType: "wysiwyg.viewer.components.WRichText",
                skin: "wysiwyg.viewer.skins.WRichTextNewSkin",
                style: "txtNew",
                layout: {
                    width: 980,
                    height: 31,
                    x: 0,
                    y: 388,
                    scale: 1,
                    rotationInDegrees: 0,
                    fixedPosition: false
                },
                connections: {
                    type: "ConnectionList",
                    items: [
                        {
                            type: "WixCodeConnectionItem",
                            role: "text1"
                        }
                    ]
                },
                data: {
                    type: "StyledText",
                    text: "<p></p>",
                    linkList: [
                        
                    ]
                },
                props: {
                    type: "WRichTextProperties",
                    isHidden: false,
                    brightness: 1,
                    packed: false,
                    metadata: {
                        schemaVersion: "1.0",
                        autoGenerated: false
                    }
                },
                activeModes: { }
            }
        },
        {
            html: null,
            compDef: {
                type: "Component",
                componentType: "wysiwyg.viewer.components.WRichText",
                skin: "wysiwyg.viewer.skins.WRichTextNewSkin",
                style: "txtNew",
                layout: {
                    width: 980,
                    height: 31,
                    x: 0,
                    y: 388,
                    scale: 1,
                    rotationInDegrees: 0,
                    fixedPosition: false
                },
                connections: {
                    type: "ConnectionList",
                    items: [
                        {
                            type: "WixCodeConnectionItem",
                            role: "text1"
                        }
                    ]
                },
                data: {
                    type: "StyledText",
                    text: "<p></p>",
                    linkList: [
                        
                    ]
                },
                props: {
                    type: "WRichTextProperties",
                    isHidden: false,
                    brightness: 1,
                    packed: false,
                    metadata: {
                        schemaVersion: "1.0",
                        autoGenerated: false
                    }
                },
                activeModes: { }
            }
        }
    ];
    const wixCode = `import { fetch } from 'wix-fetch';
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

;
\$w.onReady(() => \$w("#html1").onMessage(event => console.log("got message!", event.data)));

`;
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
    
        console.log('saveHtml request', saveHtmlUrl);
        return fetch(
            saveHtmlUrl,
            {
                "credentials": "include",
                "headers": {
                    "accept": "*/*",
                    "accept-language": "en-US,en;q=0.9,ru;q=0.8,he;q=0.7,es;q=0.6",
                    "content-type": "application/json; charset=utf-8",
                    "x-xsrf-token": getCookie("XSRF-TOKEN")
                },
                "referrer": "https://editor.wix.com/html/editor/web/renderer/edit",
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

    function getCookie(cname) {
        var name = cname + "=";
        var decodedCookie = decodeURIComponent(document.cookie);
        var ca = decodedCookie.split(';');
        for(var i = 0; i <ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) == 0) {
                return c.substring(name.length, c.length);
            }
        }
        return "";
    }

    ;
}());
