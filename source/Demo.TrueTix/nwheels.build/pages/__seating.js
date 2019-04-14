console.log("--- generating page: seating ---");
const pageName = "Seating";
const comps = [
    {
        html: "<SaetingMapHtml />",
        compDef: {
            type: "Component",
            componentType: null,
            skin: null,
            style: null,
            layout: null,
            connections: null,
            data: null,
            props: null,
            activeModes: null
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
                packed: false
            },
            activeModes: null
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
                packed: false
            },
            activeModes: null
        }
    }
];
const pageCorvid = () => 
    $w.onReady(() => 
        $w("#html1").onMessage(event => 
            console.log("got message!", event.data)
        )
    )
;
const getCodeAsString = func => {
    const funcStr = func.toString;
    if (funcStr.indexOf("() =>") === 0) 
    {
        return funcStr.substring(6);
        
    };
    return funcStr;
};
